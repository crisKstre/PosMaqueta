using System;
using Microsoft.Data.Sqlite;
using Entidades;

namespace AccesoData
{
    public class DatabaseInitializer : ConexionSqlite
    {
        public void Inicializar()
        {
            using (var con = GetConnection())
            {
                con.Open();
                CrearTablas(con);
                MigrarEsquema(con);
                SembrarAdmin(con);
                SembrarEmpleadoDemo(con);
                SincronizarCategorias(con);
            }
        }

        private void CrearTablas(SqliteConnection con)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Categoria (
                    IdCategoria INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre      TEXT    NOT NULL UNIQUE
                );

                CREATE TABLE IF NOT EXISTS Usuario (
                    IdUsuario   INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre      TEXT    NOT NULL,
                    LoginNombre TEXT    NOT NULL UNIQUE,
                    Pass        TEXT    NOT NULL,
                    Rol         TEXT    NOT NULL,
                    Activo      INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Producto (
                    IdProducto   INTEGER PRIMARY KEY AUTOINCREMENT,
                    CodigoBarras TEXT    UNIQUE,
                    Nombre       TEXT    NOT NULL,
                    Categoria    TEXT,
                    Precio       REAL    NOT NULL DEFAULT 0,
                    Stock        REAL    NOT NULL DEFAULT 0,
                    StockMinimo  REAL    NOT NULL DEFAULT 0,
                    UnidadMedida TEXT    NOT NULL DEFAULT 'Unidad',
                    Activo       INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Caja (
                    IdCaja        INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdUsuario     INTEGER NOT NULL,
                    FechaApertura TEXT    NOT NULL,
                    FechaCierre   TEXT,
                    MontoInicial  REAL    NOT NULL DEFAULT 0,
                    MontoEsperado REAL    NOT NULL DEFAULT 0,
                    MontoReal     REAL    NOT NULL DEFAULT 0,
                    Estado        TEXT    NOT NULL,
                    FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
                );

                CREATE TABLE IF NOT EXISTS Venta (
                    IdVenta   INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdCaja    INTEGER,
                    IdUsuario INTEGER NOT NULL,
                    Fecha     TEXT    NOT NULL,
                    Total     REAL    NOT NULL DEFAULT 0,
                    Descuento REAL    NOT NULL DEFAULT 0,
                    MedioPago TEXT,
                    FOREIGN KEY (IdCaja)    REFERENCES Caja(IdCaja),
                    FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
                );

                CREATE TABLE IF NOT EXISTS LogMovimiento (
                    IdLog       INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha       TEXT    NOT NULL,
                    IdUsuario   INTEGER NOT NULL,
                    NombreUsuario TEXT  NOT NULL,
                    Modulo      TEXT    NOT NULL,
                    Accion      TEXT    NOT NULL,
                    Detalle     TEXT,
                    FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
                );

                CREATE TABLE IF NOT EXISTS DetalleVenta (
                    IdDetalle      INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdVenta        INTEGER NOT NULL,
                    IdProducto     INTEGER NOT NULL,
                    Cantidad       REAL    NOT NULL DEFAULT 1,
                    PrecioUnitario REAL    NOT NULL DEFAULT 0,
                    Subtotal       REAL    NOT NULL DEFAULT 0,
                    FOREIGN KEY (IdVenta)    REFERENCES Venta(IdVenta),
                    FOREIGN KEY (IdProducto) REFERENCES Producto(IdProducto)
                );";

            using (var cmd = new SqliteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }
        }

        // Migraciones incrementales: agrega columnas nuevas a bases de datos ya creadas.
        private void MigrarEsquema(SqliteConnection con)
        {
            if (!ColumnaExiste(con, "Venta", "Anulada"))
                using (var cmd = new SqliteCommand(
                    "ALTER TABLE Venta ADD COLUMN Anulada INTEGER NOT NULL DEFAULT 0;", con))
                    cmd.ExecuteNonQuery();

            if (!ColumnaExiste(con, "Venta", "Descuento"))
                using (var cmd = new SqliteCommand(
                    "ALTER TABLE Venta ADD COLUMN Descuento REAL NOT NULL DEFAULT 0;", con))
                    cmd.ExecuteNonQuery();
        }

        private bool ColumnaExiste(SqliteConnection con, string tabla, string columna)
        {
            using (var cmd = new SqliteCommand("PRAGMA table_info(" + tabla + ");", con))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    if (string.Equals(reader.GetString(1), columna, StringComparison.OrdinalIgnoreCase))
                        return true;
            return false;
        }

        private void SembrarAdmin(SqliteConnection con)
        {
            using (var cmdCount = new SqliteCommand("SELECT COUNT(*) FROM Usuario;", con))
            {
                long total = Convert.ToInt64(cmdCount.ExecuteScalar());
                if (total > 0) return;
            }

            string insert = @"
                INSERT INTO Usuario (Nombre, LoginNombre, Pass, Rol, Activo)
                VALUES (@nombre, @login, @pass, @rol, 1);";

            using (var cmd = new SqliteCommand(insert, con))
            {
                cmd.Parameters.AddWithValue("@nombre", "Administrador");
                cmd.Parameters.AddWithValue("@login", "admin");
                cmd.Parameters.AddWithValue("@pass", Seguridad.Hash("admin123"));
                cmd.Parameters.AddWithValue("@rol", RolUsuario.Admin);
                cmd.ExecuteNonQuery();
            }

            // Categorías de ejemplo
            string[] cats = { "Abarrotes", "Bebidas", "Lácteos", "Panadería", "Limpieza", "Otros" };
            foreach (var c in cats)
            {
                using (var cmd = new SqliteCommand(
                    "INSERT OR IGNORE INTO Categoria (Nombre) VALUES (@n);", con))
                {
                    cmd.Parameters.AddWithValue("@n", c);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Empleado de prueba (rol Cajero). Idempotente: solo se crea si aún no existe,
        // así también aparece en bases de datos ya creadas.
        private void SembrarEmpleadoDemo(SqliteConnection con)
        {
            using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Usuario WHERE LoginNombre = @login;", con))
            {
                cmd.Parameters.AddWithValue("@login", "empleado");
                if (Convert.ToInt64(cmd.ExecuteScalar()) > 0) return;
            }

            using (var cmd = new SqliteCommand(@"
                INSERT INTO Usuario (Nombre, LoginNombre, Pass, Rol, Activo)
                VALUES (@nombre, @login, @pass, @rol, 1);", con))
            {
                cmd.Parameters.AddWithValue("@nombre", "Empleado Demo");
                cmd.Parameters.AddWithValue("@login", "empleado");
                cmd.Parameters.AddWithValue("@pass", Seguridad.Hash("empleado123"));
                cmd.Parameters.AddWithValue("@rol", RolUsuario.Cajero);
                cmd.ExecuteNonQuery();
            }
        }

        // Asegura que toda categoría usada por algún producto exista en la lista de Categorías,
        // para que aparezca como filtro en Ventas (útil tras importar o sembrar productos).
        private void SincronizarCategorias(SqliteConnection con)
        {
            using (var cmd = new SqliteCommand(@"
                INSERT OR IGNORE INTO Categoria (Nombre)
                SELECT DISTINCT Categoria FROM Producto
                WHERE Categoria IS NOT NULL AND TRIM(Categoria) <> '';", con))
                cmd.ExecuteNonQuery();
        }
    }
}

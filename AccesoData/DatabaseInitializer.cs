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
                SembrarAdmin(con);
            }
        }

        private void CrearTablas(SqliteConnection con)
        {
            string sql = @"
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
                    MedioPago TEXT,
                    FOREIGN KEY (IdCaja)    REFERENCES Caja(IdCaja),
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

        private void SembrarAdmin(SqliteConnection con)
        {
            // Solo crea el admin si la tabla Usuario está vacía
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
        }
    }
}

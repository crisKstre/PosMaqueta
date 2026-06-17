using System;
using System.Data.Common;
using System.Data.SqlClient;
using Entidades;

namespace AccesoData
{
    /// <summary>
    /// Crea y migra el esquema. Soporta SQLite (local) y SQL Server (central).
    /// Las fechas se guardan como texto y los montos como DECIMAL en ambos motores,
    /// de modo que el código de los DAOs es el mismo para los dos.
    /// </summary>
    public class DatabaseInitializer : ConexionBD
    {
        public void Inicializar()
        {
            if (ConfigBD.Proveedor == ProveedorBD.SqlServer)
                AsegurarBaseSqlServer();   // crea la base de datos si no existe

            using (var con = GetConnection())
            {
                con.Open();

                if (ConfigBD.Proveedor == ProveedorBD.Sqlite)
                    using (var pragma = con.Comando("PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;"))
                        pragma.ExecuteNonQuery();

                CrearTablas(con);
                MigrarEsquema(con);
                CrearIndices(con);
                SembrarAdmin(con);
                SembrarEmpleadoDemo(con);
                SincronizarCategorias(con);
            }
        }

        // Conecta a 'master' y crea la base de datos de la cadena si aún no existe.
        private void AsegurarBaseSqlServer()
        {
            var sb = new SqlConnectionStringBuilder(ConfigBD.CadenaConexion);
            string bd = sb.InitialCatalog;
            if (string.IsNullOrEmpty(bd)) return;
            sb.InitialCatalog = "master";
            using (var con = new SqlConnection(sb.ConnectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "IF DB_ID(@db) IS NULL CREATE DATABASE [" + bd.Replace("]", "]]") + "];";
                    var p = cmd.CreateParameter(); p.ParameterName = "@db"; p.Value = bd; cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CrearTablas(DbConnection con)
        {
            string sql = ConfigBD.Proveedor == ProveedorBD.SqlServer ? DDL_SqlServer : DDL_Sqlite;
            using (var cmd = con.Comando(sql))
                cmd.ExecuteNonQuery();
        }

        private const string DDL_Sqlite = @"
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
                Activo       INTEGER NOT NULL DEFAULT 1,
                DescuentoPorcentaje REAL NOT NULL DEFAULT 0
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
                PrecioOriginal REAL    NOT NULL DEFAULT 0,
                DescuentoPorcentaje REAL NOT NULL DEFAULT 0,
                Subtotal       REAL    NOT NULL DEFAULT 0,
                FOREIGN KEY (IdVenta)    REFERENCES Venta(IdVenta),
                FOREIGN KEY (IdProducto) REFERENCES Producto(IdProducto)
            );";

        // Esquema completo (incluye Anulada). Orden respetando las claves foráneas.
        private const string DDL_SqlServer = @"
            IF OBJECT_ID(N'dbo.Categoria','U') IS NULL
            CREATE TABLE Categoria (
                IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
                Nombre      NVARCHAR(100) NOT NULL UNIQUE
            );
            IF OBJECT_ID(N'dbo.Usuario','U') IS NULL
            CREATE TABLE Usuario (
                IdUsuario   INT IDENTITY(1,1) PRIMARY KEY,
                Nombre      NVARCHAR(100) NOT NULL,
                LoginNombre NVARCHAR(50)  NOT NULL UNIQUE,
                Pass        NVARCHAR(200) NOT NULL,
                Rol         NVARCHAR(20)  NOT NULL,
                Activo      INT NOT NULL DEFAULT 1
            );
            IF OBJECT_ID(N'dbo.Producto','U') IS NULL
            CREATE TABLE Producto (
                IdProducto   INT IDENTITY(1,1) PRIMARY KEY,
                CodigoBarras NVARCHAR(50) NULL,
                Nombre       NVARCHAR(150) NOT NULL,
                Categoria    NVARCHAR(100) NULL,
                Precio       DECIMAL(18,4) NOT NULL DEFAULT 0,
                Stock        DECIMAL(18,4) NOT NULL DEFAULT 0,
                StockMinimo  DECIMAL(18,4) NOT NULL DEFAULT 0,
                UnidadMedida NVARCHAR(20)  NOT NULL DEFAULT 'Unidad',
                Activo       INT NOT NULL DEFAULT 1,
                DescuentoPorcentaje DECIMAL(18,4) NOT NULL DEFAULT 0
            );
            IF OBJECT_ID(N'dbo.Caja','U') IS NULL
            CREATE TABLE Caja (
                IdCaja        INT IDENTITY(1,1) PRIMARY KEY,
                IdUsuario     INT NOT NULL,
                FechaApertura NVARCHAR(19) NOT NULL,
                FechaCierre   NVARCHAR(19) NULL,
                MontoInicial  DECIMAL(18,4) NOT NULL DEFAULT 0,
                MontoEsperado DECIMAL(18,4) NOT NULL DEFAULT 0,
                MontoReal     DECIMAL(18,4) NOT NULL DEFAULT 0,
                Estado        NVARCHAR(20) NOT NULL,
                CONSTRAINT FK_Caja_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
            );
            IF OBJECT_ID(N'dbo.Venta','U') IS NULL
            CREATE TABLE Venta (
                IdVenta   INT IDENTITY(1,1) PRIMARY KEY,
                IdCaja    INT NULL,
                IdUsuario INT NOT NULL,
                Fecha     NVARCHAR(19) NOT NULL,
                Total     DECIMAL(18,4) NOT NULL DEFAULT 0,
                Descuento DECIMAL(18,4) NOT NULL DEFAULT 0,
                MedioPago NVARCHAR(20) NULL,
                Anulada   INT NOT NULL DEFAULT 0,
                CONSTRAINT FK_Venta_Caja    FOREIGN KEY (IdCaja)    REFERENCES Caja(IdCaja),
                CONSTRAINT FK_Venta_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
            );
            IF OBJECT_ID(N'dbo.LogMovimiento','U') IS NULL
            CREATE TABLE LogMovimiento (
                IdLog         INT IDENTITY(1,1) PRIMARY KEY,
                Fecha         NVARCHAR(19) NOT NULL,
                IdUsuario     INT NOT NULL,
                NombreUsuario NVARCHAR(100) NOT NULL,
                Modulo        NVARCHAR(50) NOT NULL,
                Accion        NVARCHAR(50) NOT NULL,
                Detalle       NVARCHAR(500) NULL,
                CONSTRAINT FK_Log_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
            );
            IF OBJECT_ID(N'dbo.DetalleVenta','U') IS NULL
            CREATE TABLE DetalleVenta (
                IdDetalle      INT IDENTITY(1,1) PRIMARY KEY,
                IdVenta        INT NOT NULL,
                IdProducto     INT NOT NULL,
                Cantidad       DECIMAL(18,4) NOT NULL DEFAULT 1,
                PrecioUnitario DECIMAL(18,4) NOT NULL DEFAULT 0,
                PrecioOriginal DECIMAL(18,4) NOT NULL DEFAULT 0,
                DescuentoPorcentaje DECIMAL(18,4) NOT NULL DEFAULT 0,
                Subtotal       DECIMAL(18,4) NOT NULL DEFAULT 0,
                CONSTRAINT FK_Detalle_Venta    FOREIGN KEY (IdVenta)    REFERENCES Venta(IdVenta),
                CONSTRAINT FK_Detalle_Producto FOREIGN KEY (IdProducto) REFERENCES Producto(IdProducto)
            );";

        // Migraciones incrementales (solo afectan a SQLite: el esquema de SQL Server ya está completo,
        // ColumnaExiste devuelve true y no ejecuta los ALTER).
        private void MigrarEsquema(DbConnection con)
        {
            if (!ColumnaExiste(con, "Venta", "Anulada"))
                Ejecutar(con, "ALTER TABLE Venta ADD COLUMN Anulada INTEGER NOT NULL DEFAULT 0;");
            if (!ColumnaExiste(con, "Venta", "Descuento"))
                Ejecutar(con, "ALTER TABLE Venta ADD COLUMN Descuento REAL NOT NULL DEFAULT 0;");
            if (!ColumnaExiste(con, "Producto", "DescuentoPorcentaje"))
                Ejecutar(con, "ALTER TABLE Producto ADD COLUMN DescuentoPorcentaje REAL NOT NULL DEFAULT 0;");
            if (!ColumnaExiste(con, "DetalleVenta", "PrecioOriginal"))
                Ejecutar(con, "ALTER TABLE DetalleVenta ADD COLUMN PrecioOriginal REAL NOT NULL DEFAULT 0;");
            if (!ColumnaExiste(con, "DetalleVenta", "DescuentoPorcentaje"))
                Ejecutar(con, "ALTER TABLE DetalleVenta ADD COLUMN DescuentoPorcentaje REAL NOT NULL DEFAULT 0;");
        }

        private bool ColumnaExiste(DbConnection con, string tabla, string columna)
        {
            if (ConfigBD.Proveedor == ProveedorBD.SqlServer)
            {
                using (var cmd = con.Comando("SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(@t) AND name = @c;"))
                {
                    cmd.AddParam("@t", tabla);
                    cmd.AddParam("@c", columna);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                }
            }
            using (var cmd = con.Comando("PRAGMA table_info(" + tabla + ");"))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    if (string.Equals(reader.GetString(1), columna, StringComparison.OrdinalIgnoreCase))
                        return true;
            return false;
        }

        private void CrearIndices(DbConnection con)
        {
            string sql = ConfigBD.Proveedor == ProveedorBD.SqlServer
                ? @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_venta_fecha')        CREATE INDEX idx_venta_fecha        ON Venta(Fecha);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_venta_caja')         CREATE INDEX idx_venta_caja         ON Venta(IdCaja);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_detalle_venta')      CREATE INDEX idx_detalle_venta      ON DetalleVenta(IdVenta);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_detalle_producto')   CREATE INDEX idx_detalle_producto   ON DetalleVenta(IdProducto);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_producto_categoria') CREATE INDEX idx_producto_categoria ON Producto(Categoria);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_log_fecha')          CREATE INDEX idx_log_fecha          ON LogMovimiento(Fecha);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='ux_producto_codigo')     CREATE UNIQUE INDEX ux_producto_codigo ON Producto(CodigoBarras) WHERE CodigoBarras IS NOT NULL;"
                : @"CREATE INDEX IF NOT EXISTS idx_venta_fecha        ON Venta(Fecha);
                    CREATE INDEX IF NOT EXISTS idx_venta_caja         ON Venta(IdCaja);
                    CREATE INDEX IF NOT EXISTS idx_detalle_venta      ON DetalleVenta(IdVenta);
                    CREATE INDEX IF NOT EXISTS idx_detalle_producto   ON DetalleVenta(IdProducto);
                    CREATE INDEX IF NOT EXISTS idx_producto_categoria ON Producto(Categoria);
                    CREATE INDEX IF NOT EXISTS idx_log_fecha          ON LogMovimiento(Fecha);";
            using (var cmd = con.Comando(sql))
                cmd.ExecuteNonQuery();
        }

        private void SembrarAdmin(DbConnection con)
        {
            using (var cmd = con.Comando("SELECT COUNT(*) FROM Usuario;"))
                if (Convert.ToInt64(cmd.ExecuteScalar()) > 0) return;

            using (var cmd = con.Comando(@"
                INSERT INTO Usuario (Nombre, LoginNombre, Pass, Rol, Activo)
                VALUES (@nombre, @login, @pass, @rol, 1);"))
            {
                cmd.AddParam("@nombre", "Administrador");
                cmd.AddParam("@login", "admin");
                cmd.AddParam("@pass", Seguridad.Hash("admin123"));
                cmd.AddParam("@rol", RolUsuario.Admin);
                cmd.ExecuteNonQuery();
            }

            string[] cats = { "Abarrotes", "Bebidas", "Lácteos", "Panadería", "Limpieza", "Otros" };
            foreach (var c in cats) InsertarCategoriaSiFalta(con, c);
        }

        // Empleado de prueba (rol Cajero). Idempotente: solo se crea si aún no existe.
        private void SembrarEmpleadoDemo(DbConnection con)
        {
            using (var cmd = con.Comando("SELECT COUNT(*) FROM Usuario WHERE LoginNombre = @login;"))
            {
                cmd.AddParam("@login", "empleado");
                if (Convert.ToInt64(cmd.ExecuteScalar()) > 0) return;
            }

            using (var cmd = con.Comando(@"
                INSERT INTO Usuario (Nombre, LoginNombre, Pass, Rol, Activo)
                VALUES (@nombre, @login, @pass, @rol, 1);"))
            {
                cmd.AddParam("@nombre", "Empleado Demo");
                cmd.AddParam("@login", "empleado");
                cmd.AddParam("@pass", Seguridad.Hash("empleado123"));
                cmd.AddParam("@rol", RolUsuario.Cajero);
                cmd.ExecuteNonQuery();
            }
        }

        // Asegura que toda categoría usada por algún producto exista en la lista de Categorías.
        private void SincronizarCategorias(DbConnection con)
        {
            string sql = ConfigBD.Proveedor == ProveedorBD.SqlServer
                ? @"INSERT INTO Categoria (Nombre)
                    SELECT DISTINCT p.Categoria FROM Producto p
                    WHERE p.Categoria IS NOT NULL AND LTRIM(RTRIM(p.Categoria)) <> ''
                      AND NOT EXISTS (SELECT 1 FROM Categoria c WHERE c.Nombre = p.Categoria);"
                : @"INSERT OR IGNORE INTO Categoria (Nombre)
                    SELECT DISTINCT Categoria FROM Producto
                    WHERE Categoria IS NOT NULL AND TRIM(Categoria) <> '';";
            using (var cmd = con.Comando(sql))
                cmd.ExecuteNonQuery();
        }

        private void InsertarCategoriaSiFalta(DbConnection con, string nombre)
        {
            using (var chk = con.Comando("SELECT COUNT(*) FROM Categoria WHERE Nombre = @n;"))
            {
                chk.AddParam("@n", nombre);
                if (Convert.ToInt64(chk.ExecuteScalar()) > 0) return;
            }
            using (var ins = con.Comando("INSERT INTO Categoria (Nombre) VALUES (@n);"))
            {
                ins.AddParam("@n", nombre);
                ins.ExecuteNonQuery();
            }
        }

        private static void Ejecutar(DbConnection con, string sql)
        {
            using (var cmd = con.Comando(sql)) cmd.ExecuteNonQuery();
        }
    }
}

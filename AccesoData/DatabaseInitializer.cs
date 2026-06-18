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
        // Versión del esquema que maneja ESTA app. Se sube al agregar una migración nueva (0.B).
        // Si la BD trae una versión MAYOR (otra caja con binario más nuevo ya migró), se aborta el
        // arranque con un mensaje claro, en vez de operar contra un esquema desconocido.
        public const int ESQUEMA_VERSION = 3;   // v2: PagoVenta (pago mixto); v3: Devolucion (devolución parcial)

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
                VerificarVersionEsquema(con);   // aborta si la BD es más nueva que esta app (multi-caja)
                MigrarEsquema(con);
                CrearIndices(con);
                SembrarAdmin(con);
                SincronizarCategorias(con);
                ActualizarVersionEsquema(con);   // deja la BD marcada con la versión de esta app
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
            CREATE TABLE IF NOT EXISTS SchemaVersion (
                Version INTEGER NOT NULL
            );
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
                Activo      INTEGER NOT NULL DEFAULT 1,
                DebeCambiarPassword INTEGER NOT NULL DEFAULT 0
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
            );
            CREATE TABLE IF NOT EXISTS PagoVenta (
                IdPago    INTEGER PRIMARY KEY AUTOINCREMENT,
                IdVenta   INTEGER NOT NULL,
                MedioPago TEXT    NOT NULL,
                Monto     REAL    NOT NULL DEFAULT 0,
                FOREIGN KEY (IdVenta) REFERENCES Venta(IdVenta)
            );
            CREATE TABLE IF NOT EXISTS Devolucion (
                IdDevolucion INTEGER PRIMARY KEY AUTOINCREMENT,
                IdVenta   INTEGER NOT NULL,
                IdCaja    INTEGER NOT NULL,
                Fecha     TEXT    NOT NULL,
                IdUsuario INTEGER NOT NULL,
                Monto     REAL    NOT NULL DEFAULT 0,
                FOREIGN KEY (IdVenta)   REFERENCES Venta(IdVenta),
                FOREIGN KEY (IdCaja)    REFERENCES Caja(IdCaja),
                FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
            );
            CREATE TABLE IF NOT EXISTS DevolucionItem (
                IdItem    INTEGER PRIMARY KEY AUTOINCREMENT,
                IdDevolucion INTEGER NOT NULL,
                IdProducto INTEGER NOT NULL,
                Cantidad  REAL    NOT NULL DEFAULT 0,
                Subtotal  REAL    NOT NULL DEFAULT 0,
                FOREIGN KEY (IdDevolucion) REFERENCES Devolucion(IdDevolucion),
                FOREIGN KEY (IdProducto)   REFERENCES Producto(IdProducto)
            );";

        // Esquema completo (incluye Anulada). Orden respetando las claves foráneas.
        private const string DDL_SqlServer = @"
            IF OBJECT_ID(N'dbo.SchemaVersion','U') IS NULL
            CREATE TABLE SchemaVersion (
                Version INT NOT NULL
            );
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
                Activo      INT NOT NULL DEFAULT 1,
                DebeCambiarPassword INT NOT NULL DEFAULT 0
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
            );
            IF OBJECT_ID(N'dbo.PagoVenta','U') IS NULL
            CREATE TABLE PagoVenta (
                IdPago    INT IDENTITY(1,1) PRIMARY KEY,
                IdVenta   INT NOT NULL,
                MedioPago NVARCHAR(20) NOT NULL,
                Monto     DECIMAL(18,4) NOT NULL DEFAULT 0,
                CONSTRAINT FK_Pago_Venta FOREIGN KEY (IdVenta) REFERENCES Venta(IdVenta)
            );
            IF OBJECT_ID(N'dbo.Devolucion','U') IS NULL
            CREATE TABLE Devolucion (
                IdDevolucion INT IDENTITY(1,1) PRIMARY KEY,
                IdVenta   INT NOT NULL,
                IdCaja    INT NOT NULL,
                Fecha     NVARCHAR(19) NOT NULL,
                IdUsuario INT NOT NULL,
                Monto     DECIMAL(18,4) NOT NULL DEFAULT 0,
                CONSTRAINT FK_Dev_Venta   FOREIGN KEY (IdVenta)   REFERENCES Venta(IdVenta),
                CONSTRAINT FK_Dev_Caja    FOREIGN KEY (IdCaja)    REFERENCES Caja(IdCaja),
                CONSTRAINT FK_Dev_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
            );
            IF OBJECT_ID(N'dbo.DevolucionItem','U') IS NULL
            CREATE TABLE DevolucionItem (
                IdItem    INT IDENTITY(1,1) PRIMARY KEY,
                IdDevolucion INT NOT NULL,
                IdProducto INT NOT NULL,
                Cantidad  DECIMAL(18,4) NOT NULL DEFAULT 0,
                Subtotal  DECIMAL(18,4) NOT NULL DEFAULT 0,
                CONSTRAINT FK_DevItem_Dev      FOREIGN KEY (IdDevolucion) REFERENCES Devolucion(IdDevolucion),
                CONSTRAINT FK_DevItem_Producto FOREIGN KEY (IdProducto)   REFERENCES Producto(IdProducto)
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
            if (!ColumnaExiste(con, "Usuario", "DebeCambiarPassword"))
            {
                Ejecutar(con, "ALTER TABLE Usuario ADD COLUMN DebeCambiarPassword INTEGER NOT NULL DEFAULT 0;");
                // BD ya existente: forzamos el cambio de la clave del admin por defecto en su próximo ingreso.
                Ejecutar(con, "UPDATE Usuario SET DebeCambiarPassword = 1 WHERE LoginNombre = 'admin';");
            }

            // Backfill de PagoVenta para BDs anteriores al pago mixto (v2): el desglose de arqueo/reportes
            // lee SOLO de PagoVenta, así que una venta sin filas de pago desaparece del efectivo del arqueo
            // mientras su total sí se cuenta. Crea un pago = total por el medio de la venta. Idempotente
            // (solo donde no exista pago) y válido en SQLite y SQL Server.
            Ejecutar(con, @"
                INSERT INTO PagoVenta (IdVenta, MedioPago, Monto)
                SELECT v.IdVenta, COALESCE(v.MedioPago, 'Efectivo'), v.Total
                FROM Venta v
                WHERE NOT EXISTS (SELECT 1 FROM PagoVenta p WHERE p.IdVenta = v.IdVenta);");
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
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='ux_producto_codigo')     CREATE UNIQUE INDEX ux_producto_codigo ON Producto(CodigoBarras) WHERE CodigoBarras IS NOT NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_pago_venta')         CREATE INDEX idx_pago_venta         ON PagoVenta(IdVenta);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_devolucion_caja')    CREATE INDEX idx_devolucion_caja    ON Devolucion(IdCaja);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='idx_devitem_dev')        CREATE INDEX idx_devitem_dev        ON DevolucionItem(IdDevolucion);"
                : @"CREATE INDEX IF NOT EXISTS idx_venta_fecha        ON Venta(Fecha);
                    CREATE INDEX IF NOT EXISTS idx_venta_caja         ON Venta(IdCaja);
                    CREATE INDEX IF NOT EXISTS idx_detalle_venta      ON DetalleVenta(IdVenta);
                    CREATE INDEX IF NOT EXISTS idx_detalle_producto   ON DetalleVenta(IdProducto);
                    CREATE INDEX IF NOT EXISTS idx_producto_categoria ON Producto(Categoria);
                    CREATE INDEX IF NOT EXISTS idx_log_fecha          ON LogMovimiento(Fecha);
                    CREATE INDEX IF NOT EXISTS idx_pago_venta         ON PagoVenta(IdVenta);
                    CREATE INDEX IF NOT EXISTS idx_devolucion_caja   ON Devolucion(IdCaja);
                    CREATE INDEX IF NOT EXISTS idx_devitem_dev       ON DevolucionItem(IdDevolucion);";
            using (var cmd = con.Comando(sql))
                cmd.ExecuteNonQuery();
        }

        private void SembrarAdmin(DbConnection con)
        {
            using (var cmd = con.Comando("SELECT COUNT(*) FROM Usuario;"))
                if (Convert.ToInt64(cmd.ExecuteScalar()) > 0) return;

            using (var cmd = con.Comando(@"
                INSERT INTO Usuario (Nombre, LoginNombre, Pass, Rol, Activo, DebeCambiarPassword)
                VALUES (@nombre, @login, @pass, @rol, 1, 1);"))
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

        // Aborta el arranque si la BD fue migrada por una versión MÁS NUEVA de la app (multi-caja:
        // otra caja con binario más nuevo ya migró el esquema central). 0.B del roadmap.
        private void VerificarVersionEsquema(DbConnection con)
        {
            int v = LeerVersionEsquema(con);
            if (v > ESQUEMA_VERSION)
                throw new InvalidOperationException(
                    "Esta caja está DESACTUALIZADA: la base de datos es versión " + v + " y esta " +
                    "aplicación maneja la versión " + ESQUEMA_VERSION + ". Actualiza la aplicación de " +
                    "esta caja antes de continuar.");
        }

        // 0 = sin registro de versión (BD nueva o anterior al versionado).
        private int LeerVersionEsquema(DbConnection con)
        {
            using (var cmd = con.Comando("SELECT Version FROM SchemaVersion;"))
            {
                var r = cmd.ExecuteScalar();
                return (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
            }
        }

        private void ActualizarVersionEsquema(DbConnection con)
        {
            int actual = LeerVersionEsquema(con);
            if (actual == 0)
                Ejecutar(con, "INSERT INTO SchemaVersion (Version) VALUES (" + ESQUEMA_VERSION + ");");
            else if (actual < ESQUEMA_VERSION)
                Ejecutar(con, "UPDATE SchemaVersion SET Version = " + ESQUEMA_VERSION + ";");
        }

        private static void Ejecutar(DbConnection con, string sql)
        {
            using (var cmd = con.Comando(sql)) cmd.ExecuteNonQuery();
        }
    }
}

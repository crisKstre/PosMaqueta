-- =====================================================================
-- Esquema de la base de datos del POS (SQLite)
-- =====================================================================
-- Este script es solo de REFERENCIA. La aplicación crea estas tablas
-- automáticamente al iniciar (ver AccesoData/DatabaseInitializer.cs).
-- Lo incluimos para que puedas revisar el modelo o crear el .db a mano
-- con DB Browser for SQLite si lo necesitas.
-- =====================================================================

CREATE TABLE IF NOT EXISTS Usuario (
    IdUsuario   INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre      TEXT    NOT NULL,
    LoginNombre TEXT    NOT NULL UNIQUE,
    Pass        TEXT    NOT NULL,          -- hash SHA256
    Rol         TEXT    NOT NULL,          -- 'Admin' o 'Cajero'
    Activo      INTEGER NOT NULL DEFAULT 1 -- 1 = activo, 0 = inactivo
);

CREATE TABLE IF NOT EXISTS Producto (
    IdProducto   INTEGER PRIMARY KEY AUTOINCREMENT,
    CodigoBarras TEXT    UNIQUE,           -- lo que lee el escáner
    Nombre       TEXT    NOT NULL,
    Categoria    TEXT,
    Precio       REAL    NOT NULL DEFAULT 0,
    Stock        INTEGER NOT NULL DEFAULT 0,
    StockMinimo  INTEGER NOT NULL DEFAULT 0,
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
    Estado        TEXT    NOT NULL,        -- 'Abierta' o 'Cerrada'
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
);

CREATE TABLE IF NOT EXISTS Venta (
    IdVenta   INTEGER PRIMARY KEY AUTOINCREMENT,
    IdCaja    INTEGER NOT NULL,
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
    Cantidad       INTEGER NOT NULL DEFAULT 1,
    PrecioUnitario REAL    NOT NULL DEFAULT 0,  -- precio congelado al momento de la venta
    Subtotal       REAL    NOT NULL DEFAULT 0,
    FOREIGN KEY (IdVenta)    REFERENCES Venta(IdVenta),
    FOREIGN KEY (IdProducto) REFERENCES Producto(IdProducto)
);

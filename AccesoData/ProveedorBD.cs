namespace AccesoData
{
    /// <summary>
    /// Motor de base de datos. SQLite (local, una sola caja, sin servidor) o
    /// SQL Server (central, varias cajas compartiendo el mismo inventario y ventas).
    /// </summary>
    public enum ProveedorBD { Sqlite, SqlServer }
}

using Microsoft.Data.Sqlite;

namespace AccesoData
{
    public abstract class ConexionSqlite
    {
        protected SqliteConnection GetConnection()
        {
            return new SqliteConnection(ConfigBD.CadenaConexion);
        }
    }
}

using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace AccesoData
{
    /// <summary>
    /// Base de los DAOs: entrega una conexión (sin abrir) del motor configurado en ConfigBD.
    /// SQLite (local) o SQL Server (central). El resto del DAO trabaja contra DbConnection.
    /// </summary>
    public abstract class ConexionBD
    {
        protected DbConnection GetConnection()
        {
            return ConfigBD.Proveedor == ProveedorBD.SqlServer
                ? (DbConnection)new SqlConnection(ConfigBD.CadenaConexion)
                : new SqliteConnection(ConfigBD.CadenaConexion);
        }
    }
}

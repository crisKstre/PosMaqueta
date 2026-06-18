using System;
using AccesoData;
using Microsoft.Data.Sqlite;
using Xunit;

namespace PosMaqueta.Tests
{
    public class EsquemaVersionTests : ServiciosTestBase
    {
        // 0.B — una caja con binario VIEJO contra una BD migrada por una versión más nueva debe
        // NEGARSE a arrancar (en vez de operar contra un esquema desconocido).
        [Fact]
        public void Inicializar_con_BD_de_version_mayor_aborta()
        {
            // ServiciosTestBase ya inicializó la BD (versión actual). La marcamos como FUTURA.
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE SchemaVersion SET Version = " + (DatabaseInitializer.ESQUEMA_VERSION + 1) + ";";
                    cmd.ExecuteNonQuery();
                }
            }
            Assert.Throws<InvalidOperationException>(() => new DatabaseInitializer().Inicializar());
        }

        [Fact]
        public void Inicializar_marca_la_BD_con_la_version_actual()
        {
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT Version FROM SchemaVersion;";
                    Assert.Equal(DatabaseInitializer.ESQUEMA_VERSION, Convert.ToInt32(cmd.ExecuteScalar()));
                }
            }
        }
    }
}

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

        // C12 — una BD en una versión MENOR debe migrar al reinicializar y subir la versión, sin perder datos.
        [Fact]
        public void Inicializar_sobre_BD_de_version_menor_migra_y_conserva_datos()
        {
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                Exec(con, "UPDATE SchemaVersion SET Version = 1;");
                Exec(con, "INSERT INTO Categoria (Nombre) VALUES ('SobreviveMigracion');");
            }

            new DatabaseInitializer().Inicializar();   // debe migrar y subir la versión

            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                Assert.Equal(DatabaseInitializer.ESQUEMA_VERSION, Escalar(con, "SELECT Version FROM SchemaVersion;"));
                Assert.Equal(1, Escalar(con, "SELECT COUNT(*) FROM Categoria WHERE Nombre = 'SobreviveMigracion';"));
            }
        }

        // C5 — al migrar una BD anterior al pago mixto, las ventas sin filas en PagoVenta deben
        // backfillearse (medio + total), si no su efectivo desaparece del arqueo/reportes.
        [Fact]
        public void Migracion_backfillea_PagoVenta_de_ventas_existentes()
        {
            // Simula una venta vieja: sin filas en PagoVenta (IdUsuario 1 = admin sembrado).
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                Exec(con, "DELETE FROM PagoVenta;");
                Exec(con, "INSERT INTO Venta (IdUsuario, Fecha, Total, Descuento, MedioPago) " +
                          "VALUES (1, '2026-01-01 10:00:00', 5000, 0, 'Efectivo');");
            }

            new DatabaseInitializer().Inicializar();   // re-corre MigrarEsquema -> backfill

            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT MedioPago, Monto FROM PagoVenta " +
                                      "WHERE IdVenta = (SELECT MAX(IdVenta) FROM Venta);";
                    using (var r = cmd.ExecuteReader())
                    {
                        Assert.True(r.Read());
                        Assert.Equal("Efectivo", r.GetString(0));
                        Assert.Equal(5000m, Convert.ToDecimal(r.GetValue(1)));
                    }
                }
            }
        }

        private static void Exec(SqliteConnection con, string sql)
        {
            using (var cmd = con.CreateCommand()) { cmd.CommandText = sql; cmd.ExecuteNonQuery(); }
        }

        private static int Escalar(SqliteConnection con, string sql)
        {
            using (var cmd = con.CreateCommand()) { cmd.CommandText = sql; return Convert.ToInt32(cmd.ExecuteScalar()); }
        }
    }
}

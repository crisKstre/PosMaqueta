using System;
using System.IO;
using AccesoData;
using Xunit;

namespace PosMaqueta.Tests
{
    /// <summary>
    /// Pruebas de la BANDEJA de LogRemoto (filtro de nivel, persistencia, tope, tolerancia a
    /// corrupción) sin necesidad de un servidor de sede. El envío real (INSERT a LogFallo) se
    /// valida manualmente con las pruebas de aceptación del brief (requiere la sede accesible).
    /// La cadena de conexión usada aquí nunca se disca: el worker recién corre a los 20 s y los
    /// tests terminan antes; Encolar() jamás abre red.
    /// </summary>
    public class LogRemotoTests : IDisposable
    {
        private readonly string dir;
        // Cadena no enrutable (con timeout corto) solo para que LogRemoto quede ACTIVO; nunca se disca.
        private const string ConnInactiva =
            "Server=10.255.255.1,1433;Database=x;User ID=u;Password=p;TrustServerCertificate=true;Connect Timeout=1";

        public LogRemotoTests()
        {
            dir = Path.Combine(Path.GetTempPath(), "logremoto_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
        }

        public void Dispose()
        {
            LogRemoto.Configurar("", "", "", "", "ERROR");   // desactiva y dispone el Timer
            try { Directory.Delete(dir, true); } catch { /* temporal */ }
        }

        private string Outbox => Path.Combine(dir, "outbox-fallos.jsonl");

        [Fact]
        public void Sin_conexion_queda_inactivo_y_no_escribe()
        {
            LogRemoto.Configurar("T", "C", "", "1.0", "ERROR", dir);
            LogRemoto.Encolar("ERROR", "x", null);
            Assert.Equal(0, LogRemoto.PendientesDeEnvio);
            Assert.False(File.Exists(Outbox));
        }

        [Fact]
        public void Filtra_por_nivel_minimo_ERROR()
        {
            LogRemoto.Configurar("T", "C", ConnInactiva, "1.0", "ERROR", dir);
            LogRemoto.Encolar("INFO", "i", null);
            LogRemoto.Encolar("WARN", "w", null);
            Assert.Equal(0, LogRemoto.PendientesDeEnvio);     // INFO/WARN no se envían

            LogRemoto.Encolar("ERROR", "e", null);
            LogRemoto.Encolar("FATAL", "f", new Exception("boom"));
            Assert.Equal(2, LogRemoto.PendientesDeEnvio);
        }

        [Fact]
        public void Nivel_WARN_incluye_advertencias()
        {
            LogRemoto.Configurar("T", "C", ConnInactiva, "1.0", "WARN", dir);
            LogRemoto.Encolar("WARN", "w", null);
            LogRemoto.Encolar("INFO", "i", null);
            Assert.Equal(1, LogRemoto.PendientesDeEnvio);     // WARN sí, INFO no
        }

        [Fact]
        public void Persiste_y_recarga_la_bandeja_tras_reinicio()
        {
            LogRemoto.Configurar("T", "C", ConnInactiva, "1.0", "ERROR", dir);
            LogRemoto.Encolar("ERROR", "uno", null);
            LogRemoto.Encolar("ERROR", "dos", new Exception("x"));
            Assert.Equal(2, File.ReadAllLines(Outbox).Length);

            LogRemoto.Configurar("T", "C", ConnInactiva, "1.0", "ERROR", dir);   // simula reinicio de la app
            Assert.Equal(2, LogRemoto.PendientesDeEnvio);
        }

        [Fact]
        public void Respeta_el_tope_descartando_los_mas_viejos()
        {
            LogRemoto.Configurar("T", "C", ConnInactiva, "1.0", "ERROR", dir, capEventos: 3);
            for (int i = 0; i < 6; i++) LogRemoto.Encolar("ERROR", "m" + i, null);
            Assert.Equal(3, LogRemoto.PendientesDeEnvio);
        }

        [Fact]
        public void Sanitiza_contrasenas_antes_de_enviar()
        {
            LogRemoto.Configurar("T", "C", ConnInactiva, "1.0", "ERROR", dir);
            LogRemoto.Encolar("ERROR", "fallo de conexión",
                new Exception("Server=x;Password=secreto123;User ID=u"));
            string contenido = File.ReadAllText(Outbox);
            Assert.DoesNotContain("secreto123", contenido);   // la clave no viaja
            Assert.Contains("***", contenido);
        }

        [Fact]
        public void Tolera_lineas_corruptas_al_cargar()
        {
            LogRemoto.Configurar("T", "C", ConnInactiva, "1.0", "ERROR", dir);
            LogRemoto.Encolar("ERROR", "valido", null);          // 1 línea JSON válida
            File.AppendAllText(Outbox, "{ esto no es json valido\n");   // 1 línea corrupta

            LogRemoto.Configurar("T", "C", ConnInactiva, "1.0", "ERROR", dir);   // recarga
            Assert.Equal(1, LogRemoto.PendientesDeEnvio);        // la corrupta se descarta, sin lanzar
        }
    }
}

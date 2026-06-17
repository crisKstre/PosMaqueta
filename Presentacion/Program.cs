using System;
using System.Windows.Forms;
using AccesoData;
using Presentacion.Forms;

namespace Presentacion
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Log.Info("════════ Sistema POS iniciado ════════");
            Log.LimpiarAntiguos(30);
            ConfigurarBaseDatos();

            // Captura global: cualquier error no manejado queda registrado
            Application.ThreadException += (s, e) =>
            {
                Log.Error("Excepción no manejada en la interfaz", e.Exception);
                MessageBox.Show("Ocurrió un error inesperado. Quedó registrado en el log.\n\n" + e.Exception.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Log.Fatal("Excepción fatal no controlada", e.ExceptionObject as Exception);

            try
            {
                // Crea la base de datos y el admin por defecto si no existen
                new DatabaseInitializer().Inicializar();
                Log.Info("Base de datos inicializada");
                // Respaldo automático en segundo plano: no debe retrasar la aparición del login.
                // (RespaldarSiCorresponde captura sus propias excepciones, así que es seguro en Task.Run.)
                System.Threading.Tasks.Task.Run(() => RespaldoBD.RespaldarSiCorresponde());
            }
            catch (Exception ex)
            {
                Log.Fatal("No se pudo inicializar la base de datos", ex);
                MessageBox.Show(
                    "No se pudo inicializar la base de datos:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FormLogin());
            Log.Info("════════ Sistema POS cerrado ════════");
        }

        // Lee el motor de BD y la cadena de conexión desde App.config (appSettings).
        // Por defecto SQLite; para varias cajas se pone ProveedorBD=SqlServer + CadenaConexion.
        private static void ConfigurarBaseDatos()
        {
            var prov = System.Configuration.ConfigurationManager.AppSettings["ProveedorBD"];
            if (!string.IsNullOrWhiteSpace(prov) && prov.Trim().Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                ConfigBD.Proveedor = ProveedorBD.SqlServer;

            var cad = System.Configuration.ConfigurationManager.AppSettings["CadenaConexion"];
            if (!string.IsNullOrWhiteSpace(cad))
                ConfigBD.CadenaConexion = cad;

            var carpeta = System.Configuration.ConfigurationManager.AppSettings["CarpetaRespaldoExterno"];
            if (!string.IsNullOrWhiteSpace(carpeta))
                ConfigBD.CarpetaRespaldoExterno = carpeta.Trim();

            Log.Info("Motor de BD: " + ConfigBD.Proveedor);
        }
    }
}

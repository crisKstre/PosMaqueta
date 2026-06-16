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
                // Respaldo automático de la base de datos (una copia por día)
                RespaldoBD.RespaldarSiCorresponde();
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
    }
}

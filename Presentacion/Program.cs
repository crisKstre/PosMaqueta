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

            try
            {
                // Crea la base de datos y el admin por defecto si no existen
                new DatabaseInitializer().Inicializar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo inicializar la base de datos:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FormLogin());
        }
    }
}

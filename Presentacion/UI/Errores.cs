using System;
using System.Windows.Forms;
using AccesoData;
using Dominio;

namespace Presentacion
{
    /// <summary>
    /// Traduce una excepción al mensaje que verá el usuario, separando dos casos:
    ///  • NegocioException → se muestra su mensaje tal cual (es un aviso esperado).
    ///  • Cualquier otra (técnica: SQLite, E/S, referencia nula…) → se registra con
    ///    su stack trace y se devuelve un mensaje genérico, sin filtrar detalles internos.
    /// </summary>
    public static class Errores
    {
        public static string Usuario(Exception ex)
        {
            if (ex is NegocioException) return ex.Message;
            Log.Error("Error técnico no controlado en la interfaz", ex);
            return "Ocurrió un problema y la acción no se completó. Inténtalo de nuevo.";
        }

        /// <summary>
        /// Muestra el error como un DIÁLOGO emergente (más visible e intuitivo que un texto en la
        /// pantalla): una regla de negocio (NegocioException, p. ej. "Debe abrir caja antes de
        /// vender") sale como advertencia con su mensaje; un fallo técnico, como error genérico
        /// (y queda registrado con su stack en el log).
        /// </summary>
        public static void Mostrar(IWin32Window owner, Exception ex, string titulo = null)
        {
            if (ex is NegocioException)
            {
                Aviso.Advertencia(owner, ex.Message, titulo ?? "Atención");
                return;
            }
            Log.Error("Error técnico no controlado en la interfaz", ex);
            Aviso.Error(owner, "Ocurrió un problema y la acción no se completó. Inténtalo de nuevo.",
                titulo ?? "Error");
        }
    }
}

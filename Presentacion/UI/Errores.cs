using System;
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
    }
}

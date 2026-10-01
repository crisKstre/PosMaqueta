using System;

namespace Dominio.Eventos
{
    // Patrón Observer simplificado: cualquier capa puede notificar que cambió
    // una entidad, y los formularios suscritos refrescan sus tablas.
    //
    // Uso típico:
    //   - En un Service tras guardar: NotificadorCambios.Notificar(Entidad.Producto);
    //   - En un Form al cargar:       NotificadorCambios.Cambio += OnCambio;
    //   - En un Form al cerrar:       NotificadorCambios.Cambio -= OnCambio;
    public static class NotificadorCambios
    {
        public static event Action<string> Cambio;

        public static void Notificar(string entidad)
        {
            var suscriptores = Cambio;
            if (suscriptores == null) return;
            foreach (Action<string> suscriptor in suscriptores.GetInvocationList())
            {
                try { suscriptor(entidad); }
                catch (Exception ex)
                {
                    // Avisos de refresco posteriores al guardado: un observador no puede convertir
                    // una operación confirmada en fallo ni impedir que se avise a los demás.
                    AccesoData.Log.Error("Falló un suscriptor del cambio de " + entidad + ".", ex);
                }
            }
        }
    }

    public static class Entidad
    {
        public const string Producto = "Producto";
        public const string Venta = "Venta";
        public const string Caja = "Caja";
        public const string Usuario = "Usuario";
    }
}

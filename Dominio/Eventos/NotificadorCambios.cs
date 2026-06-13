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
            Cambio?.Invoke(entidad);
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

using Entidades;

namespace Dominio
{
    /// <summary>
    /// Autorización en la capa de servicio (defensa en profundidad). La UI ya oculta lo que un rol
    /// no puede usar, pero las operaciones sensibles también lo exigen aquí: así, aunque se invoque
    /// un servicio directamente (sin pasar por la UI), un cajero no puede ejecutar acciones de admin.
    /// </summary>
    public static class Autorizacion
    {
        public static void ExigirAdmin()
        {
            if (Sesion.UsuarioActual == null || Sesion.UsuarioActual.Rol != RolUsuario.Admin)
                throw new NegocioException("Esta acción requiere un administrador.");
        }
    }
}

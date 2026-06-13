namespace Entidades
{
    public static class Sesion
    {
        public static Usuario UsuarioActual { get; set; }
        public static Caja CajaActual { get; set; }

        public static bool EsAdmin
        {
            get { return UsuarioActual != null && UsuarioActual.Rol == RolUsuario.Admin; }
        }

        public static void Cerrar()
        {
            UsuarioActual = null;
            CajaActual = null;
        }
    }
}

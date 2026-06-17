namespace Entidades
{
    public static class RolUsuario
    {
        public const string Admin = "Admin";
        public const string Cajero = "Cajero";

        // Roles válidos, para poblar combos y validar. El orden es el de presentación.
        public static readonly string[] Todos = { Admin, Cajero };
    }
}

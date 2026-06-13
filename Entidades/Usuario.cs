using System;

namespace Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string LoginNombre { get; set; }
        public string Pass { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
    }
}

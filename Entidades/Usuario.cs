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

        // Marca de seguridad: obliga al usuario a cambiar su contraseña en el próximo ingreso.
        // Se activa en el admin sembrado (admin/admin123), al crear un usuario y tras un reseteo por admin.
        public bool DebeCambiarPassword { get; set; }
    }
}

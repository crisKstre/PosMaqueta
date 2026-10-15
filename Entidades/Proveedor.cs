using System;

namespace Entidades
{
    public sealed class Proveedor
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; }
        public string Rut { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Direccion { get; set; }
        public string Notas { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}

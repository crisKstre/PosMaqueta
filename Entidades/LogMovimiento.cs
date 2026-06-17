using System;

namespace Entidades
{
    public class LogMovimiento
    {
        public int IdLog { get; set; }
        public DateTime Fecha { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Detalle { get; set; }
    }

    public static class ModuloLog
    {
        public const string Productos = "Productos";
        public const string Ventas    = "Ventas";
        public const string Caja      = "Caja";
        public const string Usuarios  = "Usuarios";
    }
}

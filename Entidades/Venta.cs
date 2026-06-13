using System;
using System.Collections.Generic;

namespace Entidades
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int? IdCaja { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string MedioPago { get; set; }

        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }

    public static class MedioPago
    {
        public const string Efectivo = "Efectivo";
        public const string Tarjeta = "Tarjeta";
        public const string Transferencia = "Transferencia";
    }
}

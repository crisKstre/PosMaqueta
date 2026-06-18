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
        public decimal Descuento { get; set; }
        public string MedioPago { get; set; }   // medio único, o "Mixto" si hay varios pagos

        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
        public List<PagoVenta>    Pagos    { get; set; } = new List<PagoVenta>();
    }

    public static class MedioPago
    {
        public const string Efectivo = "Efectivo";
        public const string Tarjeta = "Tarjeta";
        public const string Transferencia = "Transferencia";
        public const string Mixto = "Mixto";   // resumen de una venta con varios medios
    }
}

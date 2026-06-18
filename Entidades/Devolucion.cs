using System;
using System.Collections.Generic;

namespace Entidades
{
    /// <summary>
    /// Devolución (parcial o total) de una venta: reintegra stock y registra una salida de efectivo
    /// en la caja del turno (afecta el arqueo). Solo la realiza un administrador.
    /// </summary>
    public class Devolucion
    {
        public int IdDevolucion { get; set; }
        public int IdVenta { get; set; }
        public int IdCaja { get; set; }            // la caja abierta al momento de la devolución
        public DateTime Fecha { get; set; }
        public int IdUsuario { get; set; }
        public decimal Monto { get; set; }         // total reembolsado (suma de los ítems)
        public List<DevolucionItem> Detalles { get; set; } = new List<DevolucionItem>();
    }

    public class DevolucionItem
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }   // del detalle original (transitorio)
        public decimal Subtotal { get; set; }
    }
}

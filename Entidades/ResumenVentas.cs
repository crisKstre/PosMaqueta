namespace Entidades
{
    /// <summary>Totales de ventas de un período, para el módulo de Reportes.</summary>
    public class ResumenVentas
    {
        public int CantidadVentas { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalDevoluciones { get; set; }   // devuelto en el período (sale del efectivo de la caja)
        // Venta neta del período: lo vendido menos lo devuelto. Así el reporte cuadra con el arqueo,
        // que también descuenta las devoluciones del efectivo esperado.
        public decimal TotalNeto => TotalVendido - TotalDevoluciones;
        public decimal TicketPromedio => CantidadVentas > 0 ? TotalVendido / CantidadVentas : 0;
    }
}

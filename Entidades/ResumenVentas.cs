namespace Entidades
{
    /// <summary>Totales de ventas de un período, para el módulo de Reportes.</summary>
    public class ResumenVentas
    {
        public int CantidadVentas { get; set; }
        // Importe cobrado por ventas no anuladas del período, después de descuentos y antes de devoluciones.
        public decimal TotalVendido { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalDevoluciones { get; set; }   // devuelto en el período (sale del efectivo de la caja)
        // Venta neta del período: lo vendido menos lo devuelto. Así el reporte cuadra con el arqueo,
        // que también descuenta las devoluciones del efectivo esperado. Puede ser negativo en un
        // período con reembolsos de ventas anteriores y sin nuevos cobros.
        public decimal TotalNeto => TotalVendido - TotalDevoluciones;

        // Costos originales guardados al vender. Los productos sin costo cargado aportan cero.
        public decimal TotalCosto { get; set; }
        public decimal TotalCostoDevuelto { get; set; }
        public decimal TotalCostoNeto => TotalCosto - TotalCostoDevuelto;
        public decimal Utilidad => TotalNeto - TotalCostoNeto;
        public bool TieneMargen => TotalNeto > 0;
        public decimal MargenPorcentaje => TieneMargen ? Utilidad / TotalNeto * 100m : 0m;

        public decimal TicketPromedio => CantidadVentas > 0 ? TotalVendido / CantidadVentas : 0;
    }
}

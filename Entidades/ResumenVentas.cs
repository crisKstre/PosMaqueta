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
        public decimal TicketPromedio => CantidadVentas > 0 ? TotalVendido / CantidadVentas : 0;
    }
}

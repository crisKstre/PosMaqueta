namespace Entidades
{
    public class ResumenCaja
    {
        public int CantidadVentas { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalDevoluciones { get; set; }   // efectivo devuelto en este turno (sale del cajón)
    }
}

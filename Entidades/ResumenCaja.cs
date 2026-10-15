namespace Entidades
{
    public class ResumenCaja
    {
        public decimal OtrasEntradas { get; set; }
        public decimal OtrasSalidas { get; set; } // incluye gastos pagados desde el cajón
        public decimal TotalGastos { get; set; } // todos los medios; no se resta otra vez del arqueo
        public int CantidadVentas { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalDevoluciones { get; set; }   // efectivo devuelto en este turno (sale del cajón)
    }
}

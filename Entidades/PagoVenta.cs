namespace Entidades
{
    /// <summary>Un pago de una venta: medio + monto. Una venta puede tener varios (pago mixto).</summary>
    public class PagoVenta
    {
        public string  MedioPago { get; set; }
        public decimal Monto { get; set; }
    }
}

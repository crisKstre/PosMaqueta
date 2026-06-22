namespace Entidades
{
    public class DetalleVenta
    {
        public int IdDetalle { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public string CodigoBarras { get; set; }
        public string NombreProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }   // precio efectivamente cobrado (ya con descuento)
        public decimal PrecioOriginal { get; set; }   // precio de lista antes del descuento
        public decimal DescuentoPorcentaje { get; set; } // % aplicado a esta línea (0 = sin descuento)
        public decimal CostoUnitario { get; set; }    // snapshot del costo del producto al momento de la venta
        public decimal Subtotal { get; set; }

        public bool TieneDescuento => DescuentoPorcentaje > 0;
    }
}

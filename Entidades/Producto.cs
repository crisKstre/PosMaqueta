namespace Entidades
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public decimal Stock { get; set; }
        public decimal StockMinimo { get; set; }
        public string UnidadMedida { get; set; }
        public bool Activo { get; set; }

        // Descuento de oferta sobre el precio de lista (0–100). 0 = sin descuento.
        public decimal DescuentoPorcentaje { get; set; }

        public bool TieneDescuento => DescuentoPorcentaje > 0;

        // Precio efectivo de venta tras aplicar el descuento, redondeado al peso (CLP sin decimales).
        public decimal PrecioConDescuento =>
            DescuentoPorcentaje <= 0 ? Precio : Dinero.Redondear(Precio * (1 - DescuentoPorcentaje / 100m));
    }

    public static class UnidadMedida
    {
        public const string Unidad = "Unidad";
        public const string Kilogramo = "Kg";
    }
}

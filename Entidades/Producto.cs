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
    }

    public static class UnidadMedida
    {
        public const string Unidad = "Unidad";
        public const string Kilogramo = "Kg";
    }
}

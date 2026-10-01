namespace Entidades
{
    /// <summary>Producto agregado por cantidad/total vendido en un período (ranking de Reportes).</summary>
    public class ProductoVendido
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Total { get; set; }      // vendido después de descuentos menos reembolsos del período
        public decimal Costo { get; set; }      // costo neto: vendido menos reintegrado por devoluciones
        public decimal CostoDevuelto { get; set; }
        public decimal Utilidad => Total - Costo;
    }
}

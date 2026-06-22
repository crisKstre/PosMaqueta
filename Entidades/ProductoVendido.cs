namespace Entidades
{
    /// <summary>Producto agregado por cantidad/total vendido en un período (ranking de Reportes).</summary>
    public class ProductoVendido
    {
        public string Nombre { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Total { get; set; }      // total vendido (precio) del producto en el período
        public decimal Costo { get; set; }      // costo de lo vendido (Σ CostoUnitario × Cantidad)
        public decimal Utilidad => Total - Costo;
    }
}

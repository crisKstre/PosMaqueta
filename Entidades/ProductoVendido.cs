namespace Entidades
{
    /// <summary>Producto agregado por cantidad/total vendido en un período (ranking de Reportes).</summary>
    public class ProductoVendido
    {
        public string Nombre { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}

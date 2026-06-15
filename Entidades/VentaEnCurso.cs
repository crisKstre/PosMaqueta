using System;
using System.Collections.Generic;
using System.Linq;

namespace Entidades
{
    /// <summary>
    /// Una venta en preparación (carrito en memoria), aún no cobrada.
    /// Permite tener varias en paralelo: una activa (en primer plano) y otras en pausa.
    /// </summary>
    public class VentaEnCurso
    {
        public int Id { get; set; }                 // identificador temporal en memoria
        public string Etiqueta { get; set; }        // ej. "Venta 1"
        public List<DetalleVenta> Detalles { get; } = new List<DetalleVenta>();
        public DateTime UltimaActividad { get; set; }
        public decimal Total => Detalles.Sum(d => d.Subtotal);
    }
}

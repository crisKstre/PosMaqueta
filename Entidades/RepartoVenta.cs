using System;
using System.Collections.Generic;
using System.Linq;

namespace Entidades
{
    // Reparto monetario común a devoluciones y reportes. Sin acceso a datos ni estado global.
    public static class RepartoVenta
    {
        public static Dictionary<int, decimal> PorProducto(IEnumerable<DetalleVenta> detalles, decimal descuento)
        {
            var grupos = detalles.GroupBy(d => d.IdProducto)
                .Select(g => new { Id = g.Key, Subtotal = g.Sum(d => d.Subtotal) }).ToList();
            decimal bruto = grupos.Sum(g => g.Subtotal);
            if (descuento < 0 || descuento > bruto || descuento != decimal.Truncate(descuento) ||
                grupos.Any(g => g.Subtotal < 0 || g.Subtotal != decimal.Truncate(g.Subtotal)))
                throw new InvalidOperationException("Los importes de la venta requieren conciliación.");
            decimal cobrado = bruto - descuento;
            var cuotas = grupos.Select(g => new { g.Id, Importe = bruto == 0 ? 0 : cobrado * g.Subtotal / bruto }).ToList();
            var resultado = cuotas.ToDictionary(c => c.Id, c => decimal.Floor(c.Importe));
            int pesos = (int)(cobrado - resultado.Values.Sum());
            foreach (var c in cuotas.OrderByDescending(c => c.Importe - decimal.Floor(c.Importe)).ThenBy(c => c.Id).Take(pesos))
                resultado[c.Id]++;
            return resultado;
        }
    }
}

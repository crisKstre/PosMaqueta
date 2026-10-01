using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;

namespace Dominio
{
    // Cálculo puro compartido por la vista previa y el registro. Sin estado global ni acceso a BD.
    public sealed class CalculoDevolucion
    {
        private sealed class Saldo
        {
            public int Id;
            public string Nombre;
            public decimal Cantidad, Bruto, Presupuesto, Devuelto, Reembolsado;
        }

        private readonly List<Saldo> saldos;

        public CalculoDevolucion(Venta venta, Devolucion acumulado)
        {
            if (venta == null || venta.Detalles.Count == 0)
                throw new NegocioException("La venta no existe o no tiene detalle.");
            if (venta.Detalles.Any(d => d.Cantidad <= 0 || !EsImporte(d.Subtotal)) ||
                !EsImporte(venta.Descuento) || venta.Pagos.Count == 0 ||
                venta.Pagos.Any(p => !EsImporte(p.Monto)))
                throw Conciliacion();

            // La devolución identifica productos, no líneas. Se agrupan también las líneas históricas
            // repetidas; el subtotal guardado ya incorpora ofertas y redondeo de la venta por kilo.
            saldos = venta.Detalles.GroupBy(d => d.IdProducto).OrderBy(g => g.Key)
                .Select(g => new Saldo { Id = g.Key, Nombre = g.First().NombreProducto,
                    Cantidad = g.Sum(d => d.Cantidad), Bruto = g.Sum(d => d.Subtotal) }).ToList();
            decimal bruto = saldos.Sum(s => s.Bruto);
            decimal cobrado = bruto - venta.Descuento;
            // No usar Venta.Total: las devoluciones anteriores a MIG-001 podían haberlo reducido.
            // Pagos y detalles deben concordar; no se intenta reparar el histórico al devolver.
            if (cobrado < 0 || venta.Pagos.Sum(p => p.Monto) != cobrado)
                throw Conciliacion();

            var reparto = RepartoVenta.PorProducto(venta.Detalles, venta.Descuento);
            foreach (var s in saldos) s.Presupuesto = reparto[s.Id];

            if (!EsImporte(acumulado.Monto) || acumulado.Monto > cobrado ||
                acumulado.Detalles.Any(d => d.Cantidad < 0 || !EsImporte(d.Subtotal)) ||
                acumulado.Detalles.Sum(d => d.Subtotal) != acumulado.Monto)
                throw Conciliacion();
            foreach (var anterior in acumulado.Detalles.GroupBy(d => d.IdProducto))
            {
                var s = saldos.FirstOrDefault(x => x.Id == anterior.Key);
                if (s == null) throw Conciliacion();
                s.Devuelto = anterior.Sum(d => d.Cantidad);
                s.Reembolsado = anterior.Sum(d => d.Subtotal);
                if (s.Devuelto > s.Cantidad || s.Reembolsado != Acumulado(s, s.Devuelto))
                    throw Conciliacion();
            }
        }

        public List<DevolucionItem> ObtenerDevolvibles()
            => saldos.Where(s => s.Cantidad > s.Devuelto)
                .Select(s => CrearItem(s, s.Cantidad - s.Devuelto)).ToList();

        public List<DevolucionItem> Calcular(List<DevolucionItem> pedidos)
        {
            if (pedidos == null || pedidos.Count == 0)
                throw new NegocioException("Selecciona al menos un producto a devolver.");
            if (pedidos.Any(p => p == null || p.Cantidad < 0))
                throw new NegocioException("Las cantidades a devolver no pueden ser negativas ni los ítems vacíos.");
            if (pedidos.GroupBy(p => p.IdProducto).Any(g => g.Count() > 1))
                throw new NegocioException("Un producto aparece repetido. Indica su cantidad en un solo ítem.");

            var resultado = new List<DevolucionItem>();
            foreach (var p in pedidos)
            {
                var s = saldos.FirstOrDefault(x => x.Id == p.IdProducto);
                if (s == null) throw new NegocioException("Un producto seleccionado no pertenece a esta venta.");
                if (p.Cantidad > s.Cantidad - s.Devuelto)
                    throw new NegocioException("No puedes devolver más de lo vendido de \"" + s.Nombre +
                        "\" (disponible: " + (s.Cantidad - s.Devuelto).ToString("0.###") + ").");
                if (p.Cantidad > 0) resultado.Add(CrearItem(s, p.Cantidad));
            }
            if (resultado.Count == 0)
                throw new NegocioException("Selecciona al menos un producto a devolver.");
            return resultado;
        }

        private static DevolucionItem CrearItem(Saldo s, decimal cantidad)
            => new DevolucionItem { IdProducto = s.Id, NombreProducto = s.Nombre, Cantidad = cantidad,
                PrecioUnitario = s.Presupuesto / s.Cantidad,
                Subtotal = Acumulado(s, s.Devuelto + cantidad) - s.Reembolsado };

        // Redondear el acumulado, no cada fragmento: el último completa exactamente el presupuesto.
        private static decimal Acumulado(Saldo s, decimal cantidad)
            => cantidad == s.Cantidad ? s.Presupuesto : Dinero.Redondear(s.Presupuesto * cantidad / s.Cantidad);

        private static bool EsImporte(decimal monto) => monto >= 0 && monto == decimal.Truncate(monto);
        private static NegocioException Conciliacion()
            => new NegocioException("Los importes o cantidades de esta venta y sus devoluciones requieren conciliación antes de continuar.");
    }
}

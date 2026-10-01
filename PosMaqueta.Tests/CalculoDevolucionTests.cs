using System.Collections.Generic;
using System.Linq;
using Dominio;
using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    public class CalculoDevolucionTests
    {
        private static Venta Venta(decimal descuento, params DetalleVenta[] detalles)
        {
            decimal total = detalles.Sum(d => d.Subtotal) - descuento;
            return new Venta { Descuento = descuento, Total = total, Detalles = detalles.ToList(),
                Pagos = new List<PagoVenta> { new PagoVenta { MedioPago = MedioPago.Efectivo, Monto = total } } };
        }

        private static DetalleVenta Linea(int id, decimal cantidad, decimal subtotal)
            => new DetalleVenta { IdProducto = id, NombreProducto = "Producto " + id,
                Cantidad = cantidad, Subtotal = subtotal, PrecioUnitario = subtotal / cantidad };

        private static List<DevolucionItem> Pedido(int id, decimal cantidad)
            => new List<DevolucionItem> { new DevolucionItem { IdProducto = id, Cantidad = cantidad } };

        [Fact]
        public void Reparto_es_proporcional_a_subtotales_y_agrupa_lineas_del_mismo_producto()
        {
            // El producto 1 tiene dos precios históricos; se devuelve por producto, con promedio ponderado.
            var venta = Venta(600m, Linea(1, 1m, 1000m), Linea(2, 1m, 3000m), Linea(1, 1m, 2000m));
            var calculo = new CalculoDevolucion(venta, new Devolucion());
            var disponibles = calculo.ObtenerDevolvibles();
            Assert.Equal(2, disponibles.Count);
            Assert.Equal(2m, disponibles[0].Cantidad);
            Assert.Equal(2700m, disponibles[0].Subtotal);
            Assert.Equal(2700m, disponibles[1].Subtotal);
            Assert.Equal(1350m, Assert.Single(calculo.Calcular(Pedido(1, 1m))).Subtotal);
        }

        [Fact]
        public void Reparto_conserva_el_total_y_los_limites_para_descuentos_y_particiones()
        {
            for (int descuento = 0; descuento <= 17; descuento++)
            {
                var venta = Venta(descuento, Linea(1, 1m, 5m), Linea(2, 1m, 12m));
                var acumulado = new Devolucion();
                var inicial = new CalculoDevolucion(venta, acumulado).ObtenerDevolvibles();
                Assert.Equal(17m - descuento, inicial.Sum(i => i.Subtotal));
                Assert.InRange(inicial[0].Subtotal, 0m, 5m);
                Assert.InRange(inicial[1].Subtotal, 0m, 12m);
                // Veinte reembolsos pequeños, intercalando productos y reconstruyendo el cálculo.
                for (int paso = 0; paso < 10; paso++)
                    foreach (int producto in new[] { 2, 1 })
                    {
                        var calculo = new CalculoDevolucion(venta, acumulado);
                        var item = Assert.Single(calculo.Calcular(Pedido(producto, 0.1m)));
                        Assert.True(item.Subtotal >= 0);
                        Assert.Equal(decimal.Truncate(item.Subtotal), item.Subtotal);
                        acumulado.Detalles.Add(item);
                        acumulado.Monto += item.Subtotal;
                        Assert.InRange(acumulado.Monto, 0m, venta.Total);
                    }
                Assert.Equal(venta.Total, acumulado.Monto);
                Assert.Empty(new CalculoDevolucion(venta, acumulado).ObtenerDevolvibles());
            }
        }

        [Fact]
        public void Subtotal_guardado_por_kilo_prevalece_sobre_precio_por_cantidad()
        {
            var linea = Linea(1, 0.350m, 1047m);
            linea.PrecioUnitario = 2990m; // 1046,5 redondeado al vender
            var calculo = new CalculoDevolucion(Venta(47m, linea), new Devolucion());
            Assert.Equal(1000m, Assert.Single(calculo.Calcular(Pedido(1, 0.350m))).Subtotal);
        }

        [Fact]
        public void Venta_gratuita_no_divide_por_cero()
        {
            var calculo = new CalculoDevolucion(Venta(0m, Linea(1, 2m, 0m)), new Devolucion());
            Assert.Equal(0m, Assert.Single(calculo.Calcular(Pedido(1, 2m))).Subtotal);
        }

        [Fact]
        public void Pagos_que_no_coinciden_con_detalle_requieren_conciliacion()
        {
            var venta = Venta(500m, Linea(1, 2m, 2000m));
            venta.Pagos[0].Monto = 2000m;
            Assert.Throws<NegocioException>(() => new CalculoDevolucion(venta, new Devolucion()));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(3)]
        public void Cantidades_invalidas_se_rechazan(int cantidad)
        {
            var calculo = new CalculoDevolucion(Venta(0m, Linea(1, 2m, 2000m)), new Devolucion());
            Assert.Throws<NegocioException>(() => calculo.Calcular(Pedido(1, cantidad)));
        }

        [Fact]
        public void Seleccion_nula_item_nulo_y_producto_ajeno_se_rechazan()
        {
            var calculo = new CalculoDevolucion(Venta(0m, Linea(1, 2m, 2000m)), new Devolucion());
            Assert.Throws<NegocioException>(() => calculo.Calcular(null));
            Assert.Throws<NegocioException>(() => calculo.Calcular(new List<DevolucionItem> { null }));
            Assert.Throws<NegocioException>(() => calculo.Calcular(Pedido(99, 1m)));
        }

        [Fact]
        public void Devolucion_con_cabecera_distinta_de_sus_items_requiere_conciliacion()
        {
            var acumulado = new Devolucion { Monto = 1000m, Detalles = Pedido(1, 1m) };
            Assert.Throws<NegocioException>(() => new CalculoDevolucion(Venta(0m, Linea(1, 2m, 2000m)), acumulado));
        }
    }
}

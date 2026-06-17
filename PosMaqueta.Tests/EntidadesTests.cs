using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    /// <summary>Lógica pura de las entidades: precio con descuento, totales del carrito.</summary>
    public class EntidadesTests
    {
        [Fact]
        public void Producto_sin_descuento_usa_el_precio_de_lista()
        {
            var p = new Producto { Precio = 1000m, DescuentoPorcentaje = 0m };
            Assert.Equal(1000m, p.PrecioConDescuento);
            Assert.False(p.TieneDescuento);
        }

        [Theory]
        [InlineData(1000, 20, 800)]
        [InlineData(1000, 100, 0)]
        [InlineData(990, 10, 891)]
        [InlineData(999, 50, 500)]   // 499.5 -> 500 (AwayFromZero)
        public void Producto_aplica_el_descuento_redondeando_al_peso(decimal precio, decimal pct, decimal esperado)
        {
            var p = new Producto { Precio = precio, DescuentoPorcentaje = pct };
            Assert.Equal(esperado, p.PrecioConDescuento);
            Assert.True(p.TieneDescuento);
        }

        [Fact]
        public void VentaEnCurso_subtotal_suma_los_detalles()
        {
            var v = new VentaEnCurso();
            v.Detalles.Add(new DetalleVenta { Subtotal = 1500m });
            v.Detalles.Add(new DetalleVenta { Subtotal = 500m });
            Assert.Equal(2000m, v.Subtotal);
        }

        [Fact]
        public void VentaEnCurso_total_resta_el_descuento()
        {
            var v = new VentaEnCurso();
            v.Detalles.Add(new DetalleVenta { Subtotal = 2000m });
            v.DescuentoSolicitado = 300m;
            Assert.Equal(1700m, v.Total);
        }

        [Fact]
        public void VentaEnCurso_descuento_efectivo_se_acota_al_subtotal()
        {
            var v = new VentaEnCurso();
            v.Detalles.Add(new DetalleVenta { Subtotal = 1000m });
            v.DescuentoSolicitado = 5000m;     // mayor que el subtotal
            Assert.Equal(1000m, v.Descuento);  // se acota dinámicamente
            Assert.Equal(0m, v.Total);         // y el total nunca es negativo
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(15, true)]
        public void DetalleVenta_tiene_descuento_si_el_porcentaje_es_positivo(decimal pct, bool esperado)
            => Assert.Equal(esperado, new DetalleVenta { DescuentoPorcentaje = pct }.TieneDescuento);
    }
}

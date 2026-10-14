using System.Globalization;
using Presentacion;
using Xunit;

namespace PosMaqueta.Tests
{
    public class EntradaCantidadTests
    {
        [Theory]
        [InlineData("es-CL", "0,350")]
        [InlineData("es-CL", "0.350")]
        [InlineData("en-US", "0,350")]
        [InlineData("en-US", "0.350")]
        public void Peso_independiente_del_separador_regional(string cultura, string texto)
        {
            var anterior = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);
                Assert.True(EntradaCantidad.TryCalcular(texto, true, false, 10m, 2990m,
                    out decimal cantidad, out decimal subtotal, out _));
                Assert.Equal(0.350m, cantidad);
                Assert.Equal(1047m, subtotal);
            }
            finally { CultureInfo.CurrentCulture = anterior; }
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("-0,5")]
        [InlineData("0.0001")]
        [InlineData("1.000,5")]
        [InlineData("1,000.5")]
        [InlineData("1 000")]
        [InlineData("1e3")]
        [InlineData("999999999999999999999999999999999")]
        public void Entrada_invalida_no_se_convierte_en_un_kilo(string texto)
        {
            Assert.False(EntradaCantidad.TryCalcular(texto, true, false, 10m, 2990m,
                out decimal cantidad, out _, out string error));
            Assert.Equal(0m, cantidad);
            Assert.False(string.IsNullOrEmpty(error));
        }

        [Fact]
        public void Gramos_enteros_se_convierten_en_kilos()
        {
            Assert.True(EntradaCantidad.TryCalcular("125", true, true, 10m, 2990m,
                out decimal cantidad, out decimal subtotal, out _));
            Assert.Equal(0.125m, cantidad);
            Assert.Equal(374m, subtotal);
            Assert.False(EntradaCantidad.TryCalcular("0,5", true, true, 10m, 2990m, out _, out _, out _));
        }

        [Fact]
        public void Unidades_enteras_y_stock_disponible_se_validan()
        {
            Assert.True(EntradaCantidad.TryCalcular("2", false, false, 2m, 990m, out _, out decimal total, out _));
            Assert.Equal(1980m, total);
            Assert.False(EntradaCantidad.TryCalcular("0,5", false, false, 2m, 990m, out _, out _, out _));
            Assert.False(EntradaCantidad.TryCalcular("0,351", true, false, 0.350m, 2990m, out _, out _, out _));
            Assert.True(EntradaCantidad.TryCalcular("0,350", true, false, 0.350m, 2990m, out _, out _, out _));
            Assert.False(EntradaCantidad.TryCalcular("1", true, false, 0m, 2990m, out _, out _, out _));
        }

        [Fact]
        public void Importe_fuera_de_rango_se_rechaza()
        {
            Assert.False(EntradaCantidad.TryCalcular("2", true, false, 10m, decimal.MaxValue, out _, out _, out string error));
            Assert.Contains("demasiado grande", error);
        }
    }
}

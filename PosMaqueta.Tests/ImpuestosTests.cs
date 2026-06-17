using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    /// <summary>
    /// IVA chileno con precios que YA incluyen impuesto: Neto = total/1.19 (redondeo al peso,
    /// alejándose del cero) e Iva = total − Neto, de modo que el desglose siempre cuadra.
    /// </summary>
    public class ImpuestosTests
    {
        [Fact]
        public void TasaIva_es_19_por_ciento()
            => Assert.Equal(0.19m, Impuestos.TasaIva);

        [Theory]
        [InlineData(1190, 1000)]   // división exacta
        [InlineData(119, 100)]     // división exacta
        [InlineData(100, 84)]      // 84.03 -> 84
        [InlineData(1, 1)]         // 0.84 -> 1 (en $1 el IVA contenido es $0)
        [InlineData(0, 0)]
        [InlineData(990, 832)]     // 831.93 -> 832
        [InlineData(5430, 4563)]   // 4563.02 -> 4563
        public void Neto_desglosa_el_valor_sin_iva(decimal total, decimal netoEsperado)
            => Assert.Equal(netoEsperado, Impuestos.Neto(total));

        [Theory]
        [InlineData(1190, 190)]
        [InlineData(119, 19)]
        [InlineData(100, 16)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        public void Iva_es_el_complemento_del_neto(decimal total, decimal ivaEsperado)
            => Assert.Equal(ivaEsperado, Impuestos.Iva(total));

        // Invariante clave del modelo: el desglose nunca pierde ni inventa pesos.
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(990)]
        [InlineData(1190)]
        [InlineData(5430)]
        [InlineData(123457)]
        [InlineData(999999)]
        public void Neto_mas_Iva_siempre_es_igual_al_total(decimal total)
            => Assert.Equal(total, Impuestos.Neto(total) + Impuestos.Iva(total));

        // El redondeo es AwayFromZero (2.5 -> 3), no bancario (2). 2.975 / 1.19 == 2.5 exacto.
        [Fact]
        public void Neto_redondea_alejandose_del_cero_en_el_punto_medio()
            => Assert.Equal(3m, Impuestos.Neto(2.975m));

        [Fact]
        public void Neto_redondea_alejandose_del_cero_tambien_con_negativos()
            => Assert.Equal(-3m, Impuestos.Neto(-2.975m));

        // Devoluciones / notas de crédito suelen modelarse con montos negativos: el desglose debe cuadrar igual.
        [Theory]
        [InlineData(-100)]
        [InlineData(-1190)]
        [InlineData(-5430)]
        public void Neto_mas_Iva_cuadra_tambien_con_negativos(decimal total)
            => Assert.Equal(total, Impuestos.Neto(total) + Impuestos.Iva(total));

        [Fact]
        public void Iva_complemento_con_negativo_es_negativo()
            => Assert.Equal(-16m, Impuestos.Iva(-100m));

        // El modelo asume totales en pesos enteros. Este test FIJA el comportamiento con sub-pesos:
        // como Neto redondea al peso, Iva = total − Neto puede quedar fraccionario o incluso negativo.
        [Fact]
        public void Iva_con_montos_no_enteros_puede_quedar_fraccionario()
        {
            Assert.Equal(0m,      Impuestos.Neto(0.5m));
            Assert.Equal(0.5m,    Impuestos.Iva(0.5m));
            Assert.Equal(3m,      Impuestos.Neto(2.975m));
            Assert.Equal(-0.025m, Impuestos.Iva(2.975m));
        }
    }
}

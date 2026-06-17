using Dominio;
using Dominio.Servicios;
using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    public class CajaServiceTests : ServiciosTestBase
    {
        private readonly CajaService caja = new CajaService();
        private readonly ProductoService productos = new ProductoService();
        private readonly VentaService ventas = new VentaService();

        [Fact]
        public void AbrirCaja_marca_la_caja_como_abierta()
        {
            Assert.False(caja.HayCajaAbierta());
            int id = caja.AbrirCaja(1, 50000m);
            Assert.True(id > 0);
            Assert.True(caja.HayCajaAbierta());
            Assert.Equal(50000m, caja.ObtenerCajaAbierta().MontoInicial);
        }

        [Fact]
        public void AbrirCaja_con_otra_ya_abierta_lanza()
        {
            caja.AbrirCaja(1, 1000m);
            Assert.Throws<NegocioException>(() => caja.AbrirCaja(1, 2000m));
        }

        [Fact]
        public void AbrirCaja_con_monto_negativo_lanza()
            => Assert.Throws<NegocioException>(() => caja.AbrirCaja(1, -1m));

        [Fact]
        public void CerrarCaja_sin_caja_abierta_lanza()
            => Assert.Throws<NegocioException>(() => caja.CerrarCaja(1000m));

        [Fact]
        public void CerrarCaja_cuadrada_da_diferencia_cero()
        {
            caja.AbrirCaja(1, 50000m);
            int id = CrearProducto(productos, "Cola", 1190m, 10m);
            ventas.AgregarPorId(id, 1m);
            ventas.CobrarVenta(1, MedioPago.Efectivo);     // toma la caja abierta -> efectivo 1190

            Assert.Equal(0m, caja.CerrarCaja(51190m));     // 50000 + 1190
            Assert.False(caja.HayCajaAbierta());
        }

        [Fact]
        public void CerrarCaja_con_faltante_da_diferencia_negativa()
        {
            caja.AbrirCaja(1, 50000m);
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 1m);
            ventas.CobrarVenta(1, MedioPago.Efectivo);     // esperado 51000

            Assert.Equal(-500m, caja.CerrarCaja(50500m));  // contó 500 de menos
        }

        [Fact]
        public void Solo_el_efectivo_cuenta_para_el_esperado()
        {
            caja.AbrirCaja(1, 50000m);
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 1m);
            ventas.CobrarVenta(1, MedioPago.Tarjeta);      // tarjeta NO suma al efectivo esperado

            Assert.Equal(0m, caja.CerrarCaja(50000m));     // esperado sigue siendo 50000
        }
    }
}

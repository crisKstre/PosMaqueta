using System.Collections.Generic;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    public class DevolucionServiceTests : ServiciosTestBase
    {
        private readonly ProductoService    productos    = new ProductoService();
        private readonly VentaService       ventas       = new VentaService();
        private readonly DevolucionService  devoluciones = new DevolucionService();

        private List<DevolucionItem> Item(int idProd, decimal cant)
            => new List<DevolucionItem> { new DevolucionItem { IdProducto = idProd, Cantidad = cant } };

        [Fact]
        public void Devolver_reintegra_stock_y_baja_el_efectivo_esperado()
        {
            AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);   // stock 10->7, efectivo +3000
            Assert.Equal(7m, productos.ObtenerPorId(idProd).Stock);

            devoluciones.Devolver(idVenta, Item(idProd, 2m));          // devuelve 2
            Assert.Equal(9m, productos.ObtenerPorId(idProd).Stock);    // 7 + 2 reintegrado

            var caja = new CajaService();
            var abierta = caja.ObtenerCajaAbierta();
            var resumen = caja.ObtenerResumen(abierta.IdCaja);
            Assert.Equal(2000m, resumen.TotalDevoluciones);
            Assert.Equal(1000m, caja.CalcularEfectivoEsperado(abierta, resumen));   // 0 + 3000 − 2000
        }

        [Fact]
        public void Devolver_mas_de_lo_vendido_lanza()
        {
            AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 2m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            Assert.Throws<NegocioException>(() => devoluciones.Devolver(idVenta, Item(idProd, 3m)));
        }

        [Fact]
        public void Devolver_no_permite_doble_devolucion_del_mismo_item()
        {
            AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);

            devoluciones.Devolver(idVenta, Item(idProd, 2m));                 // quedan 1 devolvible
            Assert.Throws<NegocioException>(() => devoluciones.Devolver(idVenta, Item(idProd, 2m)));   // excede
            devoluciones.Devolver(idVenta, Item(idProd, 1m));                 // el último sí
            Assert.Empty(devoluciones.ObtenerDevolvibles(idVenta));
        }

        [Fact]
        public void Devolver_como_cajero_lanza()
        {
            AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 1m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);

            Sesion.UsuarioActual = new UsuarioService().ObtenerTodos().Find(u => u.LoginNombre == "empleado");
            Assert.Throws<NegocioException>(() => devoluciones.Devolver(idVenta, Item(idProd, 1m)));
        }
    }
}

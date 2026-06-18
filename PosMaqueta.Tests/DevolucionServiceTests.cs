using System;
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

            Sesion.UsuarioActual = CrearCajero();
            Assert.Throws<NegocioException>(() => devoluciones.Devolver(idVenta, Item(idProd, 1m)));
        }

        // C1 — coordinar anulación ↔ devolución: anular una venta que YA tuvo una devolución parcial
        // reintegraría el stock por segunda vez (doble reintegro). Debe rechazarse.
        [Fact]
        public void Anular_una_venta_con_devoluciones_lanza_y_no_duplica_stock()
        {
            AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);   // stock 10 -> 7
            devoluciones.Devolver(idVenta, Item(idProd, 2m));          // stock 7 -> 9
            Assert.Equal(9m, productos.ObtenerPorId(idProd).Stock);

            Assert.Throws<NegocioException>(() => ventas.AnularVenta(idVenta));
            Assert.Equal(9m, productos.ObtenerPorId(idProd).Stock);    // NO se sumó +3 extra
        }

        // C2 — el servicio debe impedir devolver una venta anulada (no solo la UI), si no reintegraría
        // stock y sacaría efectivo de una venta que ya fue revertida.
        [Fact]
        public void Devolver_una_venta_anulada_lanza()
        {
            AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);   // stock 10 -> 7
            ventas.AnularVenta(idVenta);                               // stock 7 -> 10
            Assert.Equal(10m, productos.ObtenerPorId(idProd).Stock);

            Assert.Throws<NegocioException>(() => devoluciones.Devolver(idVenta, Item(idProd, 1m)));
            Assert.Equal(10m, productos.ObtenerPorId(idProd).Stock);   // sin reintegro extra
        }

        // C7 — el resumen de ventas (Reportes) debe reflejar las devoluciones, igual que el arqueo de caja.
        [Fact]
        public void ResumenVentas_refleja_las_devoluciones()
        {
            AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);   // vendido 3000
            devoluciones.Devolver(idVenta, Item(idProd, 2m));          // devuelto 2000

            var hoy = DateTime.Today;
            var r = ventas.ObtenerResumenVentas(hoy, hoy);
            Assert.Equal(3000m, r.TotalVendido);
            Assert.Equal(2000m, r.TotalDevoluciones);
            Assert.Equal(1000m, r.TotalNeto);
        }
    }
}

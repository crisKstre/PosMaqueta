using System;
using System.Collections.Generic;
using AccesoData;
using AccesoData.DAO;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Microsoft.Data.Sqlite;
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

        [Theory]
        [InlineData(MedioPago.Efectivo)]
        [InlineData(MedioPago.Tarjeta)]
        [InlineData(MedioPago.Transferencia)]
        [InlineData(MedioPago.Mixto)]
        public void Devolver_conserva_venta_y_pagos_originales_y_reembolsa_en_efectivo(string medio)
        {
            int idCaja = AbrirCaja(5000m);
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 3m);
            var pagos = medio == MedioPago.Mixto
                ? new List<PagoVenta>
                {
                    new PagoVenta { MedioPago = MedioPago.Efectivo, Monto = 500m },
                    new PagoVenta { MedioPago = MedioPago.Tarjeta, Monto = 1500m },
                    new PagoVenta { MedioPago = MedioPago.Transferencia, Monto = 1000m }
                }
                : new List<PagoVenta> { new PagoVenta { MedioPago = medio, Monto = 3000m } };
            int idVenta = ventas.CobrarVenta(1, pagos);

            devoluciones.Devolver(idVenta, Item(idProd, 2m));

            var hoy = DateTime.Today;
            var venta = Assert.Single(new VentaDao().ObtenerVentas(hoy, hoy));
            Assert.Equal(3000m, venta.Total);
            var r = ventas.ObtenerResumenVentas(hoy, hoy);
            Assert.Equal(3000m, r.TotalVendido);
            Assert.Equal(1000m, r.TotalNeto);
            Assert.Equal(medio == MedioPago.Efectivo ? 3000m : medio == MedioPago.Mixto ? 500m : 0m, r.TotalEfectivo);
            Assert.Equal(medio == MedioPago.Tarjeta ? 3000m : medio == MedioPago.Mixto ? 1500m : 0m, r.TotalTarjeta);
            Assert.Equal(medio == MedioPago.Transferencia ? 3000m : medio == MedioPago.Mixto ? 1000m : 0m, r.TotalTransferencia);
            var cajas = new CajaService();
            var resumen = cajas.ObtenerResumen(idCaja);
            Assert.Equal(3000m, resumen.TotalVendido);
            Assert.Equal(2000m, resumen.TotalDevoluciones);
            Assert.Equal(5000m + r.TotalEfectivo - 2000m,
                cajas.CalcularEfectivoEsperado(cajas.ObtenerCajaAbierta(), resumen));
        }

        [Fact]
        public void Devoluciones_sucesivas_hasta_el_total_conservan_bruto_y_dejan_neto_cero()
        {
            AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            devoluciones.Devolver(idVenta, Item(idProd, 2m));
            devoluciones.Devolver(idVenta, Item(idProd, 1m));

            var r = ventas.ObtenerResumenVentas(DateTime.Today, DateTime.Today);
            Assert.Equal(1, r.CantidadVentas);
            Assert.Equal(3000m, r.TotalVendido);
            Assert.Equal(3000m, r.TotalDevoluciones);
            Assert.Equal(0m, r.TotalNeto);
            Assert.Equal(3000m, r.TicketPromedio);
            Assert.Equal(10m, productos.ObtenerPorId(idProd).Stock);
            Assert.Empty(devoluciones.ObtenerDevolvibles(idVenta));
        }

        [Fact]
        public void Devolucion_posterior_se_imputa_a_su_fecha_y_turno_sin_reescribir_la_venta()
        {
            var cajas = new CajaService();
            int cajaVenta = AbrirCaja();
            int idProd = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(idProd, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            Assert.Equal(0m, cajas.CerrarCaja(3000m));
            int cajaDevolucion = AbrirCaja(5000m);
            int idDev = devoluciones.Devolver(idVenta, Item(idProd, 2m));

            // Fechas fijas en la BD aislada: sin depender del reloj ni esperar otro día.
            var diaVenta = new DateTime(2026, 9, 20);
            var diaDevolucion = diaVenta.AddDays(1);
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Venta SET Fecha = @fecha WHERE IdVenta = @id;";
                    cmd.Parameters.AddWithValue("@fecha", "2026-09-20 10:00:00");
                    cmd.Parameters.AddWithValue("@id", idVenta);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Devolucion SET Fecha = @fecha WHERE IdDevolucion = @id;";
                    cmd.Parameters.AddWithValue("@fecha", "2026-09-21 12:00:00");
                    cmd.Parameters.AddWithValue("@id", idDev);
                    cmd.ExecuteNonQuery();
                }
            }

            var original = ventas.ObtenerResumenVentas(diaVenta, diaVenta);
            Assert.Equal(3000m, original.TotalVendido);
            Assert.Equal(0m, original.TotalDevoluciones);
            Assert.Equal(3000m, original.TotalNeto);
            var posterior = ventas.ObtenerResumenVentas(diaDevolucion, diaDevolucion);
            Assert.Equal(0, posterior.CantidadVentas);
            Assert.Equal(0m, posterior.TotalVendido);
            Assert.Equal(2000m, posterior.TotalDevoluciones);
            Assert.Equal(-2000m, posterior.TotalNeto);
            var ambos = ventas.ObtenerResumenVentas(diaVenta, diaDevolucion);
            Assert.Equal(3000m, ambos.TotalVendido);
            Assert.Equal(2000m, ambos.TotalDevoluciones);
            Assert.Equal(1000m, ambos.TotalNeto);
            Assert.Equal(3000m, cajas.ObtenerResumen(cajaVenta).TotalVendido);
            Assert.Equal(0m, cajas.ObtenerResumen(cajaVenta).TotalDevoluciones);
            var resumenDevolucion = cajas.ObtenerResumen(cajaDevolucion);
            Assert.Equal(0m, resumenDevolucion.TotalVendido);
            Assert.Equal(2000m, resumenDevolucion.TotalDevoluciones);
            Assert.Equal(3000m, cajas.CalcularEfectivoEsperado(cajas.ObtenerCajaAbierta(), resumenDevolucion));
        }
    }
}

using System;
using System.Collections.Generic;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    public class VentaServiceTests : ServiciosTestBase
    {
        private readonly ProductoService productos = new ProductoService();
        private readonly VentaService ventas = new VentaService();

        [Fact]
        public void AgregarPorId_agrega_con_precio_de_lista()
        {
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 2m);
            var item = Assert.Single(ventas.Carrito);
            Assert.Equal(1000m, item.PrecioUnitario);
            Assert.Equal(2000m, item.Subtotal);
            Assert.False(item.TieneDescuento);
        }

        [Fact]
        public void AgregarPorId_aplica_el_descuento_del_producto()
        {
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            productos.AplicarDescuento(id, 20m);
            ventas.AgregarPorId(id, 2m);
            var item = Assert.Single(ventas.Carrito);
            Assert.Equal(800m, item.PrecioUnitario);
            Assert.Equal(1000m, item.PrecioOriginal);
            Assert.Equal(20m, item.DescuentoPorcentaje);
            Assert.Equal(1600m, item.Subtotal);
        }

        [Fact]
        public void AgregarPorId_sin_stock_suficiente_lanza()
        {
            int id = CrearProducto(productos, "Poco", 100m, 3m);
            Assert.Throws<NegocioException>(() => ventas.AgregarPorId(id, 5m));
        }

        [Fact]
        public void AgregarPorId_cantidad_cero_lanza()
        {
            int id = CrearProducto(productos, "X", 100m, 5m);
            Assert.Throws<NegocioException>(() => ventas.AgregarPorId(id, 0m));
        }

        [Fact]
        public void AplicarDescuento_se_acota_al_subtotal()
        {
            int id = CrearProducto(productos, "X", 1000m, 10m);
            ventas.AgregarPorId(id, 2m);           // subtotal 2000
            ventas.AplicarDescuento(5000m);        // mayor que el subtotal
            Assert.Equal(2000m, ventas.Descuento); // se acota
            Assert.Equal(0m, ventas.Total);
        }

        // C9 — un descuento fraccionario tecleado a mano (100.50) no debe colar centavos en el Total.
        [Fact]
        public void AplicarDescuento_redondea_al_peso_entero()
        {
            int id = CrearProducto(productos, "X", 5000m, 10m);
            ventas.AgregarPorId(id, 1m);              // subtotal 5000
            ventas.AplicarDescuento(100.50m);         // fraccionario
            Assert.Equal(101m, ventas.Descuento);     // redondeado AwayFromZero
            Assert.Equal(4899m, ventas.Total);        // total entero (5000 - 101)
            Assert.Equal(ventas.Total, decimal.Truncate(ventas.Total));
        }

        [Fact]
        public void CobrarVenta_persiste_descuenta_stock_y_limpia_carrito()
        {
            AbrirCaja();
            int id = CrearProducto(productos, "Cola", 1190m, 10m);
            ventas.AgregarPorId(id, 2m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);

            Assert.True(idVenta > 0);
            Assert.Equal(8m, productos.ObtenerPorId(id).Stock);   // 10 - 2
            Assert.Empty(ventas.Carrito);                          // arranca un carrito nuevo, vacío

            var hoy = DateTime.Today;
            var v = Assert.Single(ventas.ObtenerVentas(hoy, hoy), x => x.IdVenta == idVenta);
            Assert.Equal(2380m, v.Total);
            Assert.Single(ventas.ObtenerDetalleVenta(idVenta));
        }

        // 0.C — venta por kilo: el subtotal/total se redondean al peso (sin medios pesos), para
        // que el arqueo y los reportes cuadren.
        [Fact]
        public void Venta_por_kilo_redondea_los_montos_al_peso()
        {
            AbrirCaja();
            int id = productos.Crear(new Producto
            {
                Nombre = "Tomate", Precio = 2990m, Stock = 100m,
                UnidadMedida = UnidadMedida.Kilogramo, Categoria = "Bebidas"
            });
            ventas.AgregarPorId(id, 0.350m);                 // 0.350 Kg × 2990 = 1046.5
            Assert.Equal(1047m, ventas.Subtotal);            // redondeo AwayFromZero
            Assert.Equal(1047m, ventas.Total);

            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            var v = Assert.Single(ventas.ObtenerVentas(DateTime.Today, DateTime.Today), x => x.IdVenta == idVenta);
            Assert.Equal(1047m, v.Total);
            Assert.Equal(v.Total, decimal.Truncate(v.Total));   // sin fracción de peso
        }

        // 4.C — pago mixto: el total se reparte entre medios; el arqueo solo cuenta la parte en efectivo.
        [Fact]
        public void CobrarVenta_pago_mixto_registra_y_desglosa_en_el_arqueo()
        {
            int idCaja = AbrirCaja(0m);
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 1m);     // total 1000
            var pagos = new List<PagoVenta>
            {
                new PagoVenta { MedioPago = MedioPago.Efectivo, Monto = 600m },
                new PagoVenta { MedioPago = MedioPago.Tarjeta,  Monto = 400m },
            };
            int idVenta = ventas.CobrarVenta(1, pagos);

            var v = Assert.Single(ventas.ObtenerVentas(DateTime.Today, DateTime.Today), x => x.IdVenta == idVenta);
            Assert.Equal(MedioPago.Mixto, v.MedioPago);

            var resumen = new CajaService().ObtenerResumen(idCaja);
            Assert.Equal(600m, resumen.TotalEfectivo);   // solo el efectivo va al cajón
            Assert.Equal(400m, resumen.TotalTarjeta);
        }

        [Fact]
        public void CobrarVenta_pagos_que_no_cuadran_con_el_total_lanza()
        {
            AbrirCaja();
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 1m);     // total 1000
            var pagos = new List<PagoVenta> { new PagoVenta { MedioPago = MedioPago.Efectivo, Monto = 800m } };
            Assert.Throws<NegocioException>(() => ventas.CobrarVenta(1, pagos));   // faltan $200
        }

        [Fact]
        public void CobrarVenta_con_carrito_vacio_lanza()
            => Assert.Throws<NegocioException>(() => ventas.CobrarVenta(1, MedioPago.Efectivo));

        [Fact]
        public void CobrarVenta_sin_caja_abierta_lanza()
        {
            int id = CrearProducto(productos, "X", 1000m, 5m);
            ventas.AgregarPorId(id, 1m);
            var ex = Assert.Throws<NegocioException>(() => ventas.CobrarVenta(1, MedioPago.Efectivo));
            Assert.Contains("caja", ex.Message.ToLowerInvariant());
        }

        [Fact]
        public void CobrarVenta_combina_descuento_de_producto_y_de_total()
        {
            AbrirCaja();
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            productos.AplicarDescuento(id, 10m);     // 900 c/u
            ventas.AgregarPorId(id, 2m);             // subtotal 1800
            ventas.AplicarDescuento(300m);           // total 1500
            int idVenta = ventas.CobrarVenta(1, MedioPago.Tarjeta);

            var v = Assert.Single(ventas.ObtenerVentas(DateTime.Today, DateTime.Today), x => x.IdVenta == idVenta);
            Assert.Equal(1500m, v.Total);
            Assert.Equal(300m, v.Descuento);
        }

        [Fact]
        public void AnularVenta_devuelve_el_stock_y_la_excluye_de_los_reportes()
        {
            AbrirCaja();
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            Assert.Equal(7m, productos.ObtenerPorId(id).Stock);

            ventas.AnularVenta(idVenta);
            Assert.Equal(10m, productos.ObtenerPorId(id).Stock);   // stock devuelto
            Assert.DoesNotContain(ventas.ObtenerVentas(DateTime.Today, DateTime.Today), x => x.IdVenta == idVenta);
        }

        // 3.A / C13 — un cajero no puede anular ventas aunque invoque el servicio directo.
        [Fact]
        public void AnularVenta_como_cajero_lanza()
        {
            AbrirCaja();
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 1m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);

            Sesion.UsuarioActual = CrearCajero();
            Assert.Throws<NegocioException>(() => ventas.AnularVenta(idVenta));
        }

        [Fact]
        public void CambiarCantidad_recalcula_subtotal_y_cero_quita_el_item()
        {
            int id = CrearProducto(productos, "X", 500m, 10m);
            ventas.AgregarPorId(id, 2m);
            ventas.CambiarCantidad(id, 4m);
            Assert.Equal(2000m, Assert.Single(ventas.Carrito).Subtotal);
            ventas.CambiarCantidad(id, 0m);
            Assert.Empty(ventas.Carrito);
        }

        [Fact]
        public void Multi_venta_mantiene_carritos_separados()
        {
            int id = CrearProducto(productos, "X", 100m, 50m);
            ventas.AgregarPorId(id, 1m);              // venta activa #1
            ventas.NuevaVenta();                       // nueva activa #2 (vacía)
            Assert.Empty(ventas.Carrito);
            ventas.AgregarPorId(id, 5m);
            Assert.Equal(5m, Assert.Single(ventas.Carrito).Cantidad);
            Assert.Equal(2, ventas.VentasEnCurso.Count);
        }

        [Fact]
        public void CerrarPausadasInactivas_cierra_las_vencidas_pero_no_la_activa()
        {
            int id = CrearProducto(productos, "X", 100m, 50m);
            ventas.AgregarPorId(id, 1m);             // v1
            var pausada = ventas.Activa;
            ventas.NuevaVenta();                      // v2 queda activa; v1 pasa a pausa
            pausada.UltimaActividad = DateTime.Now.AddMinutes(-20);

            int cerradas = ventas.CerrarPausadasInactivas(TimeSpan.FromMinutes(10));
            Assert.Equal(1, cerradas);
            Assert.Single(ventas.VentasEnCurso);
        }

        // ── Regresión de bugs hallados en la auditoría ────────────────────

        // Dos ventas en curso "reservan" el mismo Stock=1; al cobrar ambas el stock NO debe quedar negativo.
        [Fact]
        public void CobrarVenta_no_deja_stock_negativo_con_dos_ventas()
        {
            AbrirCaja();
            int id = CrearProducto(productos, "Cola", 1000m, 1m);   // stock 1
            ventas.AgregarPorId(id, 1m);                            // venta 1 reserva 1
            var v1 = ventas.Activa;
            ventas.NuevaVenta();
            ventas.AgregarPorId(id, 1m);                            // venta 2 también (la validación al agregar no ve la otra)
            ventas.CobrarVenta(1, MedioPago.Efectivo);             // venta 2 -> stock 0
            Assert.Equal(0m, productos.ObtenerPorId(id).Stock);

            ventas.ActivarVenta(v1.Id);
            Assert.Throws<NegocioException>(() => ventas.CobrarVenta(1, MedioPago.Efectivo)); // ya no hay stock
            Assert.Equal(0m, productos.ObtenerPorId(id).Stock);    // NO quedó negativo
        }

        // El carrito quedó obsoleto (otro módulo bajó el stock): el cobro se rechaza y no descuenta.
        [Fact]
        public void CobrarVenta_con_stock_desactualizado_lanza_y_no_descuenta()
        {
            int id = CrearProducto(productos, "X", 1000m, 5m);
            ventas.AgregarPorId(id, 5m);
            productos.AjustarStock(id, -4m);                       // otro módulo lo deja en 1
            Assert.Throws<NegocioException>(() => ventas.CobrarVenta(1, MedioPago.Efectivo));
            Assert.Equal(1m, productos.ObtenerPorId(id).Stock);   // intacto
        }

        // Anular dos veces no debe devolver el stock por duplicado.
        [Fact]
        public void AnularVenta_dos_veces_no_duplica_el_stock()
        {
            AbrirCaja();
            int id = CrearProducto(productos, "Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            ventas.AnularVenta(idVenta);
            Assert.Equal(10m, productos.ObtenerPorId(id).Stock);   // stock devuelto

            Assert.Throws<NegocioException>(() => ventas.AnularVenta(idVenta));  // segunda anulación
            Assert.Equal(10m, productos.ObtenerPorId(id).Stock);   // NO se duplicó
        }

        // Si se reduce el carrito tras aplicar un descuento grande, el descuento efectivo se reacota.
        [Fact]
        public void Descuento_se_reajusta_cuando_baja_el_subtotal()
        {
            AbrirCaja();
            int a = CrearProducto(productos, "A", 6000m, 10m);
            int b = CrearProducto(productos, "B", 4000m, 10m);
            ventas.AgregarPorId(a, 1m);
            ventas.AgregarPorId(b, 1m);               // subtotal 10000
            ventas.AplicarDescuento(8000m);           // total 2000
            ventas.QuitarDelCarrito(b);               // subtotal baja a 6000
            Assert.Equal(6000m, ventas.Subtotal);
            Assert.Equal(6000m, ventas.Descuento);    // el descuento efectivo se acota (era 8000)

            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            var v = Assert.Single(ventas.ObtenerVentas(DateTime.Today, DateTime.Today), x => x.IdVenta == idVenta);
            Assert.Equal(6000m, v.Descuento);         // se persiste el descuento acotado, no 8000
            Assert.Equal(0m, v.Total);
        }

        // ── Costo / utilidad ──────────────────────────────────────────────
        private int CrearConCosto(string nombre, decimal precio, decimal costo, decimal stock = 100m)
            => productos.Crear(new Producto { Nombre = nombre, Precio = precio, Costo = costo, Stock = stock,
                UnidadMedida = UnidadMedida.Unidad, Categoria = "Bebidas" });

        [Fact]
        public void CobrarVenta_guarda_el_costo_unitario_como_snapshot()
        {
            AbrirCaja();
            int id = CrearConCosto("Cola", 1000m, 600m);
            ventas.AgregarPorId(id, 2m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            var det = Assert.Single(ventas.ObtenerDetalleVenta(idVenta));
            Assert.Equal(600m, det.CostoUnitario);
        }

        [Fact]
        public void Cambiar_el_costo_despues_de_vender_no_altera_la_utilidad_historica()
        {
            AbrirCaja();
            int id = CrearConCosto("Cola", 1000m, 600m);
            ventas.AgregarPorId(id, 1m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);

            var p = productos.ObtenerPorId(id); p.Costo = 950m; productos.Actualizar(p);  // sube el costo después
            var det = Assert.Single(ventas.ObtenerDetalleVenta(idVenta));
            Assert.Equal(600m, det.CostoUnitario);   // el snapshot del momento de la venta no cambia
        }

        [Fact]
        public void ResumenVentas_calcula_utilidad_y_margen()
        {
            AbrirCaja();
            int id = CrearConCosto("Cola", 1000m, 600m);
            ventas.AgregarPorId(id, 2m);              // vende 2000, costo 1200
            ventas.CobrarVenta(1, MedioPago.Efectivo);
            var hoy = DateTime.Today;
            var r = ventas.ObtenerResumenVentas(hoy, hoy);
            Assert.Equal(2000m, r.TotalVendido);
            Assert.Equal(1200m, r.TotalCosto);
            Assert.Equal(800m,  r.Utilidad);
            Assert.Equal(40m,   r.MargenPorcentaje);  // 800 / 2000 * 100
        }

        [Fact]
        public void ObtenerTopUtilidad_ordena_por_utilidad_no_por_cantidad()
        {
            AbrirCaja();
            int barato = CrearConCosto("MuchaVenta",   1000m, 950m);  // margen 50 c/u
            int caro   = CrearConCosto("MuchaUtilidad", 2000m, 500m); // margen 1500 c/u
            ventas.AgregarPorId(barato, 5m); ventas.CobrarVenta(1, MedioPago.Efectivo);  // utilidad 250
            ventas.AgregarPorId(caro, 1m);   ventas.CobrarVenta(1, MedioPago.Efectivo);  // utilidad 1500

            var hoy = DateTime.Today;
            var top = ventas.ObtenerTopUtilidad(hoy, hoy, 10);
            Assert.Equal("MuchaUtilidad", top[0].Nombre);
            Assert.Equal(1500m, top[0].Utilidad);
        }
    }
}

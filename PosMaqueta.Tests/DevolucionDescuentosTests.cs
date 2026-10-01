using System;
using System.Collections.Generic;
using System.Linq;
using AccesoData;
using AccesoData.DAO;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Microsoft.Data.Sqlite;
using Xunit;

namespace PosMaqueta.Tests
{
    public class DevolucionDescuentosTests : ServiciosTestBase
    {
        private readonly ProductoService productos = new ProductoService();
        private readonly VentaService ventas = new VentaService();
        private readonly DevolucionService devoluciones = new DevolucionService();

        private static List<DevolucionItem> Item(int id, decimal cantidad)
            => new List<DevolucionItem> { new DevolucionItem { IdProducto = id, Cantidad = cantidad } };

        private decimal Devuelto() => ventas.ObtenerResumenVentas(DateTime.Today, DateTime.Today).TotalDevoluciones;

        [Theory]
        [InlineData(0, 1000, 1000)]
        [InlineData(10, 300, 1500)]
        public void Descuentos_se_respetan_en_devoluciones_parciales_y_total(int oferta, int descuento, int cobrado)
        {
            AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            productos.AplicarDescuento(p, oferta);
            ventas.AgregarPorId(p, 2m);
            ventas.AplicarDescuento(descuento);
            int venta = ventas.CobrarVenta(1, MedioPago.Tarjeta);
            Assert.Equal(cobrado, Assert.Single(devoluciones.ObtenerDevolvibles(venta)).Subtotal);
            // El precio aportado por el llamador no es autoritativo.
            var pedido = Item(p, 1m);
            pedido[0].PrecioUnitario = 99999m;
            pedido[0].Subtotal = 99999m;
            devoluciones.Devolver(venta, pedido);
            Assert.Equal(cobrado / 2m, Devuelto());
            devoluciones.Devolver(venta, Item(p, 1m));
            Assert.Equal(cobrado, Devuelto());
            Assert.Equal(0m, ventas.ObtenerResumenVentas(DateTime.Today, DateTime.Today).TotalNeto);
            Assert.Equal(10m, productos.ObtenerPorId(p).Stock);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Producto_repetido_se_rechaza_sin_movimientos(int cantidad)
        {
            AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 3m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            var pedido = Item(p, cantidad);
            pedido.Add(new DevolucionItem { IdProducto = p, Cantidad = cantidad });
            Assert.Throws<NegocioException>(() => devoluciones.Devolver(venta, pedido));
            Assert.Equal(7m, productos.ObtenerPorId(p).Stock);
            Assert.Equal(0m, Devuelto());
            Assert.False(new DevolucionDao().TieneDevoluciones(venta));
        }

        [Fact]
        public void Descuento_total_permite_reintegrar_stock_sin_reembolsar_dinero()
        {
            AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 2m);
            ventas.AplicarDescuento(2000m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            devoluciones.Devolver(venta, Item(p, 2m));
            Assert.Equal(0m, Devuelto());
            Assert.Equal(10m, productos.ObtenerPorId(p).Stock);
            Assert.Empty(devoluciones.ObtenerDevolvibles(venta));
        }

        [Fact]
        public void Kilos_en_varias_devoluciones_no_acumulan_redondeos_de_mas()
        {
            AbrirCaja();
            int p = productos.Crear(new Producto { Nombre = "Granel", Precio = 5m, Stock = 10m,
                UnidadMedida = UnidadMedida.Kilogramo, Categoria = "Bebidas" });
            ventas.AgregarPorId(p, 1m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            var esperados = new[] { 1m, 1m, 2m, 2m, 3m, 3m, 4m, 4m, 5m, 5m };
            foreach (decimal esperado in esperados)
            {
                devoluciones.Devolver(venta, Item(p, 0.1m));
                Assert.Equal(esperado, Devuelto());
            }
            Assert.Equal(10m, productos.ObtenerPorId(p).Stock);
            Assert.Empty(devoluciones.ObtenerDevolvibles(venta));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Residuo_del_descuento_se_asigna_por_producto_independiente_del_orden_de_devolucion(bool inverso)
        {
            AbrirCaja();
            int a = CrearProducto(productos, "A", 100m, 10m);
            int b = CrearProducto(productos, "B", 100m, 10m);
            int c = CrearProducto(productos, "C", 100m, 10m);
            foreach (int p in new[] { c, a, b }) ventas.AgregarPorId(p, 1m);
            ventas.AplicarDescuento(1m); // 299: cupos A=100, B=100, C=99.
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            var disponibles = devoluciones.ObtenerDevolvibles(venta).ToDictionary(i => i.IdProducto);
            Assert.Equal(100m, disponibles[a].Subtotal);
            Assert.Equal(100m, disponibles[b].Subtotal);
            Assert.Equal(99m, disponibles[c].Subtotal);
            foreach (int p in inverso ? new[] { c, b, a } : new[] { a, b, c })
                devoluciones.Devolver(venta, Item(p, 1m));
            Assert.Equal(299m, Devuelto());
        }

        [Fact]
        public void Historico_reembolsado_de_mas_se_bloquea_para_conciliacion()
        {
            int caja = AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 2m);
            ventas.AplicarDescuento(1000m); // cobrado 1000, presupuesto por unidad 500
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            new DevolucionDao().Registrar(new Devolucion { IdVenta = venta, IdCaja = caja,
                IdUsuario = Sesion.UsuarioActual.IdUsuario, Fecha = DateTime.Now, Monto = 1000m,
                Detalles = new List<DevolucionItem> { new DevolucionItem {
                    IdProducto = p, Cantidad = 1m, Subtotal = 1000m } } });
            Assert.Throws<NegocioException>(() => devoluciones.Devolver(venta, Item(p, 1m)));
            Assert.Equal(1000m, Devuelto());
            Assert.Equal(9m, productos.ObtenerPorId(p).Stock);
        }

        [Fact]
        public void Vista_previa_coincide_con_registro_y_no_modifica_stock_ni_devoluciones()
        {
            AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 3m);
            ventas.AplicarDescuento(1000m); // cuotas acumuladas: 667, 1333, 2000
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            var pedido = Item(p, 1m);
            Assert.Equal(667m, Assert.Single(devoluciones.Previsualizar(venta, pedido)).Subtotal);
            Assert.Equal(7m, productos.ObtenerPorId(p).Stock);
            Assert.Equal(0m, Devuelto());
            devoluciones.Devolver(venta, pedido, 667m);
            Assert.Equal(667m, Devuelto());
            Assert.Equal(666m, Assert.Single(devoluciones.Previsualizar(venta, pedido)).Subtotal);
            Assert.Throws<NegocioException>(() => devoluciones.Devolver(venta, pedido, 667m));
            Assert.Equal(667m, Devuelto());
            Assert.Equal(8m, productos.ObtenerPorId(p).Stock);
            devoluciones.Devolver(venta, pedido, 666m);
            devoluciones.Devolver(venta, pedido, 667m);
            Assert.Equal(2000m, Devuelto());
        }

        [Fact]
        public void Cambio_de_precio_actual_no_modifica_reembolso_original()
        {
            AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 2m);
            ventas.AplicarDescuento(500m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            productos.AplicarDescuento(p, 90m);
            devoluciones.Devolver(venta, Item(p, 2m));
            Assert.Equal(1500m, Devuelto());
        }

        [Fact]
        public void Total_legacy_reducido_no_se_resta_otra_vez_del_reembolso_disponible()
        {
            AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 3m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            devoluciones.Devolver(venta, Item(p, 1m));
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Venta SET Total = 2000 WHERE IdVenta = @id;";
                    cmd.Parameters.AddWithValue("@id", venta);
                    cmd.ExecuteNonQuery();
                }
            }
            devoluciones.Devolver(venta, Item(p, 2m));
            Assert.Equal(3000m, Devuelto());
            Assert.Equal(10m, productos.ObtenerPorId(p).Stock);
            // Calcular una devolución no repara el dato histórico ni sus reportes.
            Assert.Equal(2000m, Assert.Single(ventas.ObtenerVentasHoy()).Total);
        }
    }
}

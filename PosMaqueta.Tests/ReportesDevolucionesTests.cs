using System;
using System.Collections.Generic;
using System.Linq;
using AccesoData;
using AccesoData.DAO;
using Dominio.Servicios;
using Entidades;
using Microsoft.Data.Sqlite;
using Xunit;

namespace PosMaqueta.Tests
{
    public class ReportesDevolucionesTests : ServiciosTestBase
    {
        private readonly ProductoService productos = new ProductoService();
        private readonly VentaService ventas = new VentaService();
        private readonly DevolucionService devoluciones = new DevolucionService();
        private readonly DateTime hoy = DateTime.Today;

        private int Producto(string nombre, decimal precio, decimal costo)
            => productos.Crear(new Producto { Nombre = nombre, Precio = precio, Costo = costo,
                Stock = 10m, UnidadMedida = UnidadMedida.Kilogramo, Categoria = "Bebidas" });
        private int Devolver(int venta, int producto, decimal cantidad)
            => devoluciones.Devolver(venta, new List<DevolucionItem> {
                new DevolucionItem { IdProducto = producto, Cantidad = cantidad } });

        [Fact]
        public void Devolucion_parcial_reduce_costo_utilidad_y_ranking_con_costo_original()
        {
            AbrirCaja();
            int p = Producto("Pan", 1000m, 600m);
            ventas.AgregarPorId(p, 3m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            var producto = productos.ObtenerPorId(p);
            producto.Costo = 900m;
            productos.Actualizar(producto);
            Devolver(venta, p, 2m);
            var r = ventas.ObtenerResumenVentas(hoy, hoy);
            Assert.Equal(400m, r.Utilidad);
            Assert.Equal(1800m, r.TotalCosto);
            Assert.Equal(1200m, r.TotalCostoDevuelto);
            Assert.Equal(600m, r.TotalCostoNeto);
            Assert.Equal(40m, r.MargenPorcentaje);
            var top = Assert.Single(ventas.ObtenerTopUtilidad(hoy, hoy));
            Assert.Equal(1m, top.Cantidad);
            Assert.Equal(1000m, top.Total);
            Assert.Equal(600m, top.Costo);
            Assert.Equal(r.Utilidad, top.Utilidad);
        }

        [Fact]
        public void Devolucion_total_deja_costo_y_utilidad_netos_cero()
        {
            AbrirCaja();
            int p = Producto("Pan", 1000m, 600m);
            ventas.AgregarPorId(p, 3m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            Devolver(venta, p, 1m);
            Devolver(venta, p, 2m);
            Assert.Equal(0m, ventas.ObtenerResumenVentas(hoy, hoy).Utilidad);
            var top = Assert.Single(ventas.ObtenerTopProductos(hoy, hoy));
            Assert.Equal(0m, top.Cantidad);
            Assert.Equal(0m, top.Total);
            Assert.Equal(0m, top.Costo);
        }

        [Fact]
        public void Rankings_reparten_descuento_global_y_restan_reembolso_real()
        {
            AbrirCaja();
            int a = Producto("A", 100m, 40m);
            int b = Producto("B", 100m, 40m);
            int c = Producto("C", 100m, 40m);
            foreach (int p in new[] { c, b, a }) ventas.AgregarPorId(p, 1m);
            ventas.AplicarDescuento(1m);
            int venta = ventas.CobrarVenta(1, MedioPago.Tarjeta);
            var antes = ventas.ObtenerTopProductos(hoy, hoy);
            Assert.Equal(299m, antes.Sum(p => p.Total));
            Assert.Equal(99m, antes.Single(p => p.Nombre == "C").Total);
            Devolver(venta, c, 1m);
            var despues = ventas.ObtenerTopProductos(hoy, hoy);
            Assert.Equal(200m, despues.Sum(p => p.Total));
            Assert.Equal(80m, despues.Sum(p => p.Costo));
            Assert.Equal(120m, ventas.ObtenerResumenVentas(hoy, hoy).Utilidad);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Ranking_ordena_y_limita_despues_de_descontar_devoluciones(bool porUtilidad)
        {
            AbrirCaja();
            int a = Producto("AntesPrimero", 2000m, 100m);
            int b = Producto("AhoraPrimero", 1000m, 100m);
            ventas.AgregarPorId(a, 3m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            ventas.AgregarPorId(b, 2m);
            ventas.CobrarVenta(1, MedioPago.Efectivo);
            Devolver(venta, a, 3m);
            var top = porUtilidad ? ventas.ObtenerTopUtilidad(hoy, hoy, 1) : ventas.ObtenerTopProductos(hoy, hoy, 1);
            Assert.Equal("AhoraPrimero", Assert.Single(top).Nombre);
        }

        [Fact]
        public void Devolucion_en_otro_dia_no_reescribe_costo_del_dia_de_venta()
        {
            AbrirCaja();
            int p = Producto("Pan", 1000m, 600m);
            ventas.AgregarPorId(p, 3m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            int dev = Devolver(venta, p, 2m);
            Fechar(venta, dev);
            var ayer = hoy.AddDays(-1);
            Assert.Equal(1200m, ventas.ObtenerResumenVentas(ayer, ayer).Utilidad);
            var r = ventas.ObtenerResumenVentas(hoy, hoy);
            Assert.Equal(-800m, r.Utilidad);
            Assert.Equal(0m, r.TotalCosto);
            Assert.Equal(1200m, r.TotalCostoDevuelto);
            Assert.Equal(-1200m, r.TotalCostoNeto);
            Assert.False(r.TieneMargen);
            Assert.Equal(0m, r.MargenPorcentaje); // no se expresa porcentaje con ingreso neto <= 0
            var top = Assert.Single(ventas.ObtenerTopProductos(hoy, hoy));
            Assert.Equal(-2m, top.Cantidad);
            Assert.Equal(-2000m, top.Total);
            Assert.Equal(-1200m, top.Costo);
            Assert.Equal(400m, ventas.ObtenerResumenVentas(ayer, hoy).Utilidad);
        }

        [Fact]
        public void Costo_fraccionario_se_conserva_y_ventas_anuladas_no_entran_en_ranking()
        {
            AbrirCaja();
            int p = Producto("Granel", 2990m, 601m);
            ventas.AgregarPorId(p, 0.350m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            Devolver(venta, p, 0.100m);
            ventas.AgregarPorId(p, 1m);
            ventas.AnularVenta(ventas.CobrarVenta(1, MedioPago.Efectivo));
            var top = Assert.Single(ventas.ObtenerTopProductos(hoy, hoy));
            Assert.Equal(0.250m, top.Cantidad);
            Assert.Equal(150.25m, top.Costo);
            Assert.Equal(748m, top.Total); // 1047 - redondear(1047 * 0,1 / 0,35) = 748
            Assert.Equal(597.75m, ventas.ObtenerResumenVentas(hoy, hoy).Utilidad);
        }

        [Fact]
        public void Lineas_historicas_repetidas_usan_costo_ponderado_y_cuadran_entre_periodos()
        {
            int caja = AbrirCaja();
            int p = Producto("Histórico", 1000m, 99m);
            int venta = new VentaDao().RegistrarVenta(new Venta { IdCaja = caja, IdUsuario = 1,
                Fecha = hoy.AddDays(-3), Total = 3000m, MedioPago = MedioPago.Efectivo,
                Pagos = new List<PagoVenta> { new PagoVenta { MedioPago = MedioPago.Efectivo, Monto = 3000m } },
                Detalles = new List<DetalleVenta> {
                    new DetalleVenta { IdProducto = p, Cantidad = 1m, Subtotal = 1000m, PrecioUnitario = 1000m, CostoUnitario = 1m },
                    new DetalleVenta { IdProducto = p, Cantidad = 2m, Subtotal = 2000m, PrecioUnitario = 1000m, CostoUnitario = 0.5m }
                } });
            for (int i = 2; i >= 0; i--)
            {
                int dev = Devolver(venta, p, 1m);
                using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE Devolucion SET Fecha = @fecha WHERE IdDevolucion = @id;";
                        cmd.Parameters.AddWithValue("@fecha", hoy.AddDays(-i).ToString("yyyy-MM-dd 12:00:00"));
                        cmd.Parameters.AddWithValue("@id", dev);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            var juntos = ventas.ObtenerResumenVentas(hoy.AddDays(-3), hoy);
            Assert.Equal(2m, juntos.TotalCosto);
            Assert.Equal(2m, juntos.TotalCostoDevuelto);
            Assert.Equal(0m, juntos.TotalCostoNeto);
            Assert.Equal(0m, juntos.Utilidad);
            decimal costosDevueltos = 0;
            for (int i = 2; i >= 0; i--)
                costosDevueltos += ventas.ObtenerResumenVentas(hoy.AddDays(-i), hoy.AddDays(-i)).TotalCostoDevuelto;
            Assert.Equal(2m, costosDevueltos);
            Assert.Equal(0m, Assert.Single(ventas.ObtenerTopProductos(hoy.AddDays(-3), hoy)).Costo);
        }

        [Fact]
        public void Ranking_resta_el_importe_historico_real_aunque_ignorara_descuentos()
        {
            int caja = AbrirCaja();
            int p = Producto("Histórico", 1000m, 300m);
            ventas.AgregarPorId(p, 2m);
            ventas.AplicarDescuento(1000m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            new DevolucionDao().Registrar(new Devolucion { IdVenta = venta, IdCaja = caja,
                IdUsuario = 1, Fecha = DateTime.Now, Monto = 1000m,
                Detalles = new List<DevolucionItem> { new DevolucionItem { IdProducto = p, Cantidad = 1m, Subtotal = 1000m } } });
            var top = Assert.Single(ventas.ObtenerTopUtilidad(hoy, hoy));
            Assert.Equal(0m, top.Total); // No sustituir el reembolso guardado por la cuota nueva de 500.
            Assert.Equal(300m, top.Costo);
            Assert.Equal(-300m, top.Utilidad);
            Assert.Equal(top.Utilidad, ventas.ObtenerResumenVentas(hoy, hoy).Utilidad);
        }

        [Fact]
        public void Descuento_total_y_costo_desconocido_no_generan_importes_inventados()
        {
            AbrirCaja();
            int p = Producto("Sin costo", 1000m, 0m);
            ventas.AgregarPorId(p, 2m);
            ventas.AplicarDescuento(2000m);
            int venta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            Devolver(venta, p, 1m);
            var r = ventas.ObtenerResumenVentas(hoy, hoy);
            Assert.Equal(0m, r.TotalCostoNeto);
            Assert.Equal(0m, r.Utilidad);
            Assert.False(r.TieneMargen);
            var top = Assert.Single(ventas.ObtenerTopProductos(hoy, hoy));
            Assert.Equal(1m, top.Cantidad);
            Assert.Equal(0m, top.Total);
        }

        [Fact]
        public void Reporte_vacio_y_limite_cero_devuelven_resultados_vacios()
        {
            var r = ventas.ObtenerResumenVentas(hoy, hoy);
            Assert.Equal(0m, r.Utilidad);
            Assert.Equal(0m, r.TotalCostoNeto);
            Assert.Empty(ventas.ObtenerTopProductos(hoy, hoy));
            AbrirCaja();
            int p = Producto("Pan", 1000m, 500m);
            ventas.AgregarPorId(p, 1m);
            ventas.CobrarVenta(1, MedioPago.Efectivo);
            Assert.Empty(ventas.ObtenerTopProductos(hoy, hoy, 0));
            Assert.Empty(ventas.ObtenerTopUtilidad(hoy, hoy, 0));
        }

        private void Fechar(int venta, int devolucion)
        {
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Venta SET Fecha = @ayer WHERE IdVenta = @venta; UPDATE Devolucion SET Fecha = @hoy WHERE IdDevolucion = @dev;";
                    cmd.Parameters.AddWithValue("@ayer", hoy.AddDays(-1).ToString("yyyy-MM-dd 12:00:00"));
                    cmd.Parameters.AddWithValue("@hoy", hoy.ToString("yyyy-MM-dd 12:00:00"));
                    cmd.Parameters.AddWithValue("@venta", venta);
                    cmd.Parameters.AddWithValue("@dev", devolucion);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

using System;
using AccesoData;
using Dominio;
using Dominio.Eventos;
using Dominio.Servicios;
using Entidades;
using Microsoft.Data.Sqlite;
using Xunit;

namespace PosMaqueta.Tests
{
    public class ConfirmacionVentaTests : ServiciosTestBase
    {
        private readonly VentaService ventas = new VentaService();
        private readonly ProductoService productos = new ProductoService();

        private void Sql(string sql)
        {
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand()) { cmd.CommandText = sql; cmd.ExecuteNonQuery(); }
            }
        }

        private long Contar(string tabla)
        {
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM " + tabla;
                    return Convert.ToInt64(cmd.ExecuteScalar());
                }
            }
        }

        [Fact]
        public void Fallo_de_auditoria_revierte_venta_pagos_stock_y_permite_un_solo_reintento()
        {
            AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 2m);
            Sql(@"CREATE TRIGGER fallo_auditoria BEFORE INSERT ON LogMovimiento
                  WHEN NEW.Accion = 'Venta' BEGIN SELECT RAISE(ABORT, 'fallo simulado'); END;");
            Assert.ThrowsAny<Exception>(() => ventas.CobrarVenta(1, MedioPago.Efectivo));
            Assert.Equal(0, Contar("Venta"));
            Assert.Equal(0, Contar("DetalleVenta"));
            Assert.Equal(0, Contar("PagoVenta"));
            Assert.Equal(10m, productos.ObtenerPorId(p).Stock);
            Assert.Equal(2m, Assert.Single(ventas.Carrito).Cantidad);
            Sql("DROP TRIGGER fallo_auditoria;");
            int id = ventas.CobrarVenta(1, MedioPago.Efectivo);
            Assert.True(id > 0);
            Assert.Equal(1, Contar("Venta"));
            Assert.Equal(1, Contar("PagoVenta"));
            Assert.Equal(8m, productos.ObtenerPorId(p).Stock);
            Assert.Empty(ventas.Carrito);
            var log = Assert.Single(new LogService().Obtener(DateTime.Today, DateTime.Today, modulo: ModuloLog.Ventas),
                l => l.Accion == "Venta");
            Assert.Contains("N°" + id, log.Detalle);
            Assert.Equal(1, log.IdUsuario);
        }

        [Fact]
        public void Cobrar_conserva_el_carrito_pausado_y_audita_al_usuario_de_la_venta()
        {
            AbrirCaja();
            var cajero = CrearCajero();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 1m);
            int pausada = ventas.Activa.Id;
            ventas.NuevaVenta();
            ventas.AgregarPorId(p, 2m);
            int id = ventas.CobrarVenta(cajero.IdUsuario, MedioPago.Tarjeta);
            Assert.Equal(pausada, Assert.Single(ventas.VentasEnCurso).Id);
            Assert.Equal(1m, Assert.Single(ventas.Carrito).Cantidad);
            Assert.Equal(8m, productos.ObtenerPorId(p).Stock);
            var log = Assert.Single(new LogService().Obtener(DateTime.Today, DateTime.Today, modulo: ModuloLog.Ventas),
                l => l.Accion == "Venta");
            Assert.Equal(cajero.IdUsuario, log.IdUsuario);
            Assert.Equal(cajero.Nombre, log.NombreUsuario);
            Assert.Contains("N°" + id, log.Detalle);
        }

        [Fact]
        public void Suscriptor_fallido_no_oculta_confirmacion_ni_impide_otros_avisos()
        {
            AbrirCaja();
            int p = CrearProducto(productos, "Pan", 1000m, 10m);
            ventas.AgregarPorId(p, 2m);
            int avisosVenta = 0, avisosProducto = 0;
            bool carritoCerrado = false;
            Action<string> falla = entidad => { throw new InvalidOperationException("fallo de refresco simulado"); };
            Action<string> observa = entidad => {
                if (entidad == Entidad.Venta) { avisosVenta++; carritoCerrado = ventas.Carrito.Count == 0; }
                if (entidad == Entidad.Producto) avisosProducto++;
            };
            NotificadorCambios.Cambio += falla;
            NotificadorCambios.Cambio += observa;
            try
            {
                int id = ventas.CobrarVenta(1, MedioPago.Efectivo);
                Assert.True(id > 0);
                Assert.True(carritoCerrado);
                Assert.Equal(1, avisosVenta);
                Assert.Equal(1, avisosProducto);
                Assert.Equal(1, Contar("Venta"));
                Assert.Equal(8m, productos.ObtenerPorId(p).Stock);
                Assert.Throws<NegocioException>(() => ventas.CobrarVenta(1, MedioPago.Efectivo));
                Assert.Equal(1, Contar("Venta"));
            }
            finally
            {
                NotificadorCambios.Cambio -= falla;
                NotificadorCambios.Cambio -= observa;
            }
        }
    }
}

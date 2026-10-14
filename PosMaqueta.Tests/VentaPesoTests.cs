using Dominio;
using Dominio.Servicios;
using Entidades;
using Presentacion;
using Xunit;

namespace PosMaqueta.Tests
{
    public class VentaPesoTests : ServiciosTestBase
    {
        [Fact]
        public void Ingresar_gramos_editar_y_cobrar_conserva_peso_y_stock()
        {
            var productos = new ProductoService();
            var ventas = new VentaService();
            AbrirCaja();
            int id = productos.Crear(new Producto { Nombre = "Tomates", Precio = 2990m, Stock = 2.5m,
                UnidadMedida = UnidadMedida.Kilogramo, Categoria = "Bebidas", CodigoBarras = "TOM-01" });
            Assert.True(EntradaCantidad.TryCalcular("350", true, true, 2.5m, 2990m,
                out decimal cantidad, out _, out _));
            ventas.AgregarPorCodigo("TOM-01", cantidad);
            Assert.Equal(1047m, ventas.Total);
            Assert.True(EntradaCantidad.TryCalcular("0,125", true, false, 2.5m, 2990m,
                out cantidad, out _, out _));
            ventas.CambiarCantidad(id, cantidad);
            Assert.Equal(0.125m, Assert.Single(ventas.Carrito).Cantidad);
            Assert.Equal(374m, ventas.Total);

            int idVenta = ventas.CobrarVenta(Sesion.UsuarioActual.IdUsuario, MedioPago.Efectivo);
            var detalle = Assert.Single(ventas.ObtenerDetalleVenta(idVenta));
            Assert.Equal(0.125m, detalle.Cantidad);
            Assert.Equal(374m, detalle.Subtotal);
            Assert.Equal(2.375m, productos.ObtenerPorId(id).Stock);
        }

        [Fact]
        public void Agregar_fracciones_acumula_y_editar_sobre_stock_no_modifica_carrito()
        {
            var productos = new ProductoService();
            var ventas = new VentaService();
            int id = productos.Crear(new Producto { Nombre = "Tomates", Precio = 2990m, Stock = 0.875m,
                UnidadMedida = UnidadMedida.Kilogramo, Categoria = "Bebidas" });
            ventas.AgregarPorId(id, 0.350m);
            ventas.AgregarPorId(id, 0.525m);
            Assert.Equal(0.875m, Assert.Single(ventas.Carrito).Cantidad);
            Assert.Equal(2616m, ventas.Total);
            Assert.Throws<NegocioException>(() => ventas.CambiarCantidad(id, 0.876m));
            Assert.Throws<NegocioException>(() => ventas.AgregarPorId(id, 0.001m));
            Assert.Equal(0.875m, Assert.Single(ventas.Carrito).Cantidad);
        }
    }
}

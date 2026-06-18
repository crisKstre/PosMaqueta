using Dominio;
using Dominio.Servicios;
using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    public class ProductoServiceTests : ServiciosTestBase
    {
        private readonly ProductoService svc = new ProductoService();

        // 3.A — autorización en la capa de servicio: un cajero no puede crear productos aunque
        // invoque el servicio directamente (la UI ya lo oculta; esto es defensa en profundidad).
        [Fact]
        public void Crear_como_cajero_lanza_NegocioException()
        {
            Sesion.UsuarioActual = new UsuarioService().ObtenerTodos().Find(u => u.LoginNombre == "empleado");
            Assert.Throws<NegocioException>(() => CrearProducto(svc, "X", 100m, 1m));
        }

        [Fact]
        public void Crear_persiste_y_devuelve_id()
        {
            int id = CrearProducto(svc, "Agua 500ml", 700m, 50m, "111");
            Assert.True(id > 0);
            var p = svc.ObtenerPorId(id);
            Assert.NotNull(p);
            Assert.Equal("Agua 500ml", p.Nombre);
            Assert.Equal(700m, p.Precio);
            Assert.Equal(50m, p.Stock);
        }

        [Fact]
        public void Crear_con_precio_negativo_lanza_NegocioException()
        {
            var ex = Assert.Throws<NegocioException>(() =>
                svc.Crear(new Producto { Nombre = "X", Precio = -1m, Stock = 1m, UnidadMedida = UnidadMedida.Unidad }));
            Assert.Contains("precio", ex.Message.ToLowerInvariant());
        }

        [Fact]
        public void Crear_con_nombre_vacio_lanza_NegocioException()
            => Assert.Throws<NegocioException>(() =>
                svc.Crear(new Producto { Nombre = "  ", Precio = 1m, Stock = 1m, UnidadMedida = UnidadMedida.Unidad }));

        [Fact]
        public void Crear_con_codigo_duplicado_lanza_NegocioException()
        {
            CrearProducto(svc, "Uno", 100m, 1m, "DUP");
            Assert.Throws<NegocioException>(() => CrearProducto(svc, "Dos", 200m, 1m, "DUP"));
        }

        // Regresión: editar nombre/precio/stock NO debe borrar el descuento (se gestiona aparte).
        [Fact]
        public void Actualizar_no_pisa_el_descuento_del_producto()
        {
            int id = CrearProducto(svc, "Bebida", 1000m, 10m);
            svc.AplicarDescuento(id, 20m);

            var p = svc.ObtenerPorId(id);
            p.Precio = 1200m;
            p.DescuentoPorcentaje = 0m;   // el formulario arma un Producto con descuento 0
            svc.Actualizar(p);

            var recargado = svc.ObtenerPorId(id);
            Assert.Equal(20m, recargado.DescuentoPorcentaje);
            Assert.Equal(1200m, recargado.Precio);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void AplicarDescuento_fuera_de_rango_lanza(decimal pct)
        {
            int id = CrearProducto(svc, "B", 1000m, 5m);
            Assert.Throws<NegocioException>(() => svc.AplicarDescuento(id, pct));
        }

        [Fact]
        public void AplicarDescuento_persiste_y_se_quita_con_cero()
        {
            int id = CrearProducto(svc, "B", 1000m, 5m);
            svc.AplicarDescuento(id, 25m);
            Assert.Equal(25m, svc.ObtenerPorId(id).DescuentoPorcentaje);
            svc.AplicarDescuento(id, 0m);
            Assert.False(svc.ObtenerPorId(id).TieneDescuento);
        }

        [Fact]
        public void AjustarStock_incrementa_y_descuenta()
        {
            int id = CrearProducto(svc, "B", 100m, 10m);
            Assert.Equal(15m, svc.AjustarStock(id, 5m));
            Assert.Equal(12m, svc.AjustarStock(id, -3m));
            Assert.Equal(12m, svc.ObtenerPorId(id).Stock);
        }

        [Fact]
        public void AjustarStock_que_deja_negativo_lanza()
        {
            int id = CrearProducto(svc, "B", 100m, 2m);
            Assert.Throws<NegocioException>(() => svc.AjustarStock(id, -5m));
        }

        [Fact]
        public void Desactivar_lo_saca_de_activos_y_Activar_lo_devuelve()
        {
            int id = CrearProducto(svc, "Oculto", 100m, 5m);
            svc.Desactivar(id);
            Assert.DoesNotContain(svc.ObtenerActivos(), x => x.IdProducto == id);
            svc.Activar(id);
            Assert.Contains(svc.ObtenerActivos(), x => x.IdProducto == id);
        }

        [Fact]
        public void Eliminar_sin_ventas_funciona()
        {
            int id = CrearProducto(svc, "Temp", 100m, 5m);
            svc.Eliminar(id);
            Assert.Null(svc.ObtenerPorId(id));
        }

        [Fact]
        public void ObtenerBajoStock_incluye_los_que_estan_en_o_bajo_el_minimo()
        {
            int bajo = CrearProducto(svc, "Bajo", 100m, 2m, stockMin: 5m);
            int ok = CrearProducto(svc, "OK", 100m, 50m, stockMin: 5m);
            var lista = svc.ObtenerBajoStock();
            Assert.Contains(lista, x => x.IdProducto == bajo);
            Assert.DoesNotContain(lista, x => x.IdProducto == ok);
        }
    }
}

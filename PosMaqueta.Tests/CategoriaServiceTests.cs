using System.Linq;
using Dominio;
using Dominio.Servicios;
using Xunit;

namespace PosMaqueta.Tests
{
    public class CategoriaServiceTests : ServiciosTestBase
    {
        private readonly CategoriaService cats = new CategoriaService();
        private readonly ProductoService productos = new ProductoService();

        [Fact]
        public void Agregar_persiste_una_categoria_nueva()
        {
            cats.Agregar("Congelados");
            Assert.Contains(cats.ObtenerTodas(), c => c.Nombre == "Congelados");
        }

        [Fact]
        public void Agregar_vacio_lanza()
            => Assert.Throws<NegocioException>(() => cats.Agregar("   "));

        [Fact]
        public void Agregar_duplicado_lanza()
            => Assert.Throws<NegocioException>(() => cats.Agregar("Bebidas")); // categoría sembrada

        [Fact]
        public void Eliminar_categoria_con_productos_lanza()
        {
            CrearProducto(productos, "Cola", 1000m, 5m);   // queda en "Bebidas"
            var bebidas = cats.ObtenerTodas().First(c => c.Nombre == "Bebidas");
            Assert.Throws<NegocioException>(() => cats.Eliminar(bebidas.IdCategoria));
        }

        [Fact]
        public void Eliminar_categoria_sin_productos_funciona()
        {
            cats.Agregar("Temporal");
            var temp = cats.ObtenerTodas().First(c => c.Nombre == "Temporal");
            cats.Eliminar(temp.IdCategoria);
            Assert.DoesNotContain(cats.ObtenerTodas(), c => c.Nombre == "Temporal");
        }
    }
}

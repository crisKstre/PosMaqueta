using System.IO;
using System.Text;
using Dominio.Servicios;
using Xunit;

namespace PosMaqueta.Tests
{
    public class ImportacionServiceTests : ServiciosTestBase
    {
        private readonly ImportacionService svc = new ImportacionService();
        private readonly ProductoService productos = new ProductoService();

        private string Csv(string contenido)
        {
            string ruta = Path.Combine(DirTrabajo, "catalogo.csv");
            File.WriteAllText(ruta, contenido, Encoding.UTF8);
            return ruta;
        }

        [Fact]
        public void Importa_crea_productos_nuevos()
        {
            var res = svc.ImportarProductos(Csv(
                "CodigoBarras,Nombre,Categoria,Precio,Stock,StockMinimo,Unidad\n" +
                "111,Agua 500ml,Bebidas,700,50,5,Unidad\n" +
                "222,Pan,Panadería,1500,10,2,Kg\n"));

            Assert.Equal(2, res.Creados);
            Assert.Empty(res.Errores);
            Assert.NotNull(productos.ObtenerPorCodigo("111"));
            Assert.Equal("Pan", productos.ObtenerPorCodigo("222").Nombre);
        }

        [Fact]
        public void Importa_actualiza_producto_existente_por_codigo()
        {
            CrearProducto(productos, "Viejo", 500m, 5m, "AA");
            var res = svc.ImportarProductos(Csv("AA,Nuevo Nombre,Bebidas,999,20,1,Unidad\n"));

            Assert.Equal(1, res.Actualizados);
            Assert.Equal(0, res.Creados);
            var p = productos.ObtenerPorCodigo("AA");
            Assert.Equal("Nuevo Nombre", p.Nombre);
            Assert.Equal(999m, p.Precio);
        }

        [Fact]
        public void Filas_invalidas_se_reportan_sin_frenar_las_validas()
        {
            var res = svc.ImportarProductos(Csv(
                ",Sin Precio,Bebidas,,1,0,Unidad\n" +          // precio vacío -> error
                "333,Bueno,Bebidas,1000,5,0,Unidad\n"));

            Assert.Equal(1, res.Creados);
            Assert.Single(res.Errores);
        }

        [Fact]
        public void Crea_categorias_nuevas_del_archivo()
        {
            var cats = new CategoriaService();
            svc.ImportarProductos(Csv("444,Producto X,CategoriaNueva,300,1,0,Unidad\n"));
            Assert.Contains(cats.ObtenerTodas(), c => c.Nombre == "CategoriaNueva");
        }

        [Fact]
        public void Detecta_separador_punto_y_coma()
        {
            var res = svc.ImportarProductos(Csv("555;Con PuntoYComa;Bebidas;1200;3;0;Unidad\n"));
            Assert.Equal(1, res.Creados);
            Assert.NotNull(productos.ObtenerPorCodigo("555"));
        }

        [Fact]
        public void Importa_sin_codigo_actualiza_por_nombre_sin_duplicar()
        {
            svc.ImportarProductos(Csv(",Manzana Roja,Frutas,1000,10,0,Kg\n"));        // crea (sin código)
            var res = svc.ImportarProductos(Csv(",Manzana Roja,Frutas,1200,5,0,Kg\n")); // mismo nombre, sin código

            Assert.Equal(0, res.Creados);
            Assert.Equal(1, res.Actualizados);            // actualiza, no duplica
            var manzanas = productos.Buscar("Manzana Roja");
            Assert.Single(manzanas);
            Assert.Equal(1200m, manzanas[0].Precio);
        }

        [Fact]
        public void El_encabezado_no_se_importa_como_producto()
        {
            var res = svc.ImportarProductos(Csv(
                "CodigoBarras,Nombre,Categoria,Precio,Stock,StockMinimo,Unidad\n" +
                "666,Unico,Bebidas,800,2,0,Unidad\n"));
            Assert.Equal(1, res.Creados);
        }
    }
}

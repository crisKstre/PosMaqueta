using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AccesoData;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Microsoft.Data.Sqlite;
using Xunit;

namespace PosMaqueta.Tests
{
    internal static class FotoPrueba
    {
        internal static byte[] Crear(Color color, int ancho = 800, int alto = 400, bool ruido = false)
        {
            using (var bitmap = new Bitmap(ancho, alto))
            using (var memoria = new MemoryStream())
            {
                using (var g = Graphics.FromImage(bitmap)) g.Clear(color);
                if (ruido)
                {
                    var random = new Random(42);
                    for (int y = 0; y < alto; y++)
                        for (int x = 0; x < ancho; x++) bitmap.SetPixel(x, y, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                }
                bitmap.Save(memoria, ImageFormat.Png);
                return memoria.ToArray();
            }
        }
    }

    public class ProcesadorFotoProductoTests
    {
        [Fact]
        public void Normaliza_a_jpeg_limita_tamano_y_conserva_proporcion()
        {
            var foto = ProcesadorFotoProducto.Preparar(FotoPrueba.Crear(Color.Transparent));
            using (var memoria = new MemoryStream(foto.Imagen))
            using (var imagen = new Bitmap(memoria))
            {
                Assert.Equal(ImageFormat.Jpeg.Guid, imagen.RawFormat.Guid);
                Assert.Equal(512, imagen.Width);
                Assert.Equal(256, imagen.Height);
                Assert.True(imagen.GetPixel(0, 0).R > 245); // transparencia sobre blanco
            }
            using (var memoria = new MemoryStream(foto.Miniatura))
            using (var imagen = Image.FromStream(memoria))
            {
                Assert.Equal(96, imagen.Width);
                Assert.Equal(48, imagen.Height);
            }
            Assert.InRange(foto.Imagen.Length, 1, ProcesadorFotoProducto.MaxImagen);
            Assert.InRange(foto.Miniatura.Length, 1, ProcesadorFotoProducto.MaxMiniatura);
        }

        [Fact]
        public void Rechaza_archivos_invalidos_vacios_y_excesivos()
        {
            Assert.Throws<NegocioException>(() => ProcesadorFotoProducto.Preparar(null));
            Assert.Throws<NegocioException>(() => ProcesadorFotoProducto.Preparar(new byte[0]));
            Assert.Throws<NegocioException>(() => ProcesadorFotoProducto.Preparar(new byte[] { 1, 2, 3 }));
            Assert.Throws<NegocioException>(() => ProcesadorFotoProducto.Preparar(new byte[ProcesadorFotoProducto.MaxArchivo + 1]));
            Assert.Throws<NegocioException>(() => ProcesadorFotoProducto.Preparar(FotoPrueba.Crear(Color.Red, 10001, 1)));
        }
    }

    public class FotosProductoTests : ServiciosTestBase
    {
        private readonly ProductoService productos = new ProductoService();
        private readonly FotoProductoService fotos = new FotoProductoService();

        private int Crear() => CrearProducto(productos, "Tomates", 2990m, 5.125m);

        [Fact]
        public void Producto_sin_foto_y_guardar_reemplazar_quitar_sin_alterar_producto()
        {
            int id = Crear();
            Assert.Null(fotos.ObtenerImagen(id));
            Assert.Null(fotos.ObtenerMiniatura(id));
            fotos.Guardar(id, FotoPrueba.Crear(Color.Red));
            byte[] original = fotos.ObtenerImagen(id);
            Assert.NotNull(original);
            Assert.NotNull(fotos.ObtenerMiniatura(id));
            fotos.Guardar(id, FotoPrueba.Crear(Color.Blue));
            Assert.NotEqual(original, fotos.ObtenerImagen(id));
            Assert.Equal(5.125m, productos.ObtenerPorId(id).Stock);
            Assert.Equal(2990m, productos.ObtenerPorId(id).Precio);
            fotos.Quitar(id);
            Assert.Null(fotos.ObtenerImagen(id));
            Assert.Null(fotos.ObtenerMiniatura(id));
            Assert.Contains(new LogService().Obtener(DateTime.Today, DateTime.Today), l => l.Accion == "Quitar foto");
        }

        [Fact]
        public void Foto_invalida_conserva_la_anterior()
        {
            int id = Crear();
            fotos.Guardar(id, FotoPrueba.Crear(Color.Red));
            byte[] anterior = fotos.ObtenerImagen(id);
            Assert.Throws<NegocioException>(() => fotos.Guardar(id, new byte[] { 1, 2, 3 }));
            Assert.Equal(anterior, fotos.ObtenerImagen(id));
        }

        [Fact]
        public void Cajero_puede_ver_pero_no_guardar_ni_quitar()
        {
            int id = Crear();
            fotos.Guardar(id, FotoPrueba.Crear(Color.Red));
            Sesion.UsuarioActual = CrearCajero();
            Assert.NotNull(fotos.ObtenerImagen(id));
            Assert.Throws<NegocioException>(() => fotos.Guardar(id, FotoPrueba.Crear(Color.Blue)));
            Assert.Throws<NegocioException>(() => fotos.Quitar(id));
            Sesion.UsuarioActual = null;
            Assert.Throws<NegocioException>(() => fotos.Quitar(id));
        }

        [Fact]
        public void No_admite_foto_de_producto_inexistente()
        {
            Assert.Throws<NegocioException>(() => fotos.Guardar(99999, FotoPrueba.Crear(Color.Red)));
            Assert.Throws<NegocioException>(() => fotos.Quitar(99999));
        }

        [Fact]
        public void Eliminar_producto_elimina_su_foto()
        {
            int id = Crear();
            fotos.Guardar(id, FotoPrueba.Crear(Color.Red));
            productos.Eliminar(id);
            Assert.Null(fotos.ObtenerImagen(id));
        }

        [Fact]
        public void Fallo_de_auditoria_revierte_reemplazo_y_eliminacion()
        {
            int id = Crear();
            fotos.Guardar(id, FotoPrueba.Crear(Color.Red));
            byte[] anterior = fotos.ObtenerImagen(id);
            Sql("CREATE TRIGGER fallo_foto BEFORE INSERT ON LogMovimiento BEGIN SELECT RAISE(ABORT, 'fallo de prueba'); END;");
            Assert.ThrowsAny<Exception>(() => fotos.Guardar(id, FotoPrueba.Crear(Color.Blue)));
            Assert.Equal(anterior, fotos.ObtenerImagen(id));
            Assert.ThrowsAny<Exception>(() => fotos.Quitar(id));
            Assert.Equal(anterior, fotos.ObtenerImagen(id));
        }

        [Fact]
        public void Migra_v4_sin_perder_productos_y_es_repetible()
        {
            int id = Crear();
            Sql("DROP TABLE FotoProducto; UPDATE SchemaVersion SET Version = 4;");
            string previo = RespaldoBD.CrearRespaldo();
            Assert.True(File.Exists(previo));
            new DatabaseInitializer().Inicializar();
            new DatabaseInitializer().Inicializar();
            Assert.Equal(5.125m, productos.ObtenerPorId(id).Stock);
            fotos.Guardar(id, FotoPrueba.Crear(Color.Red));
            new DatabaseInitializer().Inicializar();
            Assert.NotNull(fotos.ObtenerImagen(id));
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT Version FROM SchemaVersion;";
                    Assert.Equal((long)DatabaseInitializer.ESQUEMA_VERSION, (long)cmd.ExecuteScalar());
                }
            }
        }

        [Fact]
        public void Respaldo_restaura_foto_y_miniatura_en_otra_carpeta()
        {
            int id = Crear();
            fotos.Guardar(id, FotoPrueba.Crear(Color.Red));
            byte[] imagen = fotos.ObtenerImagen(id), miniatura = fotos.ObtenerMiniatura(id);
            string respaldo = RespaldoBD.CrearRespaldo();
            fotos.Quitar(id);
            string nueva = Path.Combine(DirTrabajo, "restaurada");
            Directory.CreateDirectory(nueva);
            ConfigBD.CadenaConexion = "Data Source=" + Path.Combine(nueva, "pos.db") + ";Pooling=False;Foreign Keys=True";
            RespaldoBD.Restaurar(respaldo);
            new DatabaseInitializer().Inicializar();
            Assert.Equal(imagen, fotos.ObtenerImagen(id));
            Assert.Equal(miniatura, fotos.ObtenerMiniatura(id));
        }

        private static void Sql(string sql)
        {
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = con.CreateCommand()) { cmd.CommandText = sql; cmd.ExecuteNonQuery(); }
            }
        }
    }
}

using System;
using System.IO;
using AccesoData;
using Dominio.Servicios;
using Entidades;

namespace PosMaqueta.Tests
{
    /// <summary>
    /// Base para tests de integración de servicios: cada test corre contra una BD SQLite
    /// temporal y aislada (esquema + seed reales vía DatabaseInitializer) y reinicia el estado
    /// estático de ventas en curso. La suite corre en serie (ver TestConfig) para evitar carreras
    /// sobre ConfigBD.CadenaConexion (global).
    /// </summary>
    public abstract class ServiciosTestBase : IDisposable
    {
        private readonly string rutaDb;

        protected ServiciosTestBase()
        {
            rutaDb = Path.Combine(Path.GetTempPath(), "postest_" + Guid.NewGuid().ToString("N") + ".db");
            ConfigBD.CadenaConexion = "Data Source=" + rutaDb + ";Pooling=False;Foreign Keys=True";
            VentaService.ReiniciarVentasEnCurso();   // limpia el estado estático compartido
            new DatabaseInitializer().Inicializar();  // esquema + admin/empleado + categorías
        }

        public void Dispose()
        {
            VentaService.ReiniciarVentasEnCurso();
            ConfigBD.CadenaConexion = null;           // restaura la ubicación por defecto
            // Borra la BD y sus sidecars de WAL (-wal/-shm)
            foreach (var f in new[] { rutaDb, rutaDb + "-wal", rutaDb + "-shm" })
                try { if (File.Exists(f)) File.Delete(f); } catch { /* archivos temporales */ }
        }

        // Crea un producto activo y devuelve su id. Categoría "Bebidas" (sembrada por defecto).
        protected static int CrearProducto(ProductoService svc, string nombre, decimal precio, decimal stock,
            string codigo = null, decimal stockMin = 0)
        {
            return svc.Crear(new Producto
            {
                Nombre = nombre,
                Precio = precio,
                Stock = stock,
                StockMinimo = stockMin,
                UnidadMedida = UnidadMedida.Unidad,
                Categoria = "Bebidas",
                CodigoBarras = codigo,
            });
        }
    }
}

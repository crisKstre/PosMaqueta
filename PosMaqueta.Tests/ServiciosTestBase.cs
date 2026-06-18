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
        // Carpeta de trabajo propia del test (contiene la BD, sus Backups, etc.). Aislada por test.
        protected readonly string DirTrabajo;

        protected ServiciosTestBase()
        {
            DirTrabajo = Path.Combine(Path.GetTempPath(), "postest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirTrabajo);
            rutaDb = Path.Combine(DirTrabajo, "pos.db");
            ConfigBD.Proveedor = ProveedorBD.Sqlite;   // defensivo: por si un test de SQL Server corrió antes
            ConfigBD.CadenaConexion = "Data Source=" + rutaDb + ";Pooling=False;Foreign Keys=True";
            ConfigBD.CarpetaRespaldoExterno = null;   // evita que la config de otro test se filtre
            VentaService.ReiniciarVentasEnCurso();   // limpia el estado estático compartido
            Sesion.UsuarioActual = null;              // evita que una sesión de otro test se filtre
            new DatabaseInitializer().Inicializar();  // esquema + admin/empleado + categorías
            // Las operaciones sensibles exigen rol admin (defensa en profundidad): los tests corren
            // como el admin sembrado, que además es un IdUsuario real (FK válida para los logs).
            Sesion.UsuarioActual = new UsuarioService().ObtenerTodos().Find(u => u.LoginNombre == "admin");
        }

        public void Dispose()
        {
            VentaService.ReiniciarVentasEnCurso();
            ConfigBD.CadenaConexion = null;           // restaura la ubicación por defecto
            ConfigBD.CarpetaRespaldoExterno = null;
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            // Borra toda la carpeta de trabajo (BD, sidecars WAL, Backups, copias .previo)
            try { if (Directory.Exists(DirTrabajo)) Directory.Delete(DirTrabajo, recursive: true); }
            catch { /* archivos temporales */ }
        }

        // Abre una caja para los tests que necesitan cobrar (0.A exige caja abierta).
        protected int AbrirCaja(decimal montoInicial = 0)
        {
            return new CajaService().AbrirCaja(Sesion.UsuarioActual.IdUsuario, montoInicial);
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

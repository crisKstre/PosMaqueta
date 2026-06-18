using System;
using System.Data.SqlClient;
using AccesoData;
using AccesoData.DAO;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    /// <summary>
    /// Pruebas de humo del proveedor SQL Server contra LocalDB: verifican que el dialecto T-SQL
    /// (DDL, IDENTITY/SCOPE_IDENTITY, TOP, IN, transacciones) realmente funciona, no solo compila.
    /// Se OMITEN automáticamente si LocalDB no está disponible (otras máquinas / CI sin SQL Server).
    /// </summary>
    public class SqlServerSmokeTests : IDisposable
    {
        private const string ConnTest =
            "Server=(localdb)\\MSSQLLocalDB;Database=PosMaqueta_Test;Integrated Security=true;TrustServerCertificate=true;";
        private const string ConnMaster =
            "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";

        private readonly bool disponible;
        private readonly ProductoService productos = new ProductoService();
        private readonly VentaService    ventas    = new VentaService();
        private readonly CajaService     caja      = new CajaService();
        private readonly CategoriaService cats     = new CategoriaService();

        public SqlServerSmokeTests()
        {
            disponible = LocalDbDisponible();
            if (!disponible) return;

            ConfigBD.Proveedor = ProveedorBD.SqlServer;
            ConfigBD.CadenaConexion = ConnTest;
            VentaService.ReiniciarVentasEnCurso();

            // BD limpia por test: asegurar que exista, soltar las tablas y recrear esquema + seed.
            EjecutarMaster("IF DB_ID('PosMaqueta_Test') IS NULL CREATE DATABASE [PosMaqueta_Test];");
            using (var con = new SqlConnection(ConnTest))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
                        DROP TABLE IF EXISTS DevolucionItem;
                        DROP TABLE IF EXISTS Devolucion;
                        DROP TABLE IF EXISTS PagoVenta;
                        DROP TABLE IF EXISTS DetalleVenta;
                        DROP TABLE IF EXISTS Venta;
                        DROP TABLE IF EXISTS Caja;
                        DROP TABLE IF EXISTS LogMovimiento;
                        DROP TABLE IF EXISTS Producto;
                        DROP TABLE IF EXISTS Categoria;
                        DROP TABLE IF EXISTS Usuario;
                        DROP TABLE IF EXISTS SchemaVersion;";
                    cmd.ExecuteNonQuery();
                }
            }
            new DatabaseInitializer().Inicializar();
            // Las operaciones sensibles exigen rol admin (3.A); corremos como el admin sembrado.
            Sesion.UsuarioActual = new UsuarioService().ObtenerTodos().Find(u => u.LoginNombre == "admin");
        }

        public void Dispose()
        {
            VentaService.ReiniciarVentasEnCurso();
            Sesion.UsuarioActual = null;
            ConfigBD.Proveedor = ProveedorBD.Sqlite;
            ConfigBD.CadenaConexion = null;
        }

        private static bool LocalDbDisponible()
        {
            try { using (var c = new SqlConnection(ConnMaster)) { c.Open(); } return true; }
            catch { return false; }
        }

        private static void EjecutarMaster(string sql)
        {
            using (var con = new SqlConnection(ConnMaster))
            {
                con.Open();
                using (var cmd = con.CreateCommand()) { cmd.CommandText = sql; cmd.ExecuteNonQuery(); }
            }
        }

        private int CrearProducto(string nombre, decimal precio, decimal stock, string codigo = null, decimal stockMin = 0)
            => productos.Crear(new Producto
            {
                Nombre = nombre, Precio = precio, Stock = stock, StockMinimo = stockMin,
                UnidadMedida = UnidadMedida.Unidad, Categoria = "Bebidas", CodigoBarras = codigo
            });

        [SkippableFact]
        public void Esquema_y_seed_permiten_login_admin()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            var u = new UsuarioDao().Login("admin", "admin123");
            Assert.NotNull(u);
            Assert.Equal(RolUsuario.Admin, u.Rol);
        }

        [SkippableFact]
        public void Producto_crear_obtener_y_buscar()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            int id = CrearProducto("Cola 1.5L", 1990m, 20m, "7800001");
            var p = productos.ObtenerPorId(id);
            Assert.NotNull(p);
            Assert.Equal(1990m, p.Precio);
            Assert.Equal(20m, p.Stock);
            Assert.Contains(productos.Buscar("Cola"), x => x.IdProducto == id);
        }

        [SkippableFact]
        public void AplicarDescuento_persiste()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            int id = CrearProducto("Galleta", 1000m, 10m);
            productos.AplicarDescuento(id, 20m);
            Assert.Equal(20m, productos.ObtenerPorId(id).DescuentoPorcentaje);
        }

        [SkippableFact]
        public void Cobrar_registra_descuenta_stock_y_guarda_detalle()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            caja.AbrirCaja(1, 0m);                                     // 0.A: caja abierta para vender
            int id = CrearProducto("Cola", 1190m, 10m);
            ventas.AgregarPorId(id, 2m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);

            Assert.True(idVenta > 0);                                  // SCOPE_IDENTITY
            Assert.Equal(8m, productos.ObtenerPorId(id).Stock);       // descuento atómico
            var v = Assert.Single(ventas.ObtenerVentas(DateTime.Today, DateTime.Today), x => x.IdVenta == idVenta);
            Assert.Equal(2380m, v.Total);
            Assert.Single(ventas.ObtenerDetalleVenta(idVenta));
        }

        [SkippableFact]
        public void AnularVenta_devuelve_stock_y_es_idempotente()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            caja.AbrirCaja(1, 0m);
            int id = CrearProducto("Cola", 1000m, 10m);
            ventas.AgregarPorId(id, 3m);
            int idVenta = ventas.CobrarVenta(1, MedioPago.Efectivo);
            ventas.AnularVenta(idVenta);
            Assert.Equal(10m, productos.ObtenerPorId(id).Stock);
            Assert.Throws<NegocioException>(() => ventas.AnularVenta(idVenta));
            Assert.Equal(10m, productos.ObtenerPorId(id).Stock);
        }

        [SkippableFact]
        public void Caja_apertura_y_cierre_con_arqueo()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            caja.AbrirCaja(1, 50000m);                                 // IDENTITY
            Assert.True(caja.HayCajaAbierta());                        // TOP (1)
            int id = CrearProducto("Cola", 1190m, 10m);
            ventas.AgregarPorId(id, 1m);
            ventas.CobrarVenta(1, MedioPago.Efectivo);
            Assert.Equal(0m, caja.CerrarCaja(51190m));                 // 50000 + 1190
            Assert.False(caja.HayCajaAbierta());
        }

        [SkippableFact]
        public void Reportes_resumen_y_top_productos()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            caja.AbrirCaja(1, 0m);
            int a = CrearProducto("A", 1000m, 50m);
            int b = CrearProducto("B", 2000m, 50m);
            ventas.AgregarPorId(a, 3m); ventas.CobrarVenta(1, MedioPago.Efectivo);
            ventas.AgregarPorId(b, 1m); ventas.CobrarVenta(1, MedioPago.Tarjeta);

            var resumen = ventas.ObtenerResumenVentas(DateTime.Today, DateTime.Today);
            Assert.Equal(2, resumen.CantidadVentas);
            Assert.Equal(5000m, resumen.TotalVendido);

            var top = ventas.ObtenerTopProductos(DateTime.Today, DateTime.Today, 1);  // TOP (@top)
            Assert.Single(top);
            Assert.Equal("A", top[0].Nombre);    // 3 unidades > 1
        }

        [SkippableFact]
        public void Categoria_agregar_y_no_eliminar_si_tiene_productos()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            cats.Agregar("Congelados");
            Assert.Contains(cats.ObtenerTodas(), c => c.Nombre == "Congelados");

            CrearProducto("Cola", 1000m, 5m);   // queda en "Bebidas"
            var bebidas = cats.ObtenerTodas().Find(c => c.Nombre == "Bebidas");
            Assert.NotNull(bebidas);
            Assert.Throws<NegocioException>(() => cats.Eliminar(bebidas.IdCategoria));
        }

        [SkippableFact]
        public void ObtenerBajoStock_filtra_en_sql()
        {
            Skip.IfNot(disponible, "LocalDB no disponible");
            int bajo = CrearProducto("Bajo", 100m, 2m, stockMin: 5m);
            int ok   = CrearProducto("OK", 100m, 50m, stockMin: 5m);
            var lista = productos.ObtenerBajoStock();
            Assert.Contains(lista, x => x.IdProducto == bajo);
            Assert.DoesNotContain(lista, x => x.IdProducto == ok);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using AccesoData;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Microsoft.Data.Sqlite;
using Xunit;

namespace PosMaqueta.Tests
{
    public class MovimientosCajaTests : ServiciosTestBase
    {
        private readonly MovimientoCajaService movimientos = new MovimientoCajaService();
        private readonly CajaService caja = new CajaService();
        private MovimientoCaja Gasto(decimal monto = 5000m) => new MovimientoCaja
        {
            Tipo = TipoMovimientoCaja.Gasto, Categoria = "Insumos", Descripcion = "Bolsas",
            Monto = monto, MedioPago = MedioPago.Efectivo, Origen = OrigenMovimientoCaja.Cajon
        };
        private MovimientoCaja[] Lista() => movimientos.Obtener(DateTime.Today, DateTime.Today).ToArray();
        private decimal Esperado() { var c = caja.ObtenerCajaAbierta(); return caja.CalcularEfectivoEsperado(c, caja.ObtenerResumen(c.IdCaja)); }

        [Fact]
        public void Gasto_del_cajon_descuenta_una_sola_vez_y_transferencia_no_descuenta()
        {
            AbrirCaja(20000m);
            var productos = new ProductoService(); var ventas = new VentaService();
            int producto = CrearProducto(productos, "Venta prueba", 50000m, 2m);
            ventas.AgregarPorId(producto, 1m); ventas.CobrarVenta(1, MedioPago.Efectivo);
            movimientos.Registrar(Gasto());
            var externo = Gasto(15000m); externo.MedioPago = MedioPago.Transferencia; externo.Origen = OrigenMovimientoCaja.Externo;
            movimientos.Registrar(externo);
            Assert.Equal(65000m, Esperado());
            var r = caja.ObtenerResumen(caja.ObtenerCajaAbierta().IdCaja);
            Assert.Equal(20000m, r.TotalGastos); Assert.Equal(5000m, r.OtrasSalidas);
            Assert.Equal(0m, caja.CerrarCaja(65000m));
        }

        [Fact]
        public void Retiros_y_reposiciones_no_son_gastos_ni_ventas()
        {
            AbrirCaja(20000m);
            var m = Gasto(7000m); m.Tipo = TipoMovimientoCaja.Retiro; movimientos.Registrar(m);
            m.Tipo = TipoMovimientoCaja.Reposicion; m.Monto = 3000m; movimientos.Registrar(m);
            Assert.Equal(16000m, Esperado());
            var r = caja.ObtenerResumen(caja.ObtenerCajaAbierta().IdCaja);
            Assert.Equal(0m, r.TotalGastos); Assert.Equal(0m, r.TotalVendido);
            Assert.Equal(3000m, r.OtrasEntradas); Assert.Equal(7000m, r.OtrasSalidas);
        }

        [Theory]
        [InlineData("Gasto", 20000)] [InlineData("Retiro", 20000)] [InlineData("Reposición", 20000)]
        public void Anular_restaura_efecto_conserva_original_y_no_se_repite(string tipo, int fondo)
        {
            AbrirCaja(fondo); var m = Gasto(); m.Tipo = tipo;
            int id = movimientos.Registrar(m);
            movimientos.Anular(id, "Monto incorrecto");
            Assert.Equal(fondo, Esperado());
            var guardado = Assert.Single(Lista());
            Assert.Equal(5000m, guardado.Monto); Assert.True(guardado.Anulado);
            Assert.Equal("Monto incorrecto", guardado.MotivoAnulacion);
            Assert.Equal(Sesion.UsuarioActual.IdUsuario, guardado.IdUsuarioAnulacion);
            Assert.NotNull(guardado.FechaAnulacion);
            Assert.Throws<NegocioException>(() => movimientos.Anular(id, "Otra vez"));
            Assert.Contains(new LogService().Obtener(DateTime.Today, DateTime.Today), l => l.Accion == "Anular movimiento");
        }

        [Fact]
        public void Turno_cerrado_no_admite_anulaciones_ni_cambia_arqueo_al_abrir_otro()
        {
            int turno = AbrirCaja(20000m); int id = movimientos.Registrar(Gasto());
            var externo = Gasto(); externo.Origen = OrigenMovimientoCaja.Externo;
            int idExterno = movimientos.Registrar(externo);
            caja.CerrarCaja(15000m); AbrirCaja(9000m);
            Assert.Throws<NegocioException>(() => movimientos.Anular(id, "Corrección"));
            Assert.Throws<NegocioException>(() => movimientos.Anular(idExterno, "Corrección"));
            Assert.Equal(9000m, Esperado());
            Assert.Equal(15000m, caja.ObtenerHistorial(DateTime.Today, DateTime.Today).Single(c => c.IdCaja == turno).MontoEsperado);
        }

        [Fact]
        public void Sin_turno_solo_se_permiten_gastos_ajenos_al_cajon()
        {
            Assert.Throws<NegocioException>(() => movimientos.Registrar(Gasto()));
            var m = Gasto(); m.Origen = OrigenMovimientoCaja.Externo;
            movimientos.Registrar(m);
            Assert.Null(Assert.Single(Lista()).IdCaja);
            Assert.Equal(0m, Lista()[0].EfectoCaja);
        }

        [Fact]
        public void Cajero_consulta_pero_no_registra_ni_anula_y_sin_sesion_no_consulta()
        {
            AbrirCaja(20000m); int id = movimientos.Registrar(Gasto());
            Sesion.UsuarioActual = CrearCajero();
            Assert.Single(Lista());
            Assert.Throws<NegocioException>(() => movimientos.Registrar(Gasto()));
            Assert.Throws<NegocioException>(() => movimientos.Anular(id, "Motivo"));
            Sesion.UsuarioActual = null;
            Assert.Throws<NegocioException>(() => Lista());
        }

        [Theory]
        [InlineData(0)] [InlineData(-1)] [InlineData(1.5)] [InlineData(1000000000000)]
        public void Rechaza_montos_invalidos_sin_guardar(decimal monto)
        {
            AbrirCaja(); Assert.Throws<NegocioException>(() => movimientos.Registrar(Gasto(monto))); Assert.Empty(Lista());
        }

        [Fact]
        public void Valida_conceptos_medios_origen_descripcion_categoria_y_motivo()
        {
            AbrirCaja(); var m = Gasto(); m.MedioPago = MedioPago.Transferencia;
            Assert.Throws<NegocioException>(() => movimientos.Registrar(m));
            m.MedioPago = "Cheque"; Assert.Throws<NegocioException>(() => movimientos.Registrar(m));
            m = Gasto(); m.Tipo = "Venta"; Assert.Throws<NegocioException>(() => movimientos.Registrar(m));
            m = Gasto(); m.Origen = "Otro"; Assert.Throws<NegocioException>(() => movimientos.Registrar(m));
            m = Gasto(); m.Tipo = TipoMovimientoCaja.Retiro; m.Origen = OrigenMovimientoCaja.Externo;
            Assert.Throws<NegocioException>(() => movimientos.Registrar(m));
            m = Gasto(); m.Descripcion = " "; Assert.Throws<NegocioException>(() => movimientos.Registrar(m));
            m = Gasto(); m.Categoria = " "; Assert.Throws<NegocioException>(() => movimientos.Registrar(m));
            m = Gasto(); m.Referencia = new string('x', 121); Assert.Throws<NegocioException>(() => movimientos.Registrar(m));
            int id = movimientos.Registrar(Gasto());
            Assert.Throws<NegocioException>(() => movimientos.Anular(id, " "));
            Assert.Throws<NegocioException>(() => movimientos.Anular(999, "Motivo"));
        }

        [Fact]
        public void Servicio_asigna_fecha_usuario_turno_y_ignora_estado_del_formulario()
        {
            int turno = AbrirCaja(); var m = Gasto();
            m.Fecha = new DateTime(2000,1,1); m.IdUsuario = 999; m.IdCaja = 999; m.Anulado = true;
            movimientos.Registrar(m); var guardado = Assert.Single(Lista());
            Assert.Equal(DateTime.Today, guardado.Fecha.Date); Assert.Equal(turno, guardado.IdCaja);
            Assert.Equal(1, guardado.IdUsuario); Assert.False(guardado.Anulado);
            Assert.Empty(movimientos.Obtener(DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1)));
        }

        [Fact]
        public void Fallo_de_auditoria_revierte_registro_y_anulacion()
        {
            AbrirCaja(20000m); int id = movimientos.Registrar(Gasto());
            Sql("CREATE TRIGGER fallo_mov BEFORE INSERT ON LogMovimiento BEGIN SELECT RAISE(ABORT, 'fallo de prueba'); END;");
            Assert.ThrowsAny<Exception>(() => movimientos.Registrar(Gasto()));
            Assert.ThrowsAny<Exception>(() => movimientos.Anular(id, "Prueba"));
            Assert.Single(Lista()); Assert.False(Lista()[0].Anulado); Assert.Equal(15000m, Esperado());
        }

        [Fact]
        public void Migra_v5_conserva_datos_es_idempotente_y_respaldo_restaura_movimientos()
        {
            int producto = CrearProducto(new ProductoService(), "Tomate", 2500m, 18.4m);
            int turno = AbrirCaja(20000m); caja.CerrarCaja(20000m);
            Sql("DROP TABLE MovimientoCaja; UPDATE SchemaVersion SET Version=5;");
            string previo = RespaldoBD.CrearRespaldo(); Assert.True(File.Exists(previo));
            new DatabaseInitializer().Inicializar(); new DatabaseInitializer().Inicializar();
            Assert.Equal(18.4m, new ProductoService().ObtenerPorId(producto).Stock);
            Assert.Equal(20000m, caja.ObtenerHistorial(DateTime.Today, DateTime.Today).Single(c => c.IdCaja==turno).MontoEsperado);
            AbrirCaja(10000m); int id = movimientos.Registrar(Gasto());
            string backup = RespaldoBD.CrearRespaldo();
            movimientos.Anular(id, "Prueba posterior al respaldo");
            // Abrir una copia autónoma confirma que el respaldo contiene tabla y arqueo coherentes.
            string restaurada = Path.Combine(DirTrabajo, "restaurada.db"); File.Copy(backup, restaurada);
            ConfigBD.CadenaConexion = "Data Source=" + restaurada + ";Pooling=False;Foreign Keys=True";
            new DatabaseInitializer().Inicializar();
            Assert.False(Assert.Single(Lista()).Anulado); Assert.Equal(5000m, Esperado());
        }

        private void Sql(string sql)
        {
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            { con.Open(); using (var cmd=con.CreateCommand()) { cmd.CommandText=sql; cmd.ExecuteNonQuery(); } }
        }
    }
}

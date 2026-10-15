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
    public class ProveedoresTests : ServiciosTestBase
    {
        private readonly ProveedorService servicio = new ProveedorService();
        [Fact]
        public void Alta_edicion_conservan_campos_id_estado_y_auditoria()
        {
            int id = servicio.Crear(new Proveedor { Nombre = "  Frutas del sur  ", Rut = "12.345.678-5", Contacto = "Ana", Telefono = "+56 9 1234 5678", Correo = "ana@example.com", Direccion = "Feria local", Notas = "Entrega los lunes" });
            var p = servicio.ObtenerPorId(id);
            Assert.Equal("Frutas del sur", p.Nombre); Assert.Equal("12345678-5", p.Rut); Assert.True(p.Activo);
            var creado = p.FechaCreacion; p.Notas = "Ahora entrega martes"; p.Activo = false; servicio.Actualizar(p);
            var editado = servicio.ObtenerPorId(id);
            Assert.Equal(creado, editado.FechaCreacion); Assert.True(editado.Activo); Assert.Equal("Ahora entrega martes", editado.Notas);
            Assert.Equal("Ana", editado.Contacto); Assert.Equal("ana@example.com", editado.Correo); Assert.Equal("Feria local", editado.Direccion);
            var logs = new LogService().Obtener(DateTime.Today, DateTime.Today).Where(l => l.Modulo == ModuloLog.Proveedores).ToArray();
            Assert.Equal(2, logs.Length); Assert.All(logs, l => Assert.Contains("Proveedor N°" + id, l.Detalle));
        }
        [Fact]
        public void Rut_opcional_y_unico_incluso_desactivado()
        {
            servicio.Crear(new Proveedor { Nombre = "A" }); servicio.Crear(new Proveedor { Nombre = "B", Rut = " " });
            int id = servicio.Crear(new Proveedor { Nombre = "C", Rut = "123456785" }); servicio.CambiarEstado(id, false);
            Assert.Throws<NegocioException>(() => servicio.Crear(new Proveedor { Nombre = "D", Rut = "12.345.678-5" }));
            Assert.Equal(3, servicio.Obtener("", null).Count);
        }
        [Theory]
        [InlineData("12345678-0")][InlineData("ABC")][InlineData("0-0")][InlineData("1234567890-1")]
        public void Rechaza_rut_invalido(string rut)
        { Assert.Throws<NegocioException>(() => servicio.Crear(new Proveedor { Nombre = "Proveedor", Rut = rut })); Assert.Empty(servicio.Obtener()); }
        [Fact]
        public void Valida_nombre_longitudes_y_correo()
        {
            Assert.Throws<NegocioException>(() => servicio.Crear(new Proveedor { Nombre = " " }));
            Assert.Throws<NegocioException>(() => servicio.Crear(new Proveedor { Nombre = new string('A',121) }));
            Assert.Throws<NegocioException>(() => servicio.Crear(new Proveedor { Nombre = "A", Notas = new string('A',1001) }));
            Assert.Throws<NegocioException>(() => servicio.Crear(new Proveedor { Nombre = "A", Correo = "correo-invalido" }));
            Assert.Empty(servicio.Obtener());
        }
        [Fact]
        public void Busqueda_literal_por_nombre_rut_contacto_telefono_y_correo()
        {
            int id = servicio.Crear(new Proveedor { Nombre = "Bodega 100%_", Rut = "12345678-5", Contacto = "Marta", Telefono = "999111222", Correo = "marta@example.com" });
            servicio.Crear(new Proveedor { Nombre = "Otra bodega" });
            foreach(var texto in new[]{"100%_", "12.345.678-5", "123456785", "MARTA", "999111", "example.com"})
                Assert.Equal(id, Assert.Single(servicio.Obtener(texto)).IdProveedor);
            Assert.Empty(servicio.Obtener("' OR 1=1 --"));
        }
        [Fact]
        public void Desactivar_y_reactivar_conservan_datos_y_id()
        {
            int id=servicio.Crear(new Proveedor { Nombre="Proveedor", Notas="Conservar" });
            servicio.CambiarEstado(id,false); Assert.Empty(servicio.Obtener()); Assert.Equal(id,Assert.Single(servicio.Obtener("",false)).IdProveedor);
            servicio.CambiarEstado(id,true); Assert.Equal("Conservar",servicio.ObtenerPorId(id).Notas);
            Assert.Throws<NegocioException>(()=>servicio.CambiarEstado(id,true));
            Assert.Throws<NegocioException>(()=>servicio.Actualizar(new Proveedor { IdProveedor=9999, Nombre="Inexistente" }));
        }
        [Fact]
        public void Cajero_consulta_y_no_modifica_sin_sesion_no_consulta()
        {
            int id=servicio.Crear(new Proveedor { Nombre="Proveedor" }); Sesion.UsuarioActual=CrearCajero();
            Assert.Single(servicio.Obtener()); Assert.NotNull(servicio.ObtenerPorId(id));
            Assert.Throws<NegocioException>(()=>servicio.Crear(new Proveedor { Nombre="Nuevo" }));
            Assert.Throws<NegocioException>(()=>servicio.Actualizar(new Proveedor { IdProveedor=id,Nombre="Cambio" }));
            Assert.Throws<NegocioException>(()=>servicio.CambiarEstado(id,false));
            Sesion.UsuarioActual=null; Assert.Throws<NegocioException>(()=>servicio.Obtener()); Assert.Throws<NegocioException>(()=>servicio.ObtenerPorId(id));
        }
        [Fact]
        public void Fallo_auditoria_revierte_alta_edicion_y_estado()
        {
            int id=servicio.Crear(new Proveedor { Nombre="Original" });
            Sql("CREATE TRIGGER fallar_proveedor BEFORE INSERT ON LogMovimiento WHEN NEW.Modulo='Proveedores' BEGIN SELECT RAISE(ABORT, 'Fallo de prueba'); END;");
            Assert.ThrowsAny<Exception>(()=>servicio.Crear(new Proveedor { Nombre="Nuevo" }));
            Assert.ThrowsAny<Exception>(()=>servicio.Actualizar(new Proveedor { IdProveedor=id,Nombre="Modificado" }));
            Assert.ThrowsAny<Exception>(()=>servicio.CambiarEstado(id,false));
            Assert.Equal("Original", Assert.Single(servicio.Obtener()).Nombre);
        }
        [Fact]
        public void Migracion_v6_es_repetible_y_respaldo_restaura_proveedores()
        {
            var productos=new ProductoService(); int producto=CrearProducto(productos,"Tomate",2500m,5.125m); AbrirCaja(20000m);
            Sql("DROP TABLE Proveedor; UPDATE SchemaVersion SET Version=6;"); string previo=RespaldoBD.CrearRespaldo(); Assert.True(File.Exists(previo));
            new DatabaseInitializer().Inicializar(); new DatabaseInitializer().Inicializar();
            Assert.Equal(5.125m,productos.ObtenerPorId(producto).Stock); Assert.Equal(20000m,new CajaService().ObtenerCajaAbierta().MontoInicial);
            int id=servicio.Crear(new Proveedor { Nombre="Respaldado",Rut="11111111-1" }); servicio.CambiarEstado(id,false);
            string respaldo=RespaldoBD.CrearRespaldo();
            string nueva=Path.Combine(DirTrabajo,"restaurada"); Directory.CreateDirectory(nueva);
            ConfigBD.CadenaConexion="Data Source="+Path.Combine(nueva,"pos.db")+";Pooling=False;Foreign Keys=True";
            RespaldoBD.Restaurar(respaldo); new DatabaseInitializer().Inicializar();
            Assert.Equal("Respaldado",servicio.ObtenerPorId(id).Nombre); Assert.False(servicio.ObtenerPorId(id).Activo);
        }
        private static void Sql(string sql)
        { using(var c=new SqliteConnection(ConfigBD.CadenaConexion)) { c.Open(); using(var cmd=c.CreateCommand()) { cmd.CommandText=sql; cmd.ExecuteNonQuery(); } } }
    }
}

using System.IO;
using AccesoData;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    public class RespaldoServiceTests : ServiciosTestBase
    {
        private readonly RespaldoService svc = new RespaldoService();
        private readonly UsuarioService usuarios = new UsuarioService();

        [Fact]
        public void RespaldarAhora_crea_archivo_y_aparece_en_la_lista()
        {
            string ruta = svc.RespaldarAhora();
            Assert.True(File.Exists(ruta));
            Assert.Contains(svc.Obtener(), r => r.Nombre == Path.GetFileName(ruta));
        }

        [Fact]
        public void Obtener_sin_respaldos_devuelve_lista_vacia()
        {
            Assert.Empty(svc.Obtener());
        }

        [Fact]
        public void Restaurar_revierte_los_cambios_posteriores()
        {
            string backup = svc.RespaldarAhora();   // estado: solo el admin sembrado
            usuarios.Crear(new Usuario { Nombre = "Extra", LoginNombre = "extra", Rol = RolUsuario.Cajero }, "extra1234");
            Assert.Contains(usuarios.ObtenerTodos(), u => u.LoginNombre == "extra");

            svc.Restaurar(backup);

            Assert.DoesNotContain(usuarios.ObtenerTodos(), u => u.LoginNombre == "extra");
            Assert.Contains(usuarios.ObtenerTodos(), u => u.LoginNombre == "admin");
        }

        [Fact]
        public void Respaldo_se_copia_a_la_carpeta_externa()
        {
            string externa = Path.Combine(DirTrabajo, "Externa");
            ConfigBD.CarpetaRespaldoExterno = externa;

            svc.RespaldarAhora();

            Assert.True(Directory.Exists(externa));
            Assert.NotEmpty(Directory.GetFiles(externa, "pos_*.db"));
            Assert.Contains(svc.Obtener(), r => r.Externo);
        }

        [Fact]
        public void Restaurar_archivo_inexistente_lanza_NegocioException()
        {
            Assert.Throws<NegocioException>(() => svc.Restaurar(Path.Combine(DirTrabajo, "no-existe.db")));
        }

        // 3.A / C13 — un cajero no puede respaldar ni restaurar aunque invoque el servicio directo.
        [Fact]
        public void RespaldarAhora_como_cajero_lanza()
        {
            Sesion.UsuarioActual = CrearCajero();
            Assert.Throws<NegocioException>(() => svc.RespaldarAhora());
        }
    }
}

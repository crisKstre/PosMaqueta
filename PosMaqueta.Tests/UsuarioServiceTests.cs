using System.Linq;
using Dominio;
using Dominio.Servicios;
using Entidades;
using Xunit;

namespace PosMaqueta.Tests
{
    public class UsuarioServiceTests : ServiciosTestBase
    {
        private readonly UsuarioService svc = new UsuarioService();

        // Crea un usuario activo (con contraseña que cumple el largo mínimo) y devuelve su id.
        private int CrearUsuario(string login, string rol = RolUsuario.Cajero)
        {
            return svc.Crear(new Usuario { Nombre = login, LoginNombre = login, Rol = rol }, login + "1234");
        }

        [Fact]
        public void Login_admin_por_defecto_requiere_cambio_de_password()
        {
            var admin = svc.Login("admin", "admin123");
            Assert.NotNull(admin);
            Assert.True(svc.RequiereCambioPassword(admin));
        }

        [Fact]
        public void Empleado_sembrado_tambien_requiere_cambio()
        {
            // 3.D: el usuario demo ya no queda operable con su clave pública; nace forzado a cambiarla.
            var emp = svc.Login("empleado", "empleado123");
            Assert.NotNull(emp);
            Assert.True(svc.RequiereCambioPassword(emp));
        }

        [Fact]
        public void Crear_persiste_activo_y_obliga_cambio_de_password()
        {
            int id = CrearUsuario("juan");
            Assert.True(id > 0);
            var u = svc.ObtenerPorId(id);
            Assert.NotNull(u);
            Assert.Equal("juan", u.LoginNombre);
            Assert.True(u.Activo);
            Assert.True(u.DebeCambiarPassword);
        }

        [Fact]
        public void Crear_con_login_duplicado_lanza()
        {
            CrearUsuario("ana");
            Assert.Throws<NegocioException>(() => CrearUsuario("ana"));
        }

        [Fact]
        public void Crear_con_login_duplicado_del_admin_sembrado_lanza()
            => Assert.Throws<NegocioException>(() => CrearUsuario("admin"));

        [Fact]
        public void Crear_con_password_corta_lanza()
        {
            var ex = Assert.Throws<NegocioException>(() =>
                svc.Crear(new Usuario { Nombre = "Pepe", LoginNombre = "pepe", Rol = RolUsuario.Cajero }, "123"));
            Assert.Contains("contraseña", ex.Message.ToLowerInvariant());
        }

        [Fact]
        public void Crear_con_rol_invalido_lanza()
            => Assert.Throws<NegocioException>(() =>
                svc.Crear(new Usuario { Nombre = "X", LoginNombre = "x", Rol = "Root" }, "clave123"));

        [Fact]
        public void CambiarPasswordPropia_cambia_clave_y_quita_el_flag()
        {
            var admin = svc.Login("admin", "admin123");
            svc.CambiarPasswordPropia(admin.IdUsuario, "admin123", "claveNueva1");

            Assert.Null(svc.Login("admin", "admin123"));          // la clave vieja ya no sirve
            var reLogin = svc.Login("admin", "claveNueva1");
            Assert.NotNull(reLogin);
            Assert.False(svc.RequiereCambioPassword(reLogin));     // y ya no se le obliga a cambiarla
        }

        [Fact]
        public void CambiarPasswordPropia_con_actual_incorrecta_lanza()
        {
            var admin = svc.Login("admin", "admin123");
            Assert.Throws<NegocioException>(() =>
                svc.CambiarPasswordPropia(admin.IdUsuario, "incorrecta", "claveNueva1"));
        }

        [Fact]
        public void CambiarPasswordPropia_con_clave_igual_a_la_actual_lanza()
        {
            var admin = svc.Login("admin", "admin123");
            Assert.Throws<NegocioException>(() =>
                svc.CambiarPasswordPropia(admin.IdUsuario, "admin123", "admin123"));
        }

        [Fact]
        public void ResetearPassword_obliga_a_cambiarla_en_el_proximo_login()
        {
            var emp = svc.Login("empleado", "empleado123");
            svc.ResetearPassword(emp.IdUsuario, "temporal1");

            var reLogin = svc.Login("empleado", "temporal1");
            Assert.NotNull(reLogin);
            Assert.True(svc.RequiereCambioPassword(reLogin));      // queda marcado para cambio forzado
        }

        [Fact]
        public void Desactivar_impide_el_login()
        {
            int id = CrearUsuario("temporal");
            Assert.NotNull(svc.Login("temporal", "temporal1234"));
            svc.Desactivar(id);
            Assert.Null(svc.Login("temporal", "temporal1234"));    // inactivo => login null
        }

        [Fact]
        public void Desactivar_al_unico_admin_lanza()
        {
            var admin = svc.Login("admin", "admin123");
            Assert.Throws<NegocioException>(() => svc.Desactivar(admin.IdUsuario));
        }

        [Fact]
        public void Actualizar_quitar_rol_admin_al_ultimo_admin_lanza()
        {
            var admin = svc.Login("admin", "admin123");
            admin.Rol = RolUsuario.Cajero;
            Assert.Throws<NegocioException>(() => svc.Actualizar(admin));
        }

        [Fact]
        public void Con_dos_admins_se_puede_desactivar_a_uno()
        {
            int id2 = CrearUsuario("admin2", RolUsuario.Admin);
            svc.Desactivar(id2);                                   // queda el admin sembrado activo
            Assert.False(svc.ObtenerPorId(id2).Activo);
        }

        [Fact]
        public void Actualizar_edita_nombre_y_login()
        {
            int id = CrearUsuario("carlos");
            var u = svc.ObtenerPorId(id);
            u.Nombre = "Carlos Pérez";
            u.LoginNombre = "cperez";
            svc.Actualizar(u);

            var recargado = svc.ObtenerPorId(id);
            Assert.Equal("Carlos Pérez", recargado.Nombre);
            Assert.Equal("cperez", recargado.LoginNombre);
        }

        [Fact]
        public void No_puedo_desactivar_mi_propio_usuario()
        {
            int id2 = CrearUsuario("admin2", RolUsuario.Admin);   // segundo admin (evita el guard de "último admin")
            Sesion.UsuarioActual = svc.ObtenerPorId(id2);
            try
            {
                var ex = Assert.Throws<NegocioException>(() => svc.Desactivar(id2));
                Assert.Contains("propio", ex.Message.ToLowerInvariant());
            }
            finally { Sesion.UsuarioActual = null; }
        }

        [Fact]
        public void ObtenerTodos_incluye_admin_y_empleado_sembrados()
        {
            var todos = svc.ObtenerTodos();
            Assert.Contains(todos, u => u.LoginNombre == "admin");
            Assert.Contains(todos, u => u.LoginNombre == "empleado");
        }
    }
}

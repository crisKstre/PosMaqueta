using System.Collections.Generic;
using AccesoData;
using AccesoData.DAO;
using Dominio.Eventos;
using Entidades;

namespace Dominio.Servicios
{
    public class UsuarioService
    {
        public const int LargoMinimoPassword = 6;

        private readonly UsuarioDao usuarioDao = new UsuarioDao();
        private readonly LogService logService = new LogService();

        public Usuario Login(string loginNombre, string pass)
        {
            return usuarioDao.Login(loginNombre, pass);
        }

        // True si el usuario debe cambiar su contraseña antes de operar (admin por defecto,
        // usuario recién creado o reseteado por un admin).
        public bool RequiereCambioPassword(Usuario u)
        {
            return u != null && u.DebeCambiarPassword;
        }

        public List<Usuario> ObtenerTodos()
        {
            return usuarioDao.ObtenerTodos();
        }

        public Usuario ObtenerPorId(int idUsuario)
        {
            return usuarioDao.ObtenerPorId(idUsuario);
        }

        // Alta. El usuario nace obligado a cambiar la contraseña que le asignó el administrador.
        public int Crear(Usuario u, string passwordPlano)
        {
            u.Nombre = (u.Nombre ?? "").Trim();
            u.LoginNombre = (u.LoginNombre ?? "").Trim();
            ValidarPerfil(u, esNuevo: true);
            ValidarPassword(passwordPlano);

            u.Pass = Seguridad.Hash(passwordPlano);
            u.Activo = true;
            u.DebeCambiarPassword = true;

            int id = usuarioDao.Insertar(u);
            Log.Info("Usuario creado N°" + id + ": '" + u.LoginNombre + "' (" + u.Rol + ")");
            logService.Registrar(ModuloLog.Usuarios, "Alta", "Usuario: " + u.LoginNombre + " (" + u.Rol + ")");
            NotificadorCambios.Notificar(Entidad.Usuario);
            return id;
        }

        // Edición de perfil (nombre, login, rol, activo). No cambia la contraseña.
        public void Actualizar(Usuario u)
        {
            u.Nombre = (u.Nombre ?? "").Trim();
            u.LoginNombre = (u.LoginNombre ?? "").Trim();
            ValidarPerfil(u, esNuevo: false);

            var original = usuarioDao.ObtenerPorId(u.IdUsuario);
            if (original == null) throw new NegocioException("El usuario no existe.");

            // No dejar el sistema sin administradores: si este era el último admin activo y se le
            // quita el rol o se le desactiva, se rechaza.
            bool perdiendoAdmin = original.Rol == RolUsuario.Admin && original.Activo &&
                                  (u.Rol != RolUsuario.Admin || !u.Activo);
            if (perdiendoAdmin && usuarioDao.ContarAdminsActivos() <= 1)
                throw new NegocioException("No puedes dejar el sistema sin administradores activos.");

            if (EsUsuarioActual(u.IdUsuario) && !u.Activo)
                throw new NegocioException("No puedes desactivar tu propio usuario.");

            usuarioDao.Actualizar(u);
            Log.Info("Usuario actualizado N°" + u.IdUsuario + ": '" + u.LoginNombre + "' (" + u.Rol +
                     ", " + (u.Activo ? "activo" : "inactivo") + ")");
            logService.Registrar(ModuloLog.Usuarios, "Modificación", "Usuario: " + u.LoginNombre);
            NotificadorCambios.Notificar(Entidad.Usuario);
        }

        public void Activar(int idUsuario)
        {
            CambiarEstado(idUsuario, true);
        }

        public void Desactivar(int idUsuario)
        {
            CambiarEstado(idUsuario, false);
        }

        private void CambiarEstado(int idUsuario, bool activo)
        {
            var u = usuarioDao.ObtenerPorId(idUsuario);
            if (u == null) throw new NegocioException("El usuario no existe.");

            if (!activo)
            {
                if (EsUsuarioActual(idUsuario))
                    throw new NegocioException("No puedes desactivar tu propio usuario.");
                if (u.Rol == RolUsuario.Admin && u.Activo && usuarioDao.ContarAdminsActivos() <= 1)
                    throw new NegocioException("No puedes desactivar al único administrador activo.");
            }

            usuarioDao.CambiarEstado(idUsuario, activo);
            Log.Info("Usuario " + (activo ? "activado" : "desactivado") + " N°" + idUsuario + " ('" + u.LoginNombre + "')");
            logService.Registrar(ModuloLog.Usuarios, activo ? "Activación" : "Desactivación", "Usuario: " + u.LoginNombre);
            NotificadorCambios.Notificar(Entidad.Usuario);
        }

        // Cambio voluntario de la propia contraseña: exige la actual correcta.
        public void CambiarPasswordPropia(int idUsuario, string passwordActual, string passwordNueva)
        {
            var u = usuarioDao.ObtenerPorId(idUsuario);
            if (u == null) throw new NegocioException("El usuario no existe.");
            if (!Seguridad.Verificar(passwordActual ?? "", u.Pass))
                throw new NegocioException("La contraseña actual no es correcta.");
            ValidarPassword(passwordNueva);
            if (Seguridad.Verificar(passwordNueva, u.Pass))
                throw new NegocioException("La nueva contraseña debe ser distinta de la actual.");

            usuarioDao.CambiarPass(idUsuario, Seguridad.Hash(passwordNueva), debeCambiar: false);
            Log.Info("Contraseña cambiada por el propio usuario N°" + idUsuario + " ('" + u.LoginNombre + "')");
            logService.Registrar(ModuloLog.Usuarios, "Cambio de contraseña", "Usuario: " + u.LoginNombre);
        }

        // Reseteo por un administrador: deja una contraseña temporal que el usuario deberá cambiar.
        public void ResetearPassword(int idUsuario, string passwordNueva)
        {
            var u = usuarioDao.ObtenerPorId(idUsuario);
            if (u == null) throw new NegocioException("El usuario no existe.");
            ValidarPassword(passwordNueva);

            usuarioDao.CambiarPass(idUsuario, Seguridad.Hash(passwordNueva), debeCambiar: true);
            Log.Advertencia("Contraseña reseteada por admin para usuario N°" + idUsuario + " ('" + u.LoginNombre + "')");
            logService.Registrar(ModuloLog.Usuarios, "Reseteo de contraseña", "Usuario: " + u.LoginNombre);
            NotificadorCambios.Notificar(Entidad.Usuario);
        }

        private void ValidarPerfil(Usuario u, bool esNuevo)
        {
            if (string.IsNullOrWhiteSpace(u.Nombre))
                throw new NegocioException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(u.LoginNombre))
                throw new NegocioException("El nombre de inicio de sesión es obligatorio.");
            if (u.Rol != RolUsuario.Admin && u.Rol != RolUsuario.Cajero)
                throw new NegocioException("El rol no es válido.");
            if (usuarioDao.ExisteLogin(u.LoginNombre, esNuevo ? 0 : u.IdUsuario))
                throw new NegocioException("Ya existe un usuario con ese nombre de inicio de sesión.");
        }

        private void ValidarPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < LargoMinimoPassword)
                throw new NegocioException("La contraseña debe tener al menos " + LargoMinimoPassword + " caracteres.");
        }

        private static bool EsUsuarioActual(int idUsuario)
        {
            return Sesion.UsuarioActual != null && Sesion.UsuarioActual.IdUsuario == idUsuario;
        }
    }
}

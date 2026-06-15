using System;
using System.Collections.Generic;
using AccesoData.DAO;
using Entidades;

namespace Dominio.Servicios
{
    public class LogService
    {
        private readonly LogDao logDao = new LogDao();

        public void Registrar(string modulo, string accion, string detalle = null)
        {
            if (Sesion.UsuarioActual == null) return;
            logDao.Registrar(new LogMovimiento
            {
                Fecha = DateTime.Now,
                IdUsuario = Sesion.UsuarioActual.IdUsuario,
                NombreUsuario = Sesion.UsuarioActual.Nombre,
                Modulo = modulo,
                Accion = accion,
                Detalle = detalle
            });
        }

        public List<LogMovimiento> Obtener(DateTime desde, DateTime hasta, int idUsuario = 0, string modulo = null)
        {
            return logDao.Obtener(desde, hasta, idUsuario, modulo);
        }

        public List<string> ObtenerUsuarios()
        {
            return logDao.ObtenerUsuariosConLog();
        }
    }
}

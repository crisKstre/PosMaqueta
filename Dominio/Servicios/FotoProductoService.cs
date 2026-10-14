using System;
using AccesoData.DAO;
using Dominio.Eventos;
using Entidades;

namespace Dominio.Servicios
{
    public sealed class FotoProductoService
    {
        private readonly FotoProductoDao dao = new FotoProductoDao();
        public byte[] ObtenerImagen(int idProducto) => dao.ObtenerImagen(idProducto);
        public byte[] ObtenerMiniatura(int idProducto) => dao.ObtenerMiniatura(idProducto);

        public void Guardar(int idProducto, byte[] archivo)
        {
            Autorizacion.ExigirAdmin();
            ExigirProducto(idProducto);
            var foto = ProcesadorFotoProducto.Preparar(archivo);
            dao.Guardar(idProducto, foto, Auditoria(idProducto, "Guardar foto"));
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public void Quitar(int idProducto)
        {
            Autorizacion.ExigirAdmin();
            ExigirProducto(idProducto);
            dao.Guardar(idProducto, null, Auditoria(idProducto, "Quitar foto"));
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        private static void ExigirProducto(int id)
        {
            if (new ProductoDao().ObtenerPorId(id) == null) throw new NegocioException("El producto no existe.");
        }

        private static LogMovimiento Auditoria(int id, string accion) => new LogMovimiento
        {
            Fecha = DateTime.Now, IdUsuario = Sesion.UsuarioActual.IdUsuario,
            NombreUsuario = Sesion.UsuarioActual.Nombre, Modulo = ModuloLog.Productos,
            Accion = accion, Detalle = "Producto N°" + id
        };
    }
}

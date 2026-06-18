using System;
using System.Collections.Generic;
using System.Linq;
using AccesoData;
using AccesoData.DAO;
using Dominio.Eventos;
using Entidades;

namespace Dominio.Servicios
{
    /// <summary>
    /// Devolución parcial o total de una venta: reintegra el stock de los ítems devueltos y registra
    /// la salida de efectivo en la caja abierta (afecta el arqueo). Solo administrador.
    /// </summary>
    public class DevolucionService
    {
        private readonly DevolucionDao devolucionDao = new DevolucionDao();
        private readonly VentaDao      ventaDao      = new VentaDao();
        private readonly CajaDao       cajaDao       = new CajaDao();
        private readonly LogService    logService    = new LogService();

        // Ítems de una venta que TODAVÍA se pueden devolver (cantidad = vendida − ya devuelta).
        public List<DevolucionItem> ObtenerDevolvibles(int idVenta)
        {
            var lista = new List<DevolucionItem>();
            foreach (var d in ventaDao.ObtenerDetalleVenta(idVenta))
            {
                decimal restante = d.Cantidad - devolucionDao.CantidadDevuelta(idVenta, d.IdProducto);
                if (restante <= 0) continue;
                lista.Add(new DevolucionItem
                {
                    IdProducto = d.IdProducto,
                    NombreProducto = d.NombreProducto,
                    Cantidad = restante,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = Dinero.Redondear(restante * d.PrecioUnitario)
                });
            }
            return lista;
        }

        // Devuelve los ítems indicados (IdProducto + Cantidad). Reintegra stock y registra la salida
        // de efectivo en la caja abierta. Los precios se toman de la venta original (autoritativo).
        public int Devolver(int idVenta, List<DevolucionItem> items)
        {
            Autorizacion.ExigirAdmin();
            if (items == null || items.Count == 0)
                throw new NegocioException("Selecciona al menos un producto a devolver.");

            var caja = cajaDao.ObtenerCajaAbierta();
            if (caja == null)
                throw new NegocioException("Debe haber una caja abierta para registrar la devolución.");

            // No se puede devolver una venta anulada: ya fue revertida (su stock volvió y nunca contó
            // como ingreso). Devolverla reintegraría stock de más y sacaría efectivo de un ingreso inexistente.
            if (ventaDao.EstaAnulada(idVenta))
                throw new NegocioException("La venta N°" + idVenta + " está anulada; no se puede devolver.");

            var detalles = ventaDao.ObtenerDetalleVenta(idVenta);
            if (detalles.Count == 0)
                throw new NegocioException("La venta no existe o no tiene detalle.");

            var dev = new Devolucion
            {
                IdVenta = idVenta,
                IdCaja = caja.IdCaja,
                Fecha = DateTime.Now,
                IdUsuario = Sesion.UsuarioActual.IdUsuario
            };

            foreach (var pedido in items)
            {
                if (pedido.Cantidad <= 0) continue;
                var original = detalles.FirstOrDefault(d => d.IdProducto == pedido.IdProducto);
                if (original == null)
                    throw new NegocioException("Un producto seleccionado no pertenece a esta venta.");

                decimal disponible = original.Cantidad - devolucionDao.CantidadDevuelta(idVenta, pedido.IdProducto);
                if (pedido.Cantidad > disponible)
                    throw new NegocioException("No puedes devolver más de lo vendido de \"" +
                        original.NombreProducto + "\" (disponible: " + disponible.ToString("0.##") + ").");

                decimal subtotal = Dinero.Redondear(pedido.Cantidad * original.PrecioUnitario);
                dev.Detalles.Add(new DevolucionItem
                {
                    IdProducto = pedido.IdProducto,
                    NombreProducto = original.NombreProducto,
                    Cantidad = pedido.Cantidad,
                    PrecioUnitario = original.PrecioUnitario,
                    Subtotal = subtotal
                });
                dev.Monto += subtotal;
            }

            if (dev.Detalles.Count == 0)
                throw new NegocioException("Selecciona al menos un producto a devolver.");

            int id = devolucionDao.Registrar(dev);
            Log.Advertencia("Devolución N°" + id + " de venta N°" + idVenta + " | $" + dev.Monto.ToString("N0") +
                " | " + dev.Detalles.Count + " ítem(s) | stock reintegrado | caja N°" + caja.IdCaja);
            logService.Registrar(ModuloLog.Ventas, "Devolución",
                "Venta N°" + idVenta + " | $" + dev.Monto.ToString("N0") + " | " + dev.Detalles.Count + " ítem(s)");
            NotificadorCambios.Notificar(Entidad.Venta);
            NotificadorCambios.Notificar(Entidad.Caja);
            NotificadorCambios.Notificar(Entidad.Producto);
            return id;
        }
    }
}

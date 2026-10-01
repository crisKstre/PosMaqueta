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
            => CargarCalculo(idVenta).ObtenerDevolvibles();

        public List<DevolucionItem> Previsualizar(int idVenta, List<DevolucionItem> items)
        {
            Autorizacion.ExigirAdmin();
            return CargarCalculo(idVenta).Calcular(items);
        }

        private CalculoDevolucion CargarCalculo(int idVenta)
        {
            if (ventaDao.EstaAnulada(idVenta))
                throw new NegocioException("La venta N°" + idVenta + " está anulada; no se puede devolver.");
            return new CalculoDevolucion(ventaDao.ObtenerParaDevolucion(idVenta), devolucionDao.ObtenerAcumulado(idVenta));
        }

        // Devuelve los ítems indicados (IdProducto + Cantidad). Reintegra stock y registra la salida
        // de efectivo en la caja abierta. El cálculo se repite con datos persistidos, nunca con precios de la UI.
        public int Devolver(int idVenta, List<DevolucionItem> items, decimal? montoEsperado = null)
        {
            Autorizacion.ExigirAdmin();
            if (items == null || items.Count == 0)
                throw new NegocioException("Selecciona al menos un producto a devolver.");

            var caja = cajaDao.ObtenerCajaAbierta();
            if (caja == null)
                throw new NegocioException("Debe haber una caja abierta para registrar la devolución.");

            var dev = new Devolucion
            {
                IdVenta = idVenta,
                IdCaja = caja.IdCaja,
                Fecha = DateTime.Now,
                IdUsuario = Sesion.UsuarioActual.IdUsuario,
                Detalles = Previsualizar(idVenta, items)
            };
            dev.Monto = dev.Detalles.Sum(d => d.Subtotal);
            if (montoEsperado.HasValue && montoEsperado.Value != dev.Monto)
                throw new NegocioException("El importe a reembolsar cambió. Revisa nuevamente la devolución.");

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

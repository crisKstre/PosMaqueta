using System;
using System.Collections.Generic;
using System.Linq;
using AccesoData;
using AccesoData.DAO;
using Dominio.Eventos;
using Entidades;

namespace Dominio.Servicios
{
    public class VentaService
    {
        private readonly VentaDao ventaDao = new VentaDao();
        private readonly ProductoDao productoDao = new ProductoDao();
        private readonly CajaDao cajaDao = new CajaDao();
        private readonly LogService logService = new LogService();

        // ── Ventas en curso ───────────────────────────────────────────────
        // Estado COMPARTIDO (estático): persiste aunque se recree el FormVentas
        // al navegar entre módulos, y permite varias ventas simultáneas.
        private static readonly List<VentaEnCurso> enCurso = new List<VentaEnCurso>();
        private static VentaEnCurso activa;
        private static int contador = 0;

        public IReadOnlyList<VentaEnCurso> VentasEnCurso => enCurso;

        /// <summary>La venta en primer plano. Si no hay ninguna, crea una.</summary>
        public VentaEnCurso Activa
        {
            get
            {
                if (activa == null) NuevaVenta();
                return activa;
            }
        }

        public IReadOnlyList<DetalleVenta> Carrito => Activa.Detalles;
        public decimal Subtotal => Activa.Subtotal;
        public decimal Descuento => Activa.Descuento;
        public decimal Total => Activa.Total;

        // Fija el descuento (monto en $) de la venta activa, acotado al subtotal.
        public void AplicarDescuento(decimal monto)
        {
            if (monto < 0) monto = 0;
            // El descuento EFECTIVO se acota dinámicamente al subtotal (VentaEnCurso.Descuento),
            // así que si luego se quitan ítems el descuento baja con el carrito.
            Activa.DescuentoSolicitado = monto;
            Activa.UltimaActividad = DateTime.Now;
            Log.Info("Descuento aplicado $" + Activa.Descuento.ToString("N0") + " (solicitado $" + monto.ToString("N0") +
                     ", subtotal $" + Activa.Subtotal.ToString("N0") + ")");
        }

        public VentaEnCurso NuevaVenta()
        {
            contador++;
            var v = new VentaEnCurso
            {
                Id = contador,
                Etiqueta = "Venta " + contador,
                UltimaActividad = DateTime.Now
            };
            enCurso.Add(v);
            activa = v;
            return v;
        }

        public void ActivarVenta(int id)
        {
            var v = enCurso.FirstOrDefault(x => x.Id == id);
            if (v != null)
            {
                activa = v;
                activa.UltimaActividad = DateTime.Now;
            }
        }

        /// <summary>Descarta una venta en curso (su carrito se pierde).</summary>
        public void CerrarVenta(int id)
        {
            var v = enCurso.FirstOrDefault(x => x.Id == id);
            if (v == null) return;
            enCurso.Remove(v);
            if (activa == v) activa = enCurso.LastOrDefault();
        }

        /// <summary>
        /// Cierra las ventas EN PAUSA (todas menos la activa) sin actividad por más
        /// de 'limite'. La activa (en primer plano) nunca se cierra. Devuelve cuántas cerró.
        /// </summary>
        public int CerrarPausadasInactivas(TimeSpan limite)
        {
            var ahora = DateTime.Now;
            var aCerrar = enCurso
                .Where(v => v != activa && (ahora - v.UltimaActividad) > limite)
                .ToList();
            foreach (var v in aCerrar) enCurso.Remove(v);
            if (aCerrar.Count > 0)
                Log.Advertencia("Auto-cierre de " + aCerrar.Count + " venta(s) en pausa por inactividad");
            return aCerrar.Count;
        }

        // Descarta TODAS las ventas en curso (p. ej. al cerrar sesión un cajero).
        public static void ReiniciarVentasEnCurso()
        {
            enCurso.Clear();
            activa = null;
        }

        public void AgregarPorCodigo(string codigoBarras, decimal cantidad)
        {
            if (cantidad <= 0)
                throw new NegocioException("La cantidad debe ser mayor a cero.");

            var producto = productoDao.ObtenerPorCodigo(codigoBarras);
            if (producto == null)
                throw new NegocioException("Producto no encontrado con ese código de barras.");

            ValidarStock(producto, cantidad);
            AgregarProducto(producto, cantidad);
        }

        public List<Producto> BuscarProductos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<Producto>();
            // Solo activos para vender
            return productoDao.Buscar(texto.Trim()).FindAll(p => p.Activo);
        }

        public void AgregarPorId(int idProducto, decimal cantidad)
        {
            if (cantidad <= 0)
                throw new NegocioException("La cantidad debe ser mayor a cero.");

            var producto = productoDao.ObtenerPorId(idProducto);
            if (producto == null)
                throw new NegocioException("El producto no existe.");

            ValidarStock(producto, cantidad);
            AgregarProducto(producto, cantidad);
        }

        private void ValidarStock(Producto producto, decimal cantidadAgregar)
        {
            // Stock actual en BD menos lo que ya está en el carrito de la venta activa
            decimal yaEnCarrito = 0;
            var item = Activa.Detalles.FirstOrDefault(d => d.IdProducto == producto.IdProducto);
            if (item != null) yaEnCarrito = item.Cantidad;

            decimal disponible = producto.Stock - yaEnCarrito;
            if (cantidadAgregar > disponible)
                throw new NegocioException(
                    "Stock insuficiente para \"" + producto.Nombre + "\".\n" +
                    "Disponible: " + disponible.ToString("0.##") + " " + producto.UnidadMedida + ".");
        }

        public void AgregarProducto(Producto producto, decimal cantidad)
        {
            var carrito = Activa.Detalles;
            var existente = carrito.FirstOrDefault(d => d.IdProducto == producto.IdProducto);
            if (existente != null)
            {
                existente.Cantidad += cantidad;
                existente.Subtotal = Dinero.Redondear(existente.Cantidad * existente.PrecioUnitario);
            }
            else
            {
                decimal precioEfectivo = producto.PrecioConDescuento;   // ya incluye el % de oferta del producto
                carrito.Add(new DetalleVenta
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
                    CodigoBarras = producto.CodigoBarras,
                    Cantidad = cantidad,
                    PrecioUnitario = precioEfectivo,
                    PrecioOriginal = producto.Precio,
                    DescuentoPorcentaje = producto.DescuentoPorcentaje,
                    Subtotal = Dinero.Redondear(cantidad * precioEfectivo)
                });
            }
            Activa.UltimaActividad = DateTime.Now;
        }

        public void QuitarDelCarrito(int idProducto)
        {
            var carrito = Activa.Detalles;
            var item = carrito.FirstOrDefault(d => d.IdProducto == idProducto);
            if (item != null) carrito.Remove(item);
            Activa.UltimaActividad = DateTime.Now;
        }

        // Fija la cantidad de un ítem ya en el carrito (para editarlo desde la caja).
        // Si la nueva cantidad es <= 0 se quita el ítem. Valida contra el stock disponible.
        public void CambiarCantidad(int idProducto, decimal nuevaCantidad)
        {
            var carrito = Activa.Detalles;
            var item = carrito.FirstOrDefault(d => d.IdProducto == idProducto);
            if (item == null) return;

            if (nuevaCantidad <= 0)
            {
                carrito.Remove(item);
                Activa.UltimaActividad = DateTime.Now;
                return;
            }

            var producto = productoDao.ObtenerPorId(idProducto);
            if (producto == null)
                throw new NegocioException("El producto no existe.");
            if (nuevaCantidad > producto.Stock)
                throw new NegocioException(
                    "Stock insuficiente para \"" + producto.Nombre + "\".\n" +
                    "Disponible: " + producto.Stock.ToString("0.##") + " " + producto.UnidadMedida + ".");

            item.Cantidad = nuevaCantidad;
            item.Subtotal = Dinero.Redondear(item.Cantidad * item.PrecioUnitario);
            Activa.UltimaActividad = DateTime.Now;
        }

        // Suma/resta una cantidad relativa a un ítem del carrito (botones − / +).
        public void AjustarCantidadCarrito(int idProducto, decimal delta)
        {
            var item = Activa.Detalles.FirstOrDefault(d => d.IdProducto == idProducto);
            if (item != null) CambiarCantidad(idProducto, item.Cantidad + delta);
        }

        public void VaciarCarrito()
        {
            Activa.Detalles.Clear();
            Activa.UltimaActividad = DateTime.Now;
        }

        public List<Venta> ObtenerVentasHoy()
        {
            return ventaDao.ObtenerVentas(DateTime.Today, DateTime.Today);
        }

        // ── Reportes ───────────────────────────────────────────────────────
        public List<Venta> ObtenerVentas(DateTime desde, DateTime hasta)
            => ventaDao.ObtenerVentas(desde, hasta);

        public ResumenVentas ObtenerResumenVentas(DateTime desde, DateTime hasta)
            => ventaDao.ObtenerResumen(desde, hasta);

        public List<ProductoVendido> ObtenerTopProductos(DateTime desde, DateTime hasta, int top = 10)
            => ventaDao.ObtenerTopProductos(desde, hasta, top);

        public List<DetalleVenta> ObtenerDetalleVenta(int idVenta)
            => ventaDao.ObtenerDetalleVenta(idVenta);

        // Anula una venta registrada: devuelve el stock y la deja fuera de los reportes.
        public void AnularVenta(int idVenta)
        {
            Autorizacion.ExigirAdmin();
            if (!ventaDao.AnularVenta(idVenta))
                throw new NegocioException("La venta N°" + idVenta + " ya estaba anulada.");
            Log.Advertencia("Venta anulada N°" + idVenta + " (stock devuelto al inventario)");
            logService.Registrar(ModuloLog.Ventas, "Anulación",
                "Venta N°" + idVenta + " anulada (stock devuelto al inventario)");
            NotificadorCambios.Notificar(Entidad.Venta);
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public int CobrarVenta(int idUsuario, string medioPago, int? idCaja = null)
        {
            var actual = Activa;
            if (actual.Detalles.Count == 0)
                throw new NegocioException("El carrito está vacío.");

            // Re-validar stock contra la BD actual en UNA sola consulta: el carrito pudo quedar obsoleto
            // (venta en pausa, otro cajero/módulo movió el stock). DescontarStock es la última defensa atómica.
            var stocks = ventaDao.ObtenerStocks(actual.Detalles.Select(d => d.IdProducto));
            foreach (var d in actual.Detalles)
            {
                if (!stocks.TryGetValue(d.IdProducto, out var prod))
                    throw new NegocioException("El producto \"" + d.NombreProducto + "\" ya no existe.");
                if (d.Cantidad > prod.Stock)
                    throw new NegocioException("Stock insuficiente para \"" + d.NombreProducto +
                        "\". Disponible: " + prod.Stock.ToString("0.##") + " " + prod.UnidadMedida + ".");
            }

            // 0.A — Control interno: no se puede vender sin caja abierta (la venta quedaría
            // fuera de todo arqueo). Si no se pasó idCaja, debe haber una caja abierta.
            if (idCaja == null)
            {
                var cajaAbierta = cajaDao.ObtenerCajaAbierta();
                if (cajaAbierta == null)
                    throw new NegocioException("Debe abrir caja antes de vender.");
                idCaja = cajaAbierta.IdCaja;
            }

            var venta = new Venta
            {
                IdCaja = idCaja,
                IdUsuario = idUsuario,
                Fecha = DateTime.Now,
                Total = actual.Total,
                Descuento = actual.Descuento,
                MedioPago = medioPago,
                Detalles = new List<DetalleVenta>(actual.Detalles)
            };

            int idVenta = ventaDao.RegistrarVenta(venta);
            string itemsLog = string.Join(", ", actual.Detalles.Select(d =>
                d.NombreProducto + " x" + d.Cantidad.ToString("0.##") +
                (d.TieneDescuento ? " (-" + d.DescuentoPorcentaje.ToString("0.##") + "%)" : "")));
            Log.Info("VENTA N°" + idVenta + " | " + medioPago + " | Total $" + venta.Total.ToString("N0") +
                     " | Neto $" + Impuestos.Neto(venta.Total).ToString("N0") +
                     " | IVA $" + Impuestos.Iva(venta.Total).ToString("N0") +
                     (venta.Descuento > 0 ? " | Desc $" + venta.Descuento.ToString("N0") : "") +
                     " | usuario " + idUsuario + " | " + actual.Detalles.Count + " items: [" + itemsLog + "]");
            string detalleLog = "N°" + idVenta + " | $" + venta.Total.ToString("N0") + " | " + medioPago;
            if (venta.Descuento > 0) detalleLog += " | desc. $" + venta.Descuento.ToString("N0");
            logService.Registrar(ModuloLog.Ventas, "Venta", detalleLog);

            CerrarVenta(actual.Id);   // la venta cobrada deja de estar en curso
            NotificadorCambios.Notificar(Entidad.Venta);
            NotificadorCambios.Notificar(Entidad.Producto);
            return idVenta;
        }
    }
}

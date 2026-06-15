using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly List<DetalleVenta> carrito = new List<DetalleVenta>();
        public IReadOnlyList<DetalleVenta> Carrito => carrito;
        public decimal Total => carrito.Sum(d => d.Subtotal);

        public void AgregarPorCodigo(string codigoBarras, decimal cantidad)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            var producto = productoDao.ObtenerPorCodigo(codigoBarras);
            if (producto == null)
                throw new InvalidOperationException("Producto no encontrado con ese código de barras.");

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
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            var producto = productoDao.ObtenerPorId(idProducto);
            if (producto == null)
                throw new InvalidOperationException("El producto no existe.");

            ValidarStock(producto, cantidad);
            AgregarProducto(producto, cantidad);
        }

        private void ValidarStock(Producto producto, decimal cantidadAgregar)
        {
            // Stock actual en BD menos lo que ya está en el carrito para este producto
            decimal enCarrito = 0;
            var enCarritoItem = carrito.FirstOrDefault(d => d.IdProducto == producto.IdProducto);
            if (enCarritoItem != null) enCarrito = enCarritoItem.Cantidad;

            decimal disponible = producto.Stock - enCarrito;
            if (cantidadAgregar > disponible)
                throw new InvalidOperationException(
                    "Stock insuficiente para \"" + producto.Nombre + "\".\n" +
                    "Disponible: " + disponible.ToString("0.##") + " " + producto.UnidadMedida + ".");
        }

        public void AgregarProducto(Producto producto, decimal cantidad)
        {
            var existente = carrito.FirstOrDefault(d => d.IdProducto == producto.IdProducto);
            if (existente != null)
            {
                existente.Cantidad += cantidad;
                existente.Subtotal = existente.Cantidad * existente.PrecioUnitario;
            }
            else
            {
                carrito.Add(new DetalleVenta
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.Precio,
                    Subtotal = cantidad * producto.Precio
                });
            }
        }

        public void QuitarDelCarrito(int idProducto)
        {
            var item = carrito.FirstOrDefault(d => d.IdProducto == idProducto);
            if (item != null) carrito.Remove(item);
        }

        // Fija la cantidad de un ítem ya en el carrito (para editarlo desde la caja).
        // Si la nueva cantidad es <= 0 se quita el ítem. Valida contra el stock disponible.
        public void CambiarCantidad(int idProducto, decimal nuevaCantidad)
        {
            var item = carrito.FirstOrDefault(d => d.IdProducto == idProducto);
            if (item == null) return;

            if (nuevaCantidad <= 0)
            {
                carrito.Remove(item);
                return;
            }

            var producto = productoDao.ObtenerPorId(idProducto);
            if (producto == null)
                throw new InvalidOperationException("El producto no existe.");
            if (nuevaCantidad > producto.Stock)
                throw new InvalidOperationException(
                    "Stock insuficiente para \"" + producto.Nombre + "\".\n" +
                    "Disponible: " + producto.Stock.ToString("0.##") + " " + producto.UnidadMedida + ".");

            item.Cantidad = nuevaCantidad;
            item.Subtotal = item.Cantidad * item.PrecioUnitario;
        }

        // Suma/resta una cantidad relativa a un ítem del carrito (botones − / +).
        public void AjustarCantidadCarrito(int idProducto, decimal delta)
        {
            var item = carrito.FirstOrDefault(d => d.IdProducto == idProducto);
            if (item == null) return;
            CambiarCantidad(idProducto, item.Cantidad + delta);
        }

        public void VaciarCarrito() => carrito.Clear();

        public System.Collections.Generic.List<Venta> ObtenerVentasHoy()
        {
            return ventaDao.ObtenerVentas(DateTime.Today, DateTime.Today);
        }

        public int CobrarVenta(int idUsuario, string medioPago, int? idCaja = null)
        {
            if (carrito.Count == 0)
                throw new InvalidOperationException("El carrito está vacío.");

            if (idCaja == null)
            {
                var cajaAbierta = cajaDao.ObtenerCajaAbierta();
                if (cajaAbierta != null) idCaja = cajaAbierta.IdCaja;
            }

            var venta = new Venta
            {
                IdCaja = idCaja,
                IdUsuario = idUsuario,
                Fecha = DateTime.Now,
                Total = Total,
                MedioPago = medioPago,
                Detalles = new List<DetalleVenta>(carrito)
            };

            int idVenta = ventaDao.RegistrarVenta(venta);
            logService.Registrar(ModuloLog.Ventas, "Venta",
                "N°" + idVenta + " | $" + venta.Total.ToString("N0") + " | " + medioPago);
            VaciarCarrito();
            NotificadorCambios.Notificar(Entidad.Venta);
            NotificadorCambios.Notificar(Entidad.Producto);
            return idVenta;
        }
    }
}

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

        // El carrito vive en memoria mientras se arma la venta
        private readonly List<DetalleVenta> carrito = new List<DetalleVenta>();

        public IReadOnlyList<DetalleVenta> Carrito => carrito;

        public decimal Total => carrito.Sum(d => d.Subtotal);

        // Agrega un producto al carrito por código de barras.
        // Si ya está en el carrito, suma la cantidad.
        public void AgregarPorCodigo(string codigoBarras, decimal cantidad)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            var producto = productoDao.ObtenerPorCodigo(codigoBarras);
            if (producto == null)
                throw new InvalidOperationException("No se encontró un producto con ese código.");

            AgregarProducto(producto, cantidad);
        }

        // Busca productos por nombre o código (para el autocompletado de la UI)
        public List<Producto> BuscarProductos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<Producto>();
            return productoDao.Buscar(texto.Trim());
        }

        public void AgregarPorId(int idProducto, decimal cantidad)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            var producto = productoDao.ObtenerPorId(idProducto);
            if (producto == null)
                throw new InvalidOperationException("El producto no existe.");

            AgregarProducto(producto, cantidad);
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
            if (item != null)
                carrito.Remove(item);
        }

        public void VaciarCarrito()
        {
            carrito.Clear();
        }

        // Cierra la venta: arma la cabecera, la registra con su detalle,
        // descuenta stock y vacía el carrito. Devuelve el Id de la venta.
        public int CobrarVenta(int idUsuario, string medioPago, int? idCaja = null)
        {
            if (carrito.Count == 0)
                throw new InvalidOperationException("El carrito está vacío.");

            // Si no se pasó una caja, se asocia a la caja abierta actual (si existe).
            // Si no hay caja abierta, la venta queda con IdCaja = NULL.
            if (idCaja == null)
            {
                var cajaAbierta = cajaDao.ObtenerCajaAbierta();
                if (cajaAbierta != null)
                    idCaja = cajaAbierta.IdCaja;
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

            VaciarCarrito();
            NotificadorCambios.Notificar(Entidad.Venta);
            NotificadorCambios.Notificar(Entidad.Producto);

            return idVenta;
        }
    }
}

using System;
using System.Collections.Generic;
using AccesoData.DAO;
using Dominio.Eventos;
using Entidades;

namespace Dominio.Servicios
{
    public class ProductoService
    {
        private readonly ProductoDao productoDao = new ProductoDao();

        public List<Producto> ObtenerTodos()
        {
            return productoDao.ObtenerTodos();
        }

        public List<Producto> Buscar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return productoDao.ObtenerTodos();
            return productoDao.Buscar(texto.Trim());
        }

        public Producto ObtenerPorCodigo(string codigoBarras)
        {
            return productoDao.ObtenerPorCodigo(codigoBarras);
        }

        public Producto ObtenerPorId(int idProducto)
        {
            return productoDao.ObtenerPorId(idProducto);
        }

        // Devuelve mensaje de error, o null si todo OK
        public string Validar(Producto p, bool esNuevo)
        {
            if (string.IsNullOrWhiteSpace(p.Nombre))
                return "El nombre es obligatorio.";

            if (p.Precio < 0)
                return "El precio no puede ser negativo.";

            if (p.Stock < 0)
                return "El stock no puede ser negativo.";

            if (p.StockMinimo < 0)
                return "El stock mínimo no puede ser negativo.";

            if (!string.IsNullOrWhiteSpace(p.CodigoBarras))
            {
                int idExcluir = esNuevo ? 0 : p.IdProducto;
                if (productoDao.ExisteCodigo(p.CodigoBarras.Trim(), idExcluir))
                    return "Ya existe un producto con ese código de barras.";
            }

            return null;
        }

        public int Crear(Producto p)
        {
            string error = Validar(p, true);
            if (error != null)
                throw new InvalidOperationException(error);

            int id = productoDao.Insertar(p);
            NotificadorCambios.Notificar(Entidad.Producto);
            return id;
        }

        public void Actualizar(Producto p)
        {
            string error = Validar(p, false);
            if (error != null)
                throw new InvalidOperationException(error);

            productoDao.Actualizar(p);
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public void Desactivar(int idProducto)
        {
            productoDao.Desactivar(idProducto);
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        // Suma (delta positivo = entrada) o resta (delta negativo = salida) stock.
        // Devuelve el nuevo stock resultante.
        public decimal AjustarStock(int idProducto, decimal delta)
        {
            var p = productoDao.ObtenerPorId(idProducto);
            if (p == null)
                throw new InvalidOperationException("El producto no existe.");

            decimal nuevoStock = p.Stock + delta;
            if (nuevoStock < 0)
                throw new InvalidOperationException(
                    "Stock insuficiente. Stock actual: " + p.Stock.ToString("0.##") + " " + p.UnidadMedida + ".");

            productoDao.ActualizarStock(idProducto, nuevoStock);
            NotificadorCambios.Notificar(Entidad.Producto);
            return nuevoStock;
        }

        // Borrado permanente. Solo si el producto no tiene ventas registradas;
        // si las tiene, se debe desactivar en vez de eliminar.
        public void Eliminar(int idProducto)
        {
            if (productoDao.TieneVentas(idProducto))
                throw new InvalidOperationException(
                    "No se puede eliminar: el producto tiene ventas registradas. Usa Desactivar para conservar el historial.");

            productoDao.Eliminar(idProducto);
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public bool EstaBajoStock(Producto p)
        {
            return p.Stock <= p.StockMinimo;
        }
    }
}

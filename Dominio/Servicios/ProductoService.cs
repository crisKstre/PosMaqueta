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
        private readonly LogService logService = new LogService();

        public List<Producto> ObtenerActivos()
        {
            return productoDao.ObtenerTodos(soloActivos: true);
        }

        public List<string> ObtenerCategorias()
        {
            return productoDao.ObtenerCategorias();
        }

        public List<Producto> ObtenerPorCategoria(string categoria)
        {
            return productoDao.ObtenerPorCategoria(categoria);
        }

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
            if (error != null) throw new InvalidOperationException(error);
            int id = productoDao.Insertar(p);
            logService.Registrar(ModuloLog.Productos, "Alta", "Producto: " + p.Nombre);
            NotificadorCambios.Notificar(Entidad.Producto);
            return id;
        }

        public void Actualizar(Producto p)
        {
            string error = Validar(p, false);
            if (error != null) throw new InvalidOperationException(error);
            productoDao.Actualizar(p);
            logService.Registrar(ModuloLog.Productos, "Modificación", "Producto: " + p.Nombre);
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public void Desactivar(int idProducto)
        {
            var p = productoDao.ObtenerPorId(idProducto);
            productoDao.Desactivar(idProducto);
            logService.Registrar(ModuloLog.Productos, "Desactivación", "Producto: " + (p?.Nombre ?? idProducto.ToString()));
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public void Activar(int idProducto)
        {
            var p = productoDao.ObtenerPorId(idProducto);
            productoDao.Activar(idProducto);
            logService.Registrar(ModuloLog.Productos, "Activación", "Producto: " + (p?.Nombre ?? idProducto.ToString()));
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public decimal AjustarStock(int idProducto, decimal delta)
        {
            var p = productoDao.ObtenerPorId(idProducto);
            if (p == null) throw new InvalidOperationException("El producto no existe.");
            decimal nuevoStock = p.Stock + delta;
            if (nuevoStock < 0) throw new InvalidOperationException(
                "Stock insuficiente. Stock actual: " + p.Stock.ToString("0.##") + " " + p.UnidadMedida + ".");
            productoDao.ActualizarStock(idProducto, nuevoStock);
            string accion = delta >= 0 ? "Entrada de stock" : "Salida de stock";
            logService.Registrar(ModuloLog.Productos, accion,
                p.Nombre + " | Δ" + (delta >= 0 ? "+" : "") + delta.ToString("0.##") + " → stock: " + nuevoStock.ToString("0.##"));
            NotificadorCambios.Notificar(Entidad.Producto);
            return nuevoStock;
        }

        public void Eliminar(int idProducto)
        {
            if (productoDao.TieneVentas(idProducto)) throw new InvalidOperationException(
                "No se puede eliminar: el producto tiene ventas registradas. Usa Desactivar para conservar el historial.");
            var p = productoDao.ObtenerPorId(idProducto);
            productoDao.Eliminar(idProducto);
            logService.Registrar(ModuloLog.Productos, "Eliminación", "Producto: " + (p?.Nombre ?? idProducto.ToString()));
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public List<Producto> ObtenerBajoStock()
        {
            return productoDao.ObtenerTodos(soloActivos: true)
                .FindAll(p => p.Stock <= p.StockMinimo);
        }

        public bool EstaBajoStock(Producto p)
        {
            return p.Stock <= p.StockMinimo;
        }
    }
}

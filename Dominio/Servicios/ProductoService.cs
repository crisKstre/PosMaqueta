using System;
using System.Collections.Generic;
using AccesoData;
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

            if (p.DescuentoPorcentaje < 0 || p.DescuentoPorcentaje > 100)
                return "El descuento debe estar entre 0 y 100%.";

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
            if (error != null) throw new NegocioException(error);
            int id = productoDao.Insertar(p);
            Log.Info("Producto creado N°" + id + ": " + p.Nombre + " | $" + p.Precio.ToString("N0") +
                     " | stock " + p.Stock.ToString("0.##") + " " + p.UnidadMedida + " | cat: " + p.Categoria +
                     (p.TieneDescuento ? " | desc " + p.DescuentoPorcentaje.ToString("0.##") + "%" : ""));
            logService.Registrar(ModuloLog.Productos, "Alta", "Producto: " + p.Nombre);
            NotificadorCambios.Notificar(Entidad.Producto);
            return id;
        }

        public void Actualizar(Producto p)
        {
            string error = Validar(p, false);
            if (error != null) throw new NegocioException(error);
            productoDao.Actualizar(p);
            Log.Info("Producto actualizado N°" + p.IdProducto + ": " + p.Nombre + " | $" + p.Precio.ToString("N0") +
                     " | stock " + p.Stock.ToString("0.##") +
                     (p.TieneDescuento ? " | desc " + p.DescuentoPorcentaje.ToString("0.##") + "%" : ""));
            logService.Registrar(ModuloLog.Productos, "Modificación", "Producto: " + p.Nombre);
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        // Aplica (o quita, con 0) el descuento de oferta de un producto, sin tocar el resto de sus datos.
        public void AplicarDescuento(int idProducto, decimal porcentaje)
        {
            if (porcentaje < 0 || porcentaje > 100)
                throw new NegocioException("El descuento debe estar entre 0 y 100%.");
            var p = productoDao.ObtenerPorId(idProducto);
            if (p == null) throw new NegocioException("El producto no existe.");

            productoDao.ActualizarDescuento(idProducto, porcentaje);

            if (porcentaje > 0)
            {
                Log.Info("Descuento " + porcentaje.ToString("0.##") + "% aplicado a " + p.Nombre + " (N°" + idProducto + ")");
                logService.Registrar(ModuloLog.Productos, "Descuento", p.Nombre + " | " + porcentaje.ToString("0.##") + "%");
            }
            else
            {
                Log.Info("Descuento quitado de " + p.Nombre + " (N°" + idProducto + ")");
                logService.Registrar(ModuloLog.Productos, "Quitar descuento", "Producto: " + p.Nombre);
            }
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public void Desactivar(int idProducto)
        {
            var p = productoDao.ObtenerPorId(idProducto);
            productoDao.Desactivar(idProducto);
            Log.Info("Producto desactivado N°" + idProducto + " (" + (p?.Nombre ?? "?") + ")");
            logService.Registrar(ModuloLog.Productos, "Desactivación", "Producto: " + (p?.Nombre ?? idProducto.ToString()));
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public void Activar(int idProducto)
        {
            var p = productoDao.ObtenerPorId(idProducto);
            productoDao.Activar(idProducto);
            Log.Info("Producto activado N°" + idProducto + " (" + (p?.Nombre ?? "?") + ")");
            logService.Registrar(ModuloLog.Productos, "Activación", "Producto: " + (p?.Nombre ?? idProducto.ToString()));
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public decimal AjustarStock(int idProducto, decimal delta)
        {
            var p = productoDao.ObtenerPorId(idProducto);
            if (p == null) throw new NegocioException("El producto no existe.");
            decimal nuevoStock = p.Stock + delta;
            if (nuevoStock < 0) throw new NegocioException(
                "Stock insuficiente. Stock actual: " + p.Stock.ToString("0.##") + " " + p.UnidadMedida + ".");
            productoDao.ActualizarStock(idProducto, nuevoStock);
            Log.Info("Stock ajustado: " + p.Nombre + " | " + (delta >= 0 ? "+" : "") + delta.ToString("0.##") +
                     " → " + nuevoStock.ToString("0.##"));
            string accion = delta >= 0 ? "Entrada de stock" : "Salida de stock";
            logService.Registrar(ModuloLog.Productos, accion,
                p.Nombre + " | Δ" + (delta >= 0 ? "+" : "") + delta.ToString("0.##") + " → stock: " + nuevoStock.ToString("0.##"));
            NotificadorCambios.Notificar(Entidad.Producto);
            return nuevoStock;
        }

        public void Eliminar(int idProducto)
        {
            if (productoDao.TieneVentas(idProducto)) throw new NegocioException(
                "No se puede eliminar: el producto tiene ventas registradas. Usa Desactivar para conservar el historial.");
            var p = productoDao.ObtenerPorId(idProducto);
            productoDao.Eliminar(idProducto);
            Log.Advertencia("Producto ELIMINADO N°" + idProducto + " (" + (p?.Nombre ?? "?") + ")");
            logService.Registrar(ModuloLog.Productos, "Eliminación", "Producto: " + (p?.Nombre ?? idProducto.ToString()));
            NotificadorCambios.Notificar(Entidad.Producto);
        }

        public List<Producto> ObtenerBajoStock()
        {
            return productoDao.ObtenerBajoStock();   // filtrado en SQL, no materializa todo el catálogo
        }

        public bool EstaBajoStock(Producto p)
        {
            return p.Stock <= p.StockMinimo;
        }
    }
}

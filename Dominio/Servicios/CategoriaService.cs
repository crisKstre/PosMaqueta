using System;
using System.Collections.Generic;
using AccesoData;
using AccesoData.DAO;
using Entidades;

namespace Dominio.Servicios
{
    public class CategoriaService
    {
        private readonly CategoriaDao dao = new CategoriaDao();

        public List<Categoria> ObtenerTodas() => dao.ObtenerTodas();

        public void Agregar(string nombre)
        {
            Autorizacion.ExigirAdmin();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new NegocioException("El nombre no puede estar vacío.");
            if (dao.Existe(nombre))
                throw new NegocioException("Ya existe una categoría con ese nombre.");
            dao.Insertar(nombre);
            Log.Info("Categoría creada: " + nombre);
        }

        public void Eliminar(int idCategoria)
        {
            Autorizacion.ExigirAdmin();
            if (dao.TieneProductos(idCategoria))
                throw new NegocioException("No se puede eliminar: hay productos con esta categoría.");
            dao.Eliminar(idCategoria);
            Log.Info("Categoría eliminada N°" + idCategoria);
        }
    }
}

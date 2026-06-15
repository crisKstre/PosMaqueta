using System;
using System.Collections.Generic;
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
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidOperationException("El nombre no puede estar vacío.");
            if (dao.Existe(nombre))
                throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
            dao.Insertar(nombre);
        }

        public void Eliminar(int idCategoria)
        {
            if (dao.TieneProductos(idCategoria))
                throw new InvalidOperationException("No se puede eliminar: hay productos con esta categoría.");
            dao.Eliminar(idCategoria);
        }
    }
}

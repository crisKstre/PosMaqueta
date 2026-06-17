using System;
using System.Collections.Generic;
using System.Data.Common;
using Entidades;

namespace AccesoData.DAO
{
    public class CategoriaDao : ConexionBD
    {
        public List<Categoria> ObtenerTodas()
        {
            var lista = new List<Categoria>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "SELECT IdCategoria, Nombre FROM Categoria ORDER BY Nombre;";
                using (var cmd = con.Comando(sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        lista.Add(new Categoria { IdCategoria = reader.GetInt32(0), Nombre = reader.GetString(1) });
            }
            return lista;
        }

        public bool Existe(string nombre)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT COUNT(*) FROM Categoria WHERE LOWER(Nombre)=LOWER(@n);"))
                {
                    cmd.AddParam("@n", nombre);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public void Insertar(string nombre)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("INSERT INTO Categoria (Nombre) VALUES (@n);"))
                {
                    cmd.AddParam("@n", nombre.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool TieneProductos(int idCategoria)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(
                    "SELECT COUNT(*) FROM Producto WHERE Categoria = (SELECT Nombre FROM Categoria WHERE IdCategoria=@id);"))
                {
                    cmd.AddParam("@id", idCategoria);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public bool Eliminar(int idCategoria)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("DELETE FROM Categoria WHERE IdCategoria=@id;"))
                {
                    cmd.AddParam("@id", idCategoria);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}

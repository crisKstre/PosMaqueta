using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Entidades;

namespace AccesoData.DAO
{
    public class CategoriaDao : ConexionSqlite
    {
        public List<Categoria> ObtenerTodas()
        {
            var lista = new List<Categoria>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "SELECT IdCategoria, Nombre FROM Categoria ORDER BY Nombre;";
                using (var cmd = new SqliteCommand(sql, con))
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
                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Categoria WHERE LOWER(Nombre)=LOWER(@n);", con))
                {
                    cmd.Parameters.AddWithValue("@n", nombre);
                    return (long)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public void Insertar(string nombre)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = new SqliteCommand("INSERT INTO Categoria (Nombre) VALUES (@n);", con))
                {
                    cmd.Parameters.AddWithValue("@n", nombre.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool TieneProductos(int idCategoria)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Producto WHERE Categoria = (SELECT Nombre FROM Categoria WHERE IdCategoria=@id);", con))
                {
                    cmd.Parameters.AddWithValue("@id", idCategoria);
                    return (long)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public bool Eliminar(int idCategoria)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = new SqliteCommand("DELETE FROM Categoria WHERE IdCategoria=@id;", con))
                {
                    cmd.Parameters.AddWithValue("@id", idCategoria);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}

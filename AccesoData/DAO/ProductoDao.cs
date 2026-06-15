using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Entidades;

namespace AccesoData.DAO
{
    public class ProductoDao : ConexionSqlite
    {
        public List<Producto> ObtenerTodos(bool soloActivos = false)
        {
            var lista = new List<Producto>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdProducto, CodigoBarras, Nombre, Categoria, Precio,
                           Stock, StockMinimo, UnidadMedida, Activo
                    FROM Producto";
                if (soloActivos)
                    sql += " WHERE Activo = 1";
                sql += " ORDER BY Activo DESC, Nombre;";

                using (var cmd = new SqliteCommand(sql, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(Mapear(reader));
                }
            }
            return lista;
        }

        public List<Producto> Buscar(string texto)
        {
            var lista = new List<Producto>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdProducto, CodigoBarras, Nombre, Categoria, Precio,
                           Stock, StockMinimo, UnidadMedida, Activo
                    FROM Producto
                    WHERE (Nombre LIKE @texto OR CodigoBarras LIKE @texto)
                    ORDER BY Activo DESC, Nombre;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@texto", "%" + texto + "%");
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            lista.Add(Mapear(reader));
                }
            }
            return lista;
        }

        public Producto ObtenerPorCodigo(string codigoBarras)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdProducto, CodigoBarras, Nombre, Categoria, Precio,
                           Stock, StockMinimo, UnidadMedida, Activo
                    FROM Producto
                    WHERE CodigoBarras = @codigo AND Activo = 1;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@codigo", codigoBarras);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Mapear(reader);
                        return null;
                    }
                }
            }
        }

        public Producto ObtenerPorId(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdProducto, CodigoBarras, Nombre, Categoria, Precio,
                           Stock, StockMinimo, UnidadMedida, Activo
                    FROM Producto
                    WHERE IdProducto = @id;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idProducto);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Mapear(reader);
                        return null;
                    }
                }
            }
        }

        public List<string> ObtenerCategorias()
        {
            var lista = new List<string>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"SELECT DISTINCT Categoria FROM Producto
                               WHERE Activo = 1 AND Categoria IS NOT NULL AND Categoria <> ''
                               ORDER BY Categoria;";
                using (var cmd = new SqliteCommand(sql, con))
                using (var r   = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(r.GetString(0));
            }
            return lista;
        }

        public List<Producto> ObtenerPorCategoria(string categoria)
        {
            var lista = new List<Producto>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"SELECT IdProducto, CodigoBarras, Nombre, Categoria, Precio,
                                      Stock, StockMinimo, UnidadMedida, Activo
                               FROM Producto
                               WHERE Activo = 1 AND Categoria = @cat
                               ORDER BY Nombre;";
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@cat", categoria);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) lista.Add(Mapear(r));
                }
            }
            return lista;
        }

        public bool ExisteCodigo(string codigoBarras, int idExcluir = 0)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"SELECT COUNT(*) FROM Producto
                               WHERE CodigoBarras = @codigo AND IdProducto <> @id;";
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@codigo", codigoBarras);
                    cmd.Parameters.AddWithValue("@id", idExcluir);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public int Insertar(Producto p)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    INSERT INTO Producto (CodigoBarras, Nombre, Categoria, Precio,
                                          Stock, StockMinimo, UnidadMedida, Activo)
                    VALUES (@codigo, @nombre, @categoria, @precio,
                            @stock, @stockMin, @unidad, 1);
                    SELECT last_insert_rowid();";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    AgregarParametros(cmd, p);
                    long id = (long)cmd.ExecuteScalar();
                    return (int)id;
                }
            }
        }

        public bool Actualizar(Producto p)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    UPDATE Producto SET
                        CodigoBarras = @codigo,
                        Nombre       = @nombre,
                        Categoria    = @categoria,
                        Precio       = @precio,
                        Stock        = @stock,
                        StockMinimo  = @stockMin,
                        UnidadMedida = @unidad
                    WHERE IdProducto = @id;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    AgregarParametros(cmd, p);
                    cmd.Parameters.AddWithValue("@id", p.IdProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Desactivar(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "UPDATE Producto SET Activo = 0 WHERE IdProducto = @id;";
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Activar(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "UPDATE Producto SET Activo = 1 WHERE IdProducto = @id;";
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool ActualizarStock(int idProducto, decimal nuevoStock)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "UPDATE Producto SET Stock = @stock WHERE IdProducto = @id;";
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@stock", nuevoStock);
                    cmd.Parameters.AddWithValue("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool TieneVentas(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "SELECT COUNT(*) FROM DetalleVenta WHERE IdProducto = @id;";
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idProducto);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public bool Eliminar(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "DELETE FROM Producto WHERE IdProducto = @id;";
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private void AgregarParametros(SqliteCommand cmd, Producto p)
        {
            cmd.Parameters.AddWithValue("@codigo",
                string.IsNullOrEmpty(p.CodigoBarras) ? (object)System.DBNull.Value : p.CodigoBarras);
            cmd.Parameters.AddWithValue("@nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@categoria", (object)p.Categoria ?? System.DBNull.Value);
            cmd.Parameters.AddWithValue("@precio", p.Precio);
            cmd.Parameters.AddWithValue("@stock", p.Stock);
            cmd.Parameters.AddWithValue("@stockMin", p.StockMinimo);
            cmd.Parameters.AddWithValue("@unidad", p.UnidadMedida);
        }

        private Producto Mapear(SqliteDataReader reader)
        {
            return new Producto
            {
                IdProducto = reader.GetInt32(0),
                CodigoBarras = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Nombre = reader.GetString(2),
                Categoria = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Precio = reader.GetDecimal(4),
                Stock = reader.GetDecimal(5),
                StockMinimo = reader.GetDecimal(6),
                UnidadMedida = reader.GetString(7),
                Activo = reader.GetInt32(8) == 1
            };
        }
    }
}

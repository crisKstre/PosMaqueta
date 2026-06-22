using System;
using System.Collections.Generic;
using System.Data.Common;
using Entidades;

namespace AccesoData.DAO
{
    public class ProductoDao : ConexionBD
    {
        public List<Producto> ObtenerTodos(bool soloActivos = false)
        {
            var lista = new List<Producto>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdProducto, CodigoBarras, Nombre, Categoria, Precio,
                           Stock, StockMinimo, UnidadMedida, Activo, DescuentoPorcentaje, Costo
                    FROM Producto";
                if (soloActivos)
                    sql += " WHERE Activo = 1";
                sql += " ORDER BY Activo DESC, Nombre;";

                using (var cmd = con.Comando(sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        lista.Add(Mapear(reader));
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
                           Stock, StockMinimo, UnidadMedida, Activo, DescuentoPorcentaje, Costo
                    FROM Producto
                    WHERE (Nombre LIKE @texto OR CodigoBarras LIKE @texto)
                    ORDER BY Activo DESC, Nombre;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@texto", "%" + texto + "%");
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
                           Stock, StockMinimo, UnidadMedida, Activo, DescuentoPorcentaje, Costo
                    FROM Producto
                    WHERE CodigoBarras = @codigo AND Activo = 1;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@codigo", codigoBarras);
                    using (var reader = cmd.ExecuteReader())
                        return reader.Read() ? Mapear(reader) : null;
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
                           Stock, StockMinimo, UnidadMedida, Activo, DescuentoPorcentaje, Costo
                    FROM Producto
                    WHERE IdProducto = @id;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@id", idProducto);
                    using (var reader = cmd.ExecuteReader())
                        return reader.Read() ? Mapear(reader) : null;
                }
            }
        }

        // Primer producto con ese nombre exacto (prefiere activos). Para reimportar productos SIN
        // código de barras sin duplicarlos (5.c del roadmap).
        public Producto ObtenerPorNombre(string nombre)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdProducto, CodigoBarras, Nombre, Categoria, Precio,
                           Stock, StockMinimo, UnidadMedida, Activo, DescuentoPorcentaje, Costo
                    FROM Producto
                    WHERE Nombre = @nombre
                    ORDER BY Activo DESC, IdProducto;";
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@nombre", nombre);
                    using (var reader = cmd.ExecuteReader())
                        return reader.Read() ? Mapear(reader) : null;
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
                using (var cmd = con.Comando(sql))
                using (var r = cmd.ExecuteReader())
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
                                      Stock, StockMinimo, UnidadMedida, Activo, DescuentoPorcentaje, Costo
                               FROM Producto
                               WHERE Activo = 1 AND Categoria = @cat
                               ORDER BY Nombre;";
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@cat", categoria);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) lista.Add(Mapear(r));
                }
            }
            return lista;
        }

        // Productos activos en o bajo su stock mínimo, filtrados en SQL (no trae todo el catálogo).
        public List<Producto> ObtenerBajoStock()
        {
            var lista = new List<Producto>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdProducto, CodigoBarras, Nombre, Categoria, Precio,
                           Stock, StockMinimo, UnidadMedida, Activo, DescuentoPorcentaje, Costo
                    FROM Producto
                    WHERE Activo = 1 AND Stock <= StockMinimo
                    ORDER BY Nombre;";
                using (var cmd = con.Comando(sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        lista.Add(Mapear(reader));
            }
            return lista;
        }

        // Valor del inventario activo valorizado a COSTO (Σ Stock × Costo). Cuánta plata hay "parada".
        public decimal ValorInventarioACosto()
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT COALESCE(SUM(Stock * Costo), 0) FROM Producto WHERE Activo = 1;"))
                    return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public bool ExisteCodigo(string codigoBarras, int idExcluir = 0)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"SELECT COUNT(*) FROM Producto
                               WHERE CodigoBarras = @codigo AND IdProducto <> @id;";
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@codigo", codigoBarras);
                    cmd.AddParam("@id", idExcluir);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
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
                                          Stock, StockMinimo, UnidadMedida, Activo, DescuentoPorcentaje, Costo)
                    VALUES (@codigo, @nombre, @categoria, @precio,
                            @stock, @stockMin, @unidad, 1, @descuento, @costo);
                    " + Dialecto.UltimoId;

                using (var cmd = con.Comando(sql))
                {
                    AgregarParametros(cmd, p);
                    cmd.AddParam("@descuento", p.DescuentoPorcentaje);
                    return Convert.ToInt32(cmd.ExecuteScalar());
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
                        Costo        = @costo,
                        Stock        = @stock,
                        StockMinimo  = @stockMin,
                        UnidadMedida = @unidad
                    WHERE IdProducto = @id;";

                using (var cmd = con.Comando(sql))
                {
                    AgregarParametros(cmd, p);
                    cmd.AddParam("@id", p.IdProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Desactivar(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("UPDATE Producto SET Activo = 0 WHERE IdProducto = @id;"))
                {
                    cmd.AddParam("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Activar(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("UPDATE Producto SET Activo = 1 WHERE IdProducto = @id;"))
                {
                    cmd.AddParam("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool ActualizarStock(int idProducto, decimal nuevoStock)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("UPDATE Producto SET Stock = @stock WHERE IdProducto = @id;"))
                {
                    cmd.AddParam("@stock", nuevoStock);
                    cmd.AddParam("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool ActualizarDescuento(int idProducto, decimal porcentaje)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("UPDATE Producto SET DescuentoPorcentaje = @d WHERE IdProducto = @id;"))
                {
                    cmd.AddParam("@d", porcentaje);
                    cmd.AddParam("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool TieneVentas(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT COUNT(*) FROM DetalleVenta WHERE IdProducto = @id;"))
                {
                    cmd.AddParam("@id", idProducto);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public bool Eliminar(int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("DELETE FROM Producto WHERE IdProducto = @id;"))
                {
                    cmd.AddParam("@id", idProducto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private void AgregarParametros(DbCommand cmd, Producto p)
        {
            cmd.AddParam("@codigo", string.IsNullOrEmpty(p.CodigoBarras) ? null : p.CodigoBarras);
            cmd.AddParam("@nombre", p.Nombre);
            cmd.AddParam("@categoria", p.Categoria);
            cmd.AddParam("@precio", p.Precio);
            cmd.AddParam("@costo", p.Costo);
            cmd.AddParam("@stock", p.Stock);
            cmd.AddParam("@stockMin", p.StockMinimo);
            cmd.AddParam("@unidad", p.UnidadMedida);
        }

        private Producto Mapear(DbDataReader reader)
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
                Activo = reader.GetInt32(8) == 1,
                DescuentoPorcentaje = reader.GetDecimal(9),
                Costo = reader.GetDecimal(10)
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Entidades;

namespace AccesoData.DAO
{
    public class VentaDao : ConexionSqlite
    {
        // Registra la venta completa (cabecera + detalle) y descuenta el stock,
        // todo dentro de una transacción. Devuelve el Id de la venta generada.
        public int RegistrarVenta(Venta venta)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        int idVenta = InsertarCabecera(con, tran, venta);

                        foreach (var d in venta.Detalles)
                        {
                            InsertarDetalle(con, tran, idVenta, d);
                            DescontarStock(con, tran, d.IdProducto, d.Cantidad);
                        }

                        tran.Commit();
                        return idVenta;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private int InsertarCabecera(SqliteConnection con, SqliteTransaction tran, Venta venta)
        {
            string sql = @"
                INSERT INTO Venta (IdCaja, IdUsuario, Fecha, Total, Descuento, MedioPago)
                VALUES (@idCaja, @idUsuario, @fecha, @total, @descuento, @medioPago);
                SELECT last_insert_rowid();";

            using (var cmd = new SqliteCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@idCaja",
                    venta.IdCaja.HasValue ? (object)venta.IdCaja.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
                cmd.Parameters.AddWithValue("@fecha", venta.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@total", venta.Total);
                cmd.Parameters.AddWithValue("@descuento", venta.Descuento);
                cmd.Parameters.AddWithValue("@medioPago", (object)venta.MedioPago ?? DBNull.Value);
                long id = (long)cmd.ExecuteScalar();
                return (int)id;
            }
        }

        private void InsertarDetalle(SqliteConnection con, SqliteTransaction tran, int idVenta, DetalleVenta d)
        {
            string sql = @"
                INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario, PrecioOriginal, DescuentoPorcentaje, Subtotal)
                VALUES (@idVenta, @idProducto, @cantidad, @precio, @precioOrig, @descuento, @subtotal);";

            using (var cmd = new SqliteCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@idVenta", idVenta);
                cmd.Parameters.AddWithValue("@idProducto", d.IdProducto);
                cmd.Parameters.AddWithValue("@cantidad", d.Cantidad);
                cmd.Parameters.AddWithValue("@precio", d.PrecioUnitario);
                cmd.Parameters.AddWithValue("@precioOrig", d.PrecioOriginal > 0 ? d.PrecioOriginal : d.PrecioUnitario);
                cmd.Parameters.AddWithValue("@descuento", d.DescuentoPorcentaje);
                cmd.Parameters.AddWithValue("@subtotal", d.Subtotal);
                cmd.ExecuteNonQuery();
            }
        }

        private void DescontarStock(SqliteConnection con, SqliteTransaction tran, int idProducto, decimal cantidad)
        {
            string sql = "UPDATE Producto SET Stock = Stock - @cantidad WHERE IdProducto = @id;";
            using (var cmd = new SqliteCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                cmd.Parameters.AddWithValue("@id", idProducto);
                cmd.ExecuteNonQuery();
            }
        }

        // Totales de ventas en un rango de fechas (módulo de Reportes)
        public ResumenVentas ObtenerResumen(DateTime desde, DateTime hasta)
        {
            var r = new ResumenVentas();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT
                        COUNT(*),
                        COALESCE(SUM(Total), 0),
                        COALESCE(SUM(CASE WHEN MedioPago = @efectivo      THEN Total ELSE 0 END), 0),
                        COALESCE(SUM(CASE WHEN MedioPago = @tarjeta       THEN Total ELSE 0 END), 0),
                        COALESCE(SUM(CASE WHEN MedioPago = @transferencia THEN Total ELSE 0 END), 0)
                    FROM Venta
                    WHERE Fecha BETWEEN @desde AND @hasta AND Anulada = 0;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.Parameters.AddWithValue("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    cmd.Parameters.AddWithValue("@efectivo", MedioPago.Efectivo);
                    cmd.Parameters.AddWithValue("@tarjeta", MedioPago.Tarjeta);
                    cmd.Parameters.AddWithValue("@transferencia", MedioPago.Transferencia);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            r.CantidadVentas     = reader.GetInt32(0);
                            r.TotalVendido       = reader.GetDecimal(1);
                            r.TotalEfectivo      = reader.GetDecimal(2);
                            r.TotalTarjeta       = reader.GetDecimal(3);
                            r.TotalTransferencia = reader.GetDecimal(4);
                        }
                    }
                }
            }
            return r;
        }

        // Productos más vendidos en un rango (ordenados por cantidad)
        public List<ProductoVendido> ObtenerTopProductos(DateTime desde, DateTime hasta, int top)
        {
            var lista = new List<ProductoVendido>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT p.Nombre, SUM(d.Cantidad), SUM(d.Subtotal)
                    FROM DetalleVenta d
                    JOIN Venta v    ON d.IdVenta    = v.IdVenta
                    JOIN Producto p ON d.IdProducto = p.IdProducto
                    WHERE v.Fecha BETWEEN @desde AND @hasta AND v.Anulada = 0
                    GROUP BY d.IdProducto, p.Nombre
                    ORDER BY SUM(d.Cantidad) DESC
                    LIMIT @top;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.Parameters.AddWithValue("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    cmd.Parameters.AddWithValue("@top", top);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ProductoVendido
                            {
                                Nombre   = reader.GetString(0),
                                Cantidad = reader.GetDecimal(1),
                                Total    = reader.GetDecimal(2)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // Historial de ventas (para futuro módulo de reportes / módulo de caja)
        public List<Venta> ObtenerVentas(DateTime desde, DateTime hasta)
        {
            var lista = new List<Venta>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdVenta, IdCaja, IdUsuario, Fecha, Total, Descuento, MedioPago
                    FROM Venta
                    WHERE Fecha BETWEEN @desde AND @hasta AND Anulada = 0
                    ORDER BY Fecha DESC;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.Parameters.AddWithValue("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Venta
                            {
                                IdVenta = reader.GetInt32(0),
                                IdCaja = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                IdUsuario = reader.GetInt32(2),
                                Fecha = DateTime.Parse(reader.GetString(3)),
                                Total = reader.GetDecimal(4),
                                Descuento = reader.GetDecimal(5),
                                MedioPago = reader.IsDBNull(6) ? "" : reader.GetString(6)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // Detalle (ítems) de una venta, con el código de barras y nombre del producto.
        public List<DetalleVenta> ObtenerDetalleVenta(int idVenta)
        {
            var lista = new List<DetalleVenta>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT d.IdProducto,
                           COALESCE(p.CodigoBarras, ''),
                           COALESCE(p.Nombre, 'Producto #' || d.IdProducto),
                           d.Cantidad, d.PrecioUnitario, d.Subtotal,
                           d.PrecioOriginal, d.DescuentoPorcentaje
                    FROM DetalleVenta d
                    LEFT JOIN Producto p ON d.IdProducto = p.IdProducto
                    WHERE d.IdVenta = @id;";
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idVenta);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            lista.Add(new DetalleVenta
                            {
                                IdProducto     = reader.GetInt32(0),
                                CodigoBarras   = reader.GetString(1),
                                NombreProducto = reader.GetString(2),
                                Cantidad       = reader.GetDecimal(3),
                                PrecioUnitario = reader.GetDecimal(4),
                                Subtotal       = reader.GetDecimal(5),
                                PrecioOriginal = reader.GetDecimal(6),
                                DescuentoPorcentaje = reader.GetDecimal(7)
                            });
                }
            }
            return lista;
        }

        // Anula una venta: devuelve su stock al inventario y la marca Anulada=1, en una transacción.
        public void AnularVenta(int idVenta)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        var detalles = new List<KeyValuePair<int, decimal>>();
                        using (var cmd = new SqliteCommand(
                            "SELECT IdProducto, Cantidad FROM DetalleVenta WHERE IdVenta = @id;", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", idVenta);
                            using (var r = cmd.ExecuteReader())
                                while (r.Read())
                                    detalles.Add(new KeyValuePair<int, decimal>(r.GetInt32(0), r.GetDecimal(1)));
                        }

                        foreach (var d in detalles)
                            using (var cmd = new SqliteCommand(
                                "UPDATE Producto SET Stock = Stock + @c WHERE IdProducto = @p;", con, tran))
                            {
                                cmd.Parameters.AddWithValue("@c", d.Value);
                                cmd.Parameters.AddWithValue("@p", d.Key);
                                cmd.ExecuteNonQuery();
                            }

                        using (var cmd = new SqliteCommand(
                            "UPDATE Venta SET Anulada = 1 WHERE IdVenta = @id;", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", idVenta);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}

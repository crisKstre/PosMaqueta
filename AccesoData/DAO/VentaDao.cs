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
                INSERT INTO Venta (IdCaja, IdUsuario, Fecha, Total, MedioPago)
                VALUES (@idCaja, @idUsuario, @fecha, @total, @medioPago);
                SELECT last_insert_rowid();";

            using (var cmd = new SqliteCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@idCaja",
                    venta.IdCaja.HasValue ? (object)venta.IdCaja.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
                cmd.Parameters.AddWithValue("@fecha", venta.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@total", venta.Total);
                cmd.Parameters.AddWithValue("@medioPago", (object)venta.MedioPago ?? DBNull.Value);
                long id = (long)cmd.ExecuteScalar();
                return (int)id;
            }
        }

        private void InsertarDetalle(SqliteConnection con, SqliteTransaction tran, int idVenta, DetalleVenta d)
        {
            string sql = @"
                INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                VALUES (@idVenta, @idProducto, @cantidad, @precio, @subtotal);";

            using (var cmd = new SqliteCommand(sql, con, tran))
            {
                cmd.Parameters.AddWithValue("@idVenta", idVenta);
                cmd.Parameters.AddWithValue("@idProducto", d.IdProducto);
                cmd.Parameters.AddWithValue("@cantidad", d.Cantidad);
                cmd.Parameters.AddWithValue("@precio", d.PrecioUnitario);
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

        // Historial de ventas (para futuro módulo de reportes / módulo de caja)
        public List<Venta> ObtenerVentas(DateTime desde, DateTime hasta)
        {
            var lista = new List<Venta>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdVenta, IdCaja, IdUsuario, Fecha, Total, MedioPago
                    FROM Venta
                    WHERE Fecha BETWEEN @desde AND @hasta
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
                                MedioPago = reader.IsDBNull(5) ? "" : reader.GetString(5)
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}

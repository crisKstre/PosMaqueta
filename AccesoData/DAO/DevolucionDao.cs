using System;
using Entidades;

namespace AccesoData.DAO
{
    public class DevolucionDao : ConexionBD
    {
        // True si la venta tiene al menos una devolución registrada. Se usa para impedir ANULAR una venta
        // que ya tuvo devoluciones (si no, el stock se reintegraría dos veces: por la devolución y por la anulación).
        public bool TieneDevoluciones(int idVenta)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT COUNT(*) FROM Devolucion WHERE IdVenta = @idVenta;"))
                {
                    cmd.AddParam("@idVenta", idVenta);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // Cantidad ya devuelta de un producto en una venta (para no devolver más de lo vendido).
        public decimal CantidadDevuelta(int idVenta, int idProducto)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(@"
                    SELECT COALESCE(SUM(di.Cantidad), 0)
                    FROM DevolucionItem di JOIN Devolucion d ON di.IdDevolucion = d.IdDevolucion
                    WHERE d.IdVenta = @idVenta AND di.IdProducto = @idProducto;"))
                {
                    cmd.AddParam("@idVenta", idVenta);
                    cmd.AddParam("@idProducto", idProducto);
                    return Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
        }

        // Registra la devolución (cabecera + ítems) y REINTEGRA el stock, en una transacción.
        public int Registrar(Devolucion dev)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        int idDev;
                        using (var cmd = con.Comando(@"
                            INSERT INTO Devolucion (IdVenta, IdCaja, Fecha, IdUsuario, Monto)
                            VALUES (@idVenta, @idCaja, @fecha, @idUsuario, @monto);
                            " + Dialecto.UltimoId, tran))
                        {
                            cmd.AddParam("@idVenta", dev.IdVenta);
                            cmd.AddParam("@idCaja", dev.IdCaja);
                            cmd.AddParam("@fecha", dev.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.AddParam("@idUsuario", dev.IdUsuario);
                            cmd.AddParam("@monto", dev.Monto);
                            idDev = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (var item in dev.Detalles)
                        {
                            using (var cmd = con.Comando(@"
                                INSERT INTO DevolucionItem (IdDevolucion, IdProducto, Cantidad, Subtotal)
                                VALUES (@idDev, @idProd, @cant, @sub);", tran))
                            {
                                cmd.AddParam("@idDev", idDev);
                                cmd.AddParam("@idProd", item.IdProducto);
                                cmd.AddParam("@cant", item.Cantidad);
                                cmd.AddParam("@sub", item.Subtotal);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd = con.Comando("UPDATE Producto SET Stock = Stock + @c WHERE IdProducto = @p;", tran))
                            {
                                cmd.AddParam("@c", item.Cantidad);
                                cmd.AddParam("@p", item.IdProducto);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        return idDev;
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

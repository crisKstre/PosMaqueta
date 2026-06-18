using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Entidades;

namespace AccesoData.DAO
{
    public class VentaDao : ConexionBD
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

                        foreach (var pago in venta.Pagos)
                            InsertarPago(con, tran, idVenta, pago);

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

        private int InsertarCabecera(DbConnection con, DbTransaction tran, Venta venta)
        {
            string sql = @"
                INSERT INTO Venta (IdCaja, IdUsuario, Fecha, Total, Descuento, MedioPago)
                VALUES (@idCaja, @idUsuario, @fecha, @total, @descuento, @medioPago);
                " + Dialecto.UltimoId;

            using (var cmd = con.Comando(sql, tran))
            {
                cmd.AddParam("@idCaja", venta.IdCaja);            // null -> DBNull
                cmd.AddParam("@idUsuario", venta.IdUsuario);
                cmd.AddParam("@fecha", venta.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.AddParam("@total", venta.Total);
                cmd.AddParam("@descuento", venta.Descuento);
                cmd.AddParam("@medioPago", venta.MedioPago);      // null -> DBNull
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void InsertarDetalle(DbConnection con, DbTransaction tran, int idVenta, DetalleVenta d)
        {
            string sql = @"
                INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario, PrecioOriginal, DescuentoPorcentaje, Subtotal)
                VALUES (@idVenta, @idProducto, @cantidad, @precio, @precioOrig, @descuento, @subtotal);";

            using (var cmd = con.Comando(sql, tran))
            {
                cmd.AddParam("@idVenta", idVenta);
                cmd.AddParam("@idProducto", d.IdProducto);
                cmd.AddParam("@cantidad", d.Cantidad);
                cmd.AddParam("@precio", d.PrecioUnitario);
                cmd.AddParam("@precioOrig", d.PrecioOriginal > 0 ? d.PrecioOriginal : d.PrecioUnitario);
                cmd.AddParam("@descuento", d.DescuentoPorcentaje);
                cmd.AddParam("@subtotal", d.Subtotal);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertarPago(DbConnection con, DbTransaction tran, int idVenta, PagoVenta pago)
        {
            using (var cmd = con.Comando(
                "INSERT INTO PagoVenta (IdVenta, MedioPago, Monto) VALUES (@idVenta, @medio, @monto);", tran))
            {
                cmd.AddParam("@idVenta", idVenta);
                cmd.AddParam("@medio", pago.MedioPago);
                cmd.AddParam("@monto", pago.Monto);
                cmd.ExecuteNonQuery();
            }
        }

        private void DescontarStock(DbConnection con, DbTransaction tran, int idProducto, decimal cantidad)
        {
            // Descuento ATÓMICO: solo si hay stock suficiente. Si no afecta exactamente 1 fila,
            // se aborta la venta (la transacción hace rollback) para no dejar stock negativo (defensa TOCTOU).
            string sql = "UPDATE Producto SET Stock = Stock - @cantidad WHERE IdProducto = @id AND Stock >= @cantidad;";
            using (var cmd = con.Comando(sql, tran))
            {
                cmd.AddParam("@cantidad", cantidad);
                cmd.AddParam("@id", idProducto);
                if (cmd.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("Stock insuficiente para el producto N°" + idProducto + " al registrar la venta.");
            }
        }

        // Totales de ventas en un rango de fechas (módulo de Reportes)
        public ResumenVentas ObtenerResumen(DateTime desde, DateTime hasta)
        {
            var r = new ResumenVentas();
            using (var con = GetConnection())
            {
                con.Open();
                string d = desde.ToString("yyyy-MM-dd 00:00:00");
                string h = hasta.ToString("yyyy-MM-dd 23:59:59");

                using (var cmd = con.Comando(
                    "SELECT COUNT(*), COALESCE(SUM(Total), 0) FROM Venta WHERE Fecha BETWEEN @desde AND @hasta AND Anulada = 0;"))
                {
                    cmd.AddParam("@desde", d);
                    cmd.AddParam("@hasta", h);
                    using (var reader = cmd.ExecuteReader())
                        if (reader.Read()) { r.CantidadVentas = reader.GetInt32(0); r.TotalVendido = reader.GetDecimal(1); }
                }

                // Desglose por medio de pago desde PagoVenta (soporta pago mixto).
                using (var cmd = con.Comando(@"
                    SELECT pv.MedioPago, COALESCE(SUM(pv.Monto), 0)
                    FROM PagoVenta pv JOIN Venta v ON pv.IdVenta = v.IdVenta
                    WHERE v.Fecha BETWEEN @desde AND @hasta AND v.Anulada = 0
                    GROUP BY pv.MedioPago;"))
                {
                    cmd.AddParam("@desde", d);
                    cmd.AddParam("@hasta", h);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            decimal monto = reader.GetDecimal(1);
                            switch (reader.GetString(0))
                            {
                                case MedioPago.Efectivo:      r.TotalEfectivo      = monto; break;
                                case MedioPago.Tarjeta:       r.TotalTarjeta       = monto; break;
                                case MedioPago.Transferencia: r.TotalTransferencia = monto; break;
                            }
                        }
                }

                // Devoluciones del período: el reporte debe restarlas (igual que el arqueo de caja),
                // si no, TotalVendido/desglose quedan sobreestimados respecto del efectivo real.
                using (var cmd = con.Comando(
                    "SELECT COALESCE(SUM(Monto), 0) FROM Devolucion WHERE Fecha BETWEEN @desde AND @hasta;"))
                {
                    cmd.AddParam("@desde", d);
                    cmd.AddParam("@hasta", h);
                    r.TotalDevoluciones = Convert.ToDecimal(cmd.ExecuteScalar());
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
                string seleccion = Dialecto.EsSqlServer ? "SELECT TOP (@top) " : "SELECT ";
                string limite    = Dialecto.EsSqlServer ? "" : " LIMIT @top";
                string sql = seleccion + @"p.Nombre, SUM(d.Cantidad), SUM(d.Subtotal)
                    FROM DetalleVenta d
                    JOIN Venta v    ON d.IdVenta    = v.IdVenta
                    JOIN Producto p ON d.IdProducto = p.IdProducto
                    WHERE v.Fecha BETWEEN @desde AND @hasta AND v.Anulada = 0
                    GROUP BY d.IdProducto, p.Nombre
                    ORDER BY SUM(d.Cantidad) DESC" + limite + ";";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.AddParam("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    cmd.AddParam("@top", top);
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

        // Historial de ventas (para reportes / módulo de caja)
        public List<Venta> ObtenerVentas(DateTime desde, DateTime hasta)
        {
            var lista = new List<Venta>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT v.IdVenta, v.IdCaja, v.IdUsuario, v.Fecha, v.Total, v.Descuento, v.MedioPago,
                           COALESCE(u.Nombre, '')
                    FROM Venta v
                    LEFT JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                    WHERE v.Fecha BETWEEN @desde AND @hasta AND v.Anulada = 0
                    ORDER BY v.Fecha DESC;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.AddParam("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Venta
                            {
                                IdVenta = reader.GetInt32(0),
                                IdCaja = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                IdUsuario = reader.GetInt32(2),
                                Fecha = Persistencia.LeerFecha(reader.GetString(3)),
                                Total = reader.GetDecimal(4),
                                Descuento = reader.GetDecimal(5),
                                MedioPago = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                NombreUsuario = reader.GetString(7)
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
                           p.Nombre,
                           d.Cantidad, d.PrecioUnitario, d.Subtotal,
                           d.PrecioOriginal, d.DescuentoPorcentaje
                    FROM DetalleVenta d
                    LEFT JOIN Producto p ON d.IdProducto = p.IdProducto
                    WHERE d.IdVenta = @id;";
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@id", idVenta);
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            int idProd = reader.GetInt32(0);
                            lista.Add(new DetalleVenta
                            {
                                IdProducto     = idProd,
                                CodigoBarras   = reader.GetString(1),
                                NombreProducto = reader.IsDBNull(2) ? "Producto #" + idProd : reader.GetString(2),
                                Cantidad       = reader.GetDecimal(3),
                                PrecioUnitario = reader.GetDecimal(4),
                                Subtotal       = reader.GetDecimal(5),
                                PrecioOriginal = reader.GetDecimal(6),
                                DescuentoPorcentaje = reader.GetDecimal(7)
                            });
                        }
                }
            }
            return lista;
        }

        // Trae Stock/Nombre/UnidadMedida de varios productos en UNA sola consulta
        // (para re-validar el carrito al cobrar sin un round-trip por ítem).
        public Dictionary<int, Producto> ObtenerStocks(IEnumerable<int> idsProducto)
        {
            var ids = idsProducto.Distinct().ToList();
            var mapa = new Dictionary<int, Producto>();
            if (ids.Count == 0) return mapa;

            using (var con = GetConnection())
            {
                con.Open();
                string param = string.Join(",", ids.Select((id, i) => "@p" + i));
                string sql = "SELECT IdProducto, Nombre, Stock, UnidadMedida FROM Producto WHERE IdProducto IN (" + param + ");";
                using (var cmd = con.Comando(sql))
                {
                    for (int i = 0; i < ids.Count; i++) cmd.AddParam("@p" + i, ids[i]);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            mapa[r.GetInt32(0)] = new Producto
                            {
                                IdProducto   = r.GetInt32(0),
                                Nombre       = r.GetString(1),
                                Stock        = r.GetDecimal(2),
                                UnidadMedida = r.GetString(3),
                            };
                }
            }
            return mapa;
        }

        // True si la venta existe y está anulada. Permite al servicio impedir devolver una venta ya
        // revertida (que reintegraría stock y sacaría efectivo de una venta que nunca contó como ingreso).
        public bool EstaAnulada(int idVenta)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT Anulada FROM Venta WHERE IdVenta = @id;"))
                {
                    cmd.AddParam("@id", idVenta);
                    var r = cmd.ExecuteScalar();
                    return r != null && r != DBNull.Value && Convert.ToInt32(r) != 0;
                }
            }
        }

        // Anula una venta: devuelve su stock al inventario y la marca Anulada=1, en una transacción.
        // IDEMPOTENTE: si la venta ya estaba anulada (o no existe) devuelve false sin tocar el stock.
        public bool AnularVenta(int idVenta)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        // Marca anulada SOLO si aún no lo estaba: así el stock se devuelve una única vez.
                        int filas;
                        using (var cmd = con.Comando("UPDATE Venta SET Anulada = 1 WHERE IdVenta = @id AND Anulada = 0;", tran))
                        {
                            cmd.AddParam("@id", idVenta);
                            filas = cmd.ExecuteNonQuery();
                        }
                        if (filas == 0) { tran.Rollback(); return false; }   // ya estaba anulada o no existe

                        var detalles = new List<KeyValuePair<int, decimal>>();
                        using (var cmd = con.Comando("SELECT IdProducto, Cantidad FROM DetalleVenta WHERE IdVenta = @id;", tran))
                        {
                            cmd.AddParam("@id", idVenta);
                            using (var r = cmd.ExecuteReader())
                                while (r.Read())
                                    detalles.Add(new KeyValuePair<int, decimal>(r.GetInt32(0), r.GetDecimal(1)));
                        }

                        foreach (var d in detalles)
                            using (var cmd = con.Comando("UPDATE Producto SET Stock = Stock + @c WHERE IdProducto = @p;", tran))
                            {
                                cmd.AddParam("@c", d.Value);
                                cmd.AddParam("@p", d.Key);
                                cmd.ExecuteNonQuery();
                            }

                        tran.Commit();
                        return true;
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

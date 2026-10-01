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

                        RegistrarAuditoria(con, tran, idVenta, venta);
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

        private void RegistrarAuditoria(DbConnection con, DbTransaction tran, int idVenta, Venta venta)
        {
            string detalle = "N°" + idVenta + " | $" + venta.Total.ToString("N0") + " | " + venta.MedioPago;
            if (venta.Descuento > 0) detalle += " | desc. $" + venta.Descuento.ToString("N0");
            // El actor es el de la venta, no una sesión global que pueda cambiar. La auditoría
            // participa del mismo commit que cabecera, detalles, pagos y stock.
            using (var cmd = con.Comando(@"
                INSERT INTO LogMovimiento (Fecha, IdUsuario, NombreUsuario, Modulo, Accion, Detalle)
                SELECT @fecha, IdUsuario, Nombre, @modulo, 'Venta', @detalle
                FROM Usuario WHERE IdUsuario = @usuario;", tran))
            {
                cmd.AddParam("@fecha", venta.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.AddParam("@usuario", venta.IdUsuario);
                cmd.AddParam("@modulo", ModuloLog.Ventas);
                cmd.AddParam("@detalle", detalle);
                if (cmd.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("No se pudo registrar la auditoría de la venta.");
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
                INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario, PrecioOriginal, DescuentoPorcentaje, CostoUnitario, Subtotal)
                VALUES (@idVenta, @idProducto, @cantidad, @precio, @precioOrig, @descuento, @costo, @subtotal);";

            using (var cmd = con.Comando(sql, tran))
            {
                cmd.AddParam("@idVenta", idVenta);
                cmd.AddParam("@idProducto", d.IdProducto);
                cmd.AddParam("@cantidad", d.Cantidad);
                cmd.AddParam("@precio", d.PrecioUnitario);
                cmd.AddParam("@precioOrig", d.PrecioOriginal > 0 ? d.PrecioOriginal : d.PrecioUnitario);
                cmd.AddParam("@descuento", d.DescuentoPorcentaje);
                cmd.AddParam("@costo", d.CostoUnitario);
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

                // Devoluciones por fecha del reembolso, aunque la venta sea de otro período.
                // TotalVendido y el desglose conservan el cobro original; TotalNeto resta el reembolso.
                using (var cmd = con.Comando(
                    "SELECT COALESCE(SUM(Monto), 0) FROM Devolucion WHERE Fecha BETWEEN @desde AND @hasta;"))
                {
                    cmd.AddParam("@desde", d);
                    cmd.AddParam("@hasta", h);
                    r.TotalDevoluciones = Convert.ToDecimal(cmd.ExecuteScalar());
                }

            }
            // Misma proyección de costos que los rankings, sin otra suma en coma flotante de SQLite.
            var productos = ObtenerProductosPeriodo(desde, hasta);
            r.TotalCostoDevuelto = productos.Sum(p => p.CostoDevuelto);
            r.TotalCosto = productos.Sum(p => p.Costo + p.CostoDevuelto);
            return r;
        }

        // Ordenar y limitar DESPUÉS de aplicar descuentos y devoluciones. Los empates usan el ID.
        public List<ProductoVendido> ObtenerTopProductos(DateTime desde, DateTime hasta, int top)
            => ObtenerProductosPeriodo(desde, hasta).OrderByDescending(p => p.Cantidad)
                .ThenBy(p => p.IdProducto).Take(Math.Max(0, top)).ToList();

        public List<ProductoVendido> ObtenerTopUtilidad(DateTime desde, DateTime hasta, int top)
            => ObtenerProductosPeriodo(desde, hasta).OrderByDescending(p => p.Utilidad)
                .ThenBy(p => p.IdProducto).Take(Math.Max(0, top)).ToList();

        // Dos consultas por período, sin una consulta por venta/producto. El reparto se hace en
        // decimal con la misma regla que las devoluciones; no se recalculan reembolsos históricos.
        private List<ProductoVendido> ObtenerProductosPeriodo(DateTime desde, DateTime hasta)
        {
            var productos = new Dictionary<int, ProductoVendido>();
            var ventas = new Dictionary<int, Venta>();
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(@"
                    SELECT v.IdVenta, v.Descuento, d.IdProducto, p.Nombre,
                           d.Cantidad, d.Subtotal, d.CostoUnitario
                    FROM Venta v JOIN DetalleVenta d ON d.IdVenta = v.IdVenta
                    LEFT JOIN Producto p ON p.IdProducto = d.IdProducto
                    WHERE v.Fecha BETWEEN @desde AND @hasta AND v.Anulada = 0;"))
                {
                    cmd.AddParam("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.AddParam("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            int id = r.GetInt32(0);
                            if (!ventas.TryGetValue(id, out var venta))
                                ventas[id] = venta = new Venta { Descuento = r.GetDecimal(1) };
                            venta.Detalles.Add(new DetalleVenta { IdProducto = r.GetInt32(2),
                                NombreProducto = r.IsDBNull(3) ? "Producto #" + r.GetInt32(2) : r.GetString(3),
                                Cantidad = r.GetDecimal(4), Subtotal = r.GetDecimal(5), CostoUnitario = r.GetDecimal(6) });
                        }
                }
                foreach (var venta in ventas.Values)
                {
                    var reparto = RepartoVenta.PorProducto(venta.Detalles, venta.Descuento);
                    foreach (var grupo in venta.Detalles.GroupBy(d => d.IdProducto))
                    {
                        var p = ProductoReporte(productos, grupo.Key, grupo.First().NombreProducto);
                        p.Cantidad += grupo.Sum(d => d.Cantidad);
                        p.Total += reparto[grupo.Key];
                        p.Costo += grupo.Sum(d => d.CostoUnitario * d.Cantidad);
                    }
                }

                // La cantidad/costo originales se agrupan antes del JOIN: no duplicar devoluciones
                // cuando el histórico tiene más de una línea del mismo producto en una venta.
                using (var cmd = con.Comando(@"
                    SELECT di.IdProducto, p.Nombre, o.Cantidad, o.Costo,
                           SUM(CASE WHEN d.Fecha < @desde THEN di.Cantidad ELSE 0 END),
                           SUM(CASE WHEN d.Fecha BETWEEN @desde AND @hasta THEN di.Cantidad ELSE 0 END),
                           SUM(CASE WHEN d.Fecha BETWEEN @desde AND @hasta THEN di.Subtotal ELSE 0 END)
                    FROM Devolucion d JOIN DevolucionItem di ON di.IdDevolucion = d.IdDevolucion
                    LEFT JOIN (
                        SELECT IdVenta, IdProducto, SUM(Cantidad) AS Cantidad,
                               SUM(CostoUnitario * Cantidad) AS Costo
                        FROM DetalleVenta GROUP BY IdVenta, IdProducto
                    ) o ON o.IdVenta = d.IdVenta AND o.IdProducto = di.IdProducto
                    LEFT JOIN Producto p ON p.IdProducto = di.IdProducto
                    WHERE d.Fecha <= @hasta
                    GROUP BY d.IdVenta, di.IdProducto, p.Nombre, o.Cantidad, o.Costo
                    HAVING SUM(CASE WHEN d.Fecha BETWEEN @desde AND @hasta THEN 1 ELSE 0 END) > 0;"))
                {
                    cmd.AddParam("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.AddParam("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            if (r.IsDBNull(2) || r.GetDecimal(2) <= 0)
                                throw new InvalidOperationException("Una devolución sin detalle de venta válido requiere conciliación.");
                            decimal vendida = Convert.ToDecimal(r.GetValue(2));
                            decimal costo = Convert.ToDecimal(r.GetValue(3));
                            decimal anterior = Convert.ToDecimal(r.GetValue(4));
                            decimal cantidad = Convert.ToDecimal(r.GetValue(5));
                            if (costo < 0 || anterior < 0 || cantidad < 0 || anterior + cantidad > vendida)
                                throw new InvalidOperationException("El costo o las cantidades devueltas requieren conciliación.");
                            // Diferencia de acumulados: conserva el costo total incluso si el promedio
                            // ponderado de líneas históricas tiene decimales periódicos.
                            decimal reintegrado = CostoAcumulado(costo, vendida, anterior + cantidad)
                                - CostoAcumulado(costo, vendida, anterior);
                            int id = r.GetInt32(0);
                            var p = ProductoReporte(productos, id, r.IsDBNull(1) ? "Producto #" + id : r.GetString(1));
                            p.Cantidad -= cantidad;
                            p.Total -= Convert.ToDecimal(r.GetValue(6));
                            p.Costo -= reintegrado;
                            p.CostoDevuelto += reintegrado;
                        }
                }
            }
            return productos.Values.ToList();
        }

        private static decimal CostoAcumulado(decimal costo, decimal vendida, decimal devuelta)
            => vendida == devuelta ? costo : costo * devuelta / vendida;

        private static ProductoVendido ProductoReporte(Dictionary<int, ProductoVendido> productos, int id, string nombre)
        {
            if (!productos.TryGetValue(id, out var p))
                productos[id] = p = new ProductoVendido { IdProducto = id, Nombre = nombre };
            return p;
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

        // Importes originales necesarios para repartir descuentos en devoluciones.
        public Venta ObtenerParaDevolucion(int idVenta)
        {
            Venta venta;
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT Total, Descuento FROM Venta WHERE IdVenta = @id;"))
                {
                    cmd.AddParam("@id", idVenta);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;
                        venta = new Venta { IdVenta = idVenta, Total = r.GetDecimal(0), Descuento = r.GetDecimal(1) };
                    }
                }
                using (var cmd = con.Comando("SELECT MedioPago, Monto FROM PagoVenta WHERE IdVenta = @id;"))
                {
                    cmd.AddParam("@id", idVenta);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) venta.Pagos.Add(new PagoVenta { MedioPago = r.GetString(0), Monto = r.GetDecimal(1) });
                }
            }
            venta.Detalles = ObtenerDetalleVenta(idVenta);
            return venta;
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
                           d.PrecioOriginal, d.DescuentoPorcentaje, d.CostoUnitario
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
                                DescuentoPorcentaje = reader.GetDecimal(7),
                                CostoUnitario  = reader.GetDecimal(8)
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

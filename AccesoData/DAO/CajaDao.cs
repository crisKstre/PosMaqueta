using System;
using System.Collections.Generic;
using System.Data.Common;
using Entidades;

namespace AccesoData.DAO
{
    public class CajaDao : ConexionBD
    {
        // Devuelve la caja abierta actual, o null si no hay ninguna.
        public Caja ObtenerCajaAbierta()
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = Dialecto.EsSqlServer
                    ? @"SELECT TOP (1) IdCaja, IdUsuario, FechaApertura, FechaCierre,
                               MontoInicial, MontoEsperado, MontoReal, Estado
                        FROM Caja WHERE Estado = @estado ORDER BY IdCaja DESC;"
                    : @"SELECT IdCaja, IdUsuario, FechaApertura, FechaCierre,
                               MontoInicial, MontoEsperado, MontoReal, Estado
                        FROM Caja WHERE Estado = @estado ORDER BY IdCaja DESC LIMIT 1;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@estado", EstadoCaja.Abierta);
                    using (var reader = cmd.ExecuteReader())
                        return reader.Read() ? Mapear(reader) : null;
                }
            }
        }

        public int AbrirCaja(Caja caja)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    INSERT INTO Caja (IdUsuario, FechaApertura, MontoInicial, MontoEsperado, MontoReal, Estado)
                    VALUES (@idUsuario, @fechaApertura, @montoInicial, 0, 0, @estado);
                    " + Dialecto.UltimoId;
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@idUsuario", caja.IdUsuario);
                    cmd.AddParam("@fechaApertura", caja.FechaApertura.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.AddParam("@montoInicial", caja.MontoInicial);
                    cmd.AddParam("@estado", EstadoCaja.Abierta);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool CerrarCaja(int idCaja, decimal montoEsperado, decimal montoReal)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    UPDATE Caja SET
                        FechaCierre   = @fechaCierre,
                        MontoEsperado = @montoEsperado,
                        MontoReal     = @montoReal,
                        Estado        = @estado
                    WHERE IdCaja = @id;";
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@fechaCierre", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.AddParam("@montoEsperado", montoEsperado);
                    cmd.AddParam("@montoReal", montoReal);
                    cmd.AddParam("@estado", EstadoCaja.Cerrada);
                    cmd.AddParam("@id", idCaja);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Resumen de las ventas asociadas a un turno de caja.
        public ResumenCaja ObtenerResumen(int idCaja)
        {
            var r = new ResumenCaja();
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(
                    "SELECT COUNT(*), COALESCE(SUM(Total), 0) FROM Venta WHERE IdCaja = @idCaja AND Anulada = 0;"))
                {
                    cmd.AddParam("@idCaja", idCaja);
                    using (var reader = cmd.ExecuteReader())
                        if (reader.Read()) { r.CantidadVentas = reader.GetInt32(0); r.TotalVendido = reader.GetDecimal(1); }
                }

                // Desglose por medio de pago desde PagoVenta (soporta pago mixto en el arqueo).
                using (var cmd = con.Comando(@"
                    SELECT pv.MedioPago, COALESCE(SUM(pv.Monto), 0)
                    FROM PagoVenta pv JOIN Venta v ON pv.IdVenta = v.IdVenta
                    WHERE v.IdCaja = @idCaja AND v.Anulada = 0
                    GROUP BY pv.MedioPago;"))
                {
                    cmd.AddParam("@idCaja", idCaja);
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

                // Devoluciones del turno: el efectivo reembolsado sale del cajón (4.B).
                using (var cmd = con.Comando("SELECT COALESCE(SUM(Monto), 0) FROM Devolucion WHERE IdCaja = @idCaja;"))
                {
                    cmd.AddParam("@idCaja", idCaja);
                    r.TotalDevoluciones = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            return r;
        }

        // Todos los turnos de caja (para el histórico), con el nombre del usuario que la operó.
        public List<Caja> ObtenerHistorial(DateTime desde, DateTime hasta)
        {
            var lista = new List<Caja>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT c.IdCaja, c.IdUsuario, c.FechaApertura, c.FechaCierre,
                           c.MontoInicial, c.MontoEsperado, c.MontoReal, c.Estado,
                           COALESCE(u.Nombre, '') AS NombreUsuario
                    FROM Caja c
                    LEFT JOIN Usuario u ON c.IdUsuario = u.IdUsuario
                    WHERE c.FechaApertura BETWEEN @desde AND @hasta
                    ORDER BY c.IdCaja DESC;";
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.AddParam("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            var caja = Mapear(reader);
                            caja.NombreUsuario = reader.GetString(8);
                            lista.Add(caja);
                        }
                }
            }
            return lista;
        }

        private Caja Mapear(DbDataReader reader)
        {
            return new Caja
            {
                IdCaja = reader.GetInt32(0),
                IdUsuario = reader.GetInt32(1),
                FechaApertura = Persistencia.LeerFecha(reader.GetString(2)),
                FechaCierre = reader.IsDBNull(3) ? (DateTime?)null : Persistencia.LeerFecha(reader.GetString(3)),
                MontoInicial = reader.GetDecimal(4),
                MontoEsperado = reader.GetDecimal(5),
                MontoReal = reader.GetDecimal(6),
                Estado = reader.GetString(7)
            };
        }
    }
}

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
                string sql = @"
                    SELECT
                        COUNT(*),
                        COALESCE(SUM(Total), 0),
                        COALESCE(SUM(CASE WHEN MedioPago = @efectivo      THEN Total ELSE 0 END), 0),
                        COALESCE(SUM(CASE WHEN MedioPago = @tarjeta       THEN Total ELSE 0 END), 0),
                        COALESCE(SUM(CASE WHEN MedioPago = @transferencia THEN Total ELSE 0 END), 0)
                    FROM Venta
                    WHERE IdCaja = @idCaja AND Anulada = 0;";
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@idCaja", idCaja);
                    cmd.AddParam("@efectivo", MedioPago.Efectivo);
                    cmd.AddParam("@tarjeta", MedioPago.Tarjeta);
                    cmd.AddParam("@transferencia", MedioPago.Transferencia);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            r.CantidadVentas = reader.GetInt32(0);
                            r.TotalVendido = reader.GetDecimal(1);
                            r.TotalEfectivo = reader.GetDecimal(2);
                            r.TotalTarjeta = reader.GetDecimal(3);
                            r.TotalTransferencia = reader.GetDecimal(4);
                        }
                    }
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
                FechaApertura = DateTime.Parse(reader.GetString(2)),
                FechaCierre = reader.IsDBNull(3) ? (DateTime?)null : DateTime.Parse(reader.GetString(3)),
                MontoInicial = reader.GetDecimal(4),
                MontoEsperado = reader.GetDecimal(5),
                MontoReal = reader.GetDecimal(6),
                Estado = reader.GetString(7)
            };
        }
    }
}

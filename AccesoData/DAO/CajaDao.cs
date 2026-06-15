using System;
using Microsoft.Data.Sqlite;
using Entidades;

namespace AccesoData.DAO
{
    public class CajaDao : ConexionSqlite
    {
        // Devuelve la caja abierta actual, o null si no hay ninguna.
        public Caja ObtenerCajaAbierta()
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdCaja, IdUsuario, FechaApertura, FechaCierre,
                           MontoInicial, MontoEsperado, MontoReal, Estado
                    FROM Caja
                    WHERE Estado = @estado
                    ORDER BY IdCaja DESC
                    LIMIT 1;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@estado", EstadoCaja.Abierta);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return Mapear(reader);
                        return null;
                    }
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
                    SELECT last_insert_rowid();";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@idUsuario", caja.IdUsuario);
                    cmd.Parameters.AddWithValue("@fechaApertura", caja.FechaApertura.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@montoInicial", caja.MontoInicial);
                    cmd.Parameters.AddWithValue("@estado", EstadoCaja.Abierta);
                    long id = (long)cmd.ExecuteScalar();
                    return (int)id;
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

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fechaCierre", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@montoEsperado", montoEsperado);
                    cmd.Parameters.AddWithValue("@montoReal", montoReal);
                    cmd.Parameters.AddWithValue("@estado", EstadoCaja.Cerrada);
                    cmd.Parameters.AddWithValue("@id", idCaja);
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
                        COUNT(*) AS Cantidad,
                        COALESCE(SUM(Total), 0) AS Total,
                        COALESCE(SUM(CASE WHEN MedioPago = @efectivo      THEN Total ELSE 0 END), 0) AS Efectivo,
                        COALESCE(SUM(CASE WHEN MedioPago = @tarjeta       THEN Total ELSE 0 END), 0) AS Tarjeta,
                        COALESCE(SUM(CASE WHEN MedioPago = @transferencia THEN Total ELSE 0 END), 0) AS Transferencia
                    FROM Venta
                    WHERE IdCaja = @idCaja AND Anulada = 0;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@idCaja", idCaja);
                    cmd.Parameters.AddWithValue("@efectivo", MedioPago.Efectivo);
                    cmd.Parameters.AddWithValue("@tarjeta", MedioPago.Tarjeta);
                    cmd.Parameters.AddWithValue("@transferencia", MedioPago.Transferencia);

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
        public System.Collections.Generic.List<Caja> ObtenerHistorial(DateTime desde, DateTime hasta)
        {
            var lista = new System.Collections.Generic.List<Caja>();
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
                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.Parameters.AddWithValue("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
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

        private Caja Mapear(SqliteDataReader reader)
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

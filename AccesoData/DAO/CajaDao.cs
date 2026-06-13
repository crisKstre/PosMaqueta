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
                    WHERE IdCaja = @idCaja;";

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

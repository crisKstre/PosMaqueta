using System;
using System.Collections.Generic;
using System.Data.Common;
using Entidades;

namespace AccesoData.DAO
{
    public sealed class MovimientoCajaDao : ConexionBD
    {
        private const string Seleccion = @"SELECT m.*, u.Nombre AS Operador, a.Nombre AS Anulador
            FROM MovimientoCaja m JOIN Usuario u ON u.IdUsuario=m.IdUsuario
            LEFT JOIN Usuario a ON a.IdUsuario=m.IdUsuarioAnulacion ";

        public List<MovimientoCaja> Obtener(DateTime desde, DateTime hasta)
        {
            var lista = new List<MovimientoCaja>();
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(Seleccion + "WHERE m.Fecha >= @desde AND m.Fecha < @hasta ORDER BY m.IdMovimiento DESC;"))
                {
                    cmd.AddParam("@desde", desde.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.AddParam("@hasta", hasta.Date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
                    using (var r = cmd.ExecuteReader()) while (r.Read()) lista.Add(Mapear(r));
                }
            }
            return lista;
        }

        public int Registrar(MovimientoCaja m, LogMovimiento log)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    if (m.IdCaja.HasValue) ExigirCajaAbierta(con, tx, m.IdCaja.Value);
                    using (var cmd = con.Comando(@"INSERT INTO MovimientoCaja
                        (IdCaja, Fecha, IdUsuario, Tipo, Categoria, Descripcion, Monto, MedioPago, Origen, Referencia, Anulado)
                        VALUES (@caja,@fecha,@usuario,@tipo,@categoria,@descripcion,@monto,@medio,@origen,@referencia,0); " + Dialecto.UltimoId, tx))
                    {
                        cmd.AddParam("@caja", (object)m.IdCaja ?? DBNull.Value);
                        cmd.AddParam("@fecha", m.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.AddParam("@usuario", m.IdUsuario);
                        cmd.AddParam("@tipo", m.Tipo); cmd.AddParam("@categoria", m.Categoria);
                        cmd.AddParam("@descripcion", m.Descripcion); cmd.AddParam("@monto", m.Monto);
                        cmd.AddParam("@medio", m.MedioPago); cmd.AddParam("@origen", m.Origen);
                        cmd.AddParam("@referencia", m.Referencia);
                        m.IdMovimiento = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    Auditar(con, tx, log, m.IdMovimiento);
                    tx.Commit();
                    return m.IdMovimiento;
                }
            }
        }

        // El cierre y cada movimiento bloquean la misma caja dentro de su transacción.
        internal static void ExigirCajaAbierta(DbConnection con, DbTransaction tx, int idCaja)
        {
            string bloqueo = Dialecto.EsSqlServer ? " WITH (UPDLOCK, HOLDLOCK)" : "";
            using (var cmd = con.Comando("SELECT Estado FROM Caja" + bloqueo + " WHERE IdCaja=@id;", tx))
            {
                cmd.AddParam("@id", idCaja);
                if (Convert.ToString(cmd.ExecuteScalar()) != EstadoCaja.Abierta)
                    throw new InvalidOperationException("El turno ya está cerrado o no existe. No se puede modificar.");
            }
        }

        public void Anular(int id, string motivo, LogMovimiento log)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    int? caja;
                    using (var cmd = con.Comando("SELECT IdCaja FROM MovimientoCaja WHERE IdMovimiento=@id;", tx))
                    {
                        cmd.AddParam("@id", id);
                        var valor = cmd.ExecuteScalar();
                        if (valor == null) throw new InvalidOperationException("El movimiento no existe.");
                        caja = valor == DBNull.Value ? (int?)null : Convert.ToInt32(valor);
                    }
                    if (caja.HasValue) ExigirCajaAbierta(con, tx, caja.Value);
                    using (var cmd = con.Comando(@"UPDATE MovimientoCaja SET Anulado=1,
                        FechaAnulacion=@fecha, IdUsuarioAnulacion=@usuario, MotivoAnulacion=@motivo
                        WHERE IdMovimiento=@id AND Anulado=0;", tx))
                    {
                        cmd.AddParam("@id", id); cmd.AddParam("@motivo", motivo);
                        cmd.AddParam("@fecha", log.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.AddParam("@usuario", log.IdUsuario);
                        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("El movimiento ya está anulado.");
                    }
                    Auditar(con, tx, log, id);
                    tx.Commit();
                }
            }
        }

        private static void Auditar(DbConnection con, DbTransaction tx, LogMovimiento log, int id)
        {
            using (var cmd = con.Comando(@"INSERT INTO LogMovimiento (Fecha,IdUsuario,NombreUsuario,Modulo,Accion,Detalle)
                VALUES (@fecha,@usuario,@nombre,@modulo,@accion,@detalle);", tx))
            {
                cmd.AddParam("@fecha", log.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.AddParam("@usuario", log.IdUsuario); cmd.AddParam("@nombre", log.NombreUsuario);
                cmd.AddParam("@modulo", "Caja"); cmd.AddParam("@accion", log.Accion);
                cmd.AddParam("@detalle", "Movimiento N°" + id + " | " + log.Detalle);
                cmd.ExecuteNonQuery();
            }
        }

        private static MovimientoCaja Mapear(DbDataReader r) => new MovimientoCaja
        {
            IdMovimiento = Convert.ToInt32(r["IdMovimiento"]),
            IdCaja = r["IdCaja"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["IdCaja"]),
            Fecha = Persistencia.LeerFecha(Convert.ToString(r["Fecha"])),
            IdUsuario = Convert.ToInt32(r["IdUsuario"]), NombreUsuario = Convert.ToString(r["Operador"]),
            Tipo = Convert.ToString(r["Tipo"]), Categoria = Convert.ToString(r["Categoria"]),
            Descripcion = Convert.ToString(r["Descripcion"]), Monto = Convert.ToDecimal(r["Monto"]),
            MedioPago = Convert.ToString(r["MedioPago"]), Origen = Convert.ToString(r["Origen"]),
            Referencia = Convert.ToString(r["Referencia"]), Anulado = Convert.ToInt32(r["Anulado"]) != 0,
            FechaAnulacion = r["FechaAnulacion"] == DBNull.Value ? (DateTime?)null : Persistencia.LeerFecha(Convert.ToString(r["FechaAnulacion"])),
            IdUsuarioAnulacion = r["IdUsuarioAnulacion"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["IdUsuarioAnulacion"]),
            NombreUsuarioAnulacion = Convert.ToString(r["Anulador"]), MotivoAnulacion = Convert.ToString(r["MotivoAnulacion"])
        };
    }
}

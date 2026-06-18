using System;
using System.Collections.Generic;
using System.Data.Common;
using Entidades;

namespace AccesoData.DAO
{
    public class LogDao : ConexionBD
    {
        public void Registrar(LogMovimiento log)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    INSERT INTO LogMovimiento (Fecha, IdUsuario, NombreUsuario, Modulo, Accion, Detalle)
                    VALUES (@fecha, @idUsuario, @nombreUsuario, @modulo, @accion, @detalle);";
                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@fecha", log.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.AddParam("@idUsuario", log.IdUsuario);
                    cmd.AddParam("@nombreUsuario", log.NombreUsuario);
                    cmd.AddParam("@modulo", log.Modulo);
                    cmd.AddParam("@accion", log.Accion);
                    cmd.AddParam("@detalle", log.Detalle);   // AddParam convierte null en DBNull
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<LogMovimiento> Obtener(DateTime desde, DateTime hasta, int idUsuario = 0, string modulo = null)
        {
            var lista = new List<LogMovimiento>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    SELECT IdLog, Fecha, IdUsuario, NombreUsuario, Modulo, Accion, Detalle
                    FROM LogMovimiento
                    WHERE Fecha BETWEEN @desde AND @hasta";

                if (idUsuario > 0) sql += " AND IdUsuario = @idUsuario";
                if (!string.IsNullOrEmpty(modulo)) sql += " AND Modulo = @modulo";
                sql += " ORDER BY Fecha DESC;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@desde", desde.ToString("yyyy-MM-dd 00:00:00"));
                    cmd.AddParam("@hasta", hasta.ToString("yyyy-MM-dd 23:59:59"));
                    if (idUsuario > 0) cmd.AddParam("@idUsuario", idUsuario);
                    if (!string.IsNullOrEmpty(modulo)) cmd.AddParam("@modulo", modulo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new LogMovimiento
                            {
                                IdLog = reader.GetInt32(0),
                                Fecha = Persistencia.LeerFecha(reader.GetString(1)),
                                IdUsuario = reader.GetInt32(2),
                                NombreUsuario = reader.GetString(3),
                                Modulo = reader.GetString(4),
                                Accion = reader.GetString(5),
                                Detalle = reader.IsDBNull(6) ? "" : reader.GetString(6)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<string> ObtenerUsuariosConLog()
        {
            var lista = new List<string>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "SELECT DISTINCT NombreUsuario FROM LogMovimiento ORDER BY NombreUsuario;";
                using (var cmd = con.Comando(sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        lista.Add(reader.GetString(0));
            }
            return lista;
        }
    }
}

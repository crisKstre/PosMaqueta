using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Entidades;

namespace AccesoData.DAO
{
    public sealed class ProveedorDao : ConexionBD
    {
        public List<Proveedor> Obtener(string texto, bool? activo)
        {
            var lista = new List<Proveedor>();
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(@"SELECT * FROM Proveedor WHERE (@activo IS NULL OR Activo=@activo)
                    AND (LOWER(Nombre) LIKE LOWER(@q) ESCAPE '\' OR REPLACE(Rut,'-','') LIKE @rut ESCAPE '\'
                    OR LOWER(Contacto) LIKE LOWER(@q) ESCAPE '\' OR Telefono LIKE @q ESCAPE '\'
                    OR LOWER(Correo) LIKE LOWER(@q) ESCAPE '\') ORDER BY Nombre, IdProveedor;"))
                {
                    cmd.AddParam("@activo", activo.HasValue ? (object)(activo.Value ? 1 : 0) : DBNull.Value);
                    cmd.AddParam("@q", "%" + Escapar(texto) + "%");
                    cmd.AddParam("@rut", "%" + Escapar((texto ?? "").Replace(".", "").Replace("-", "").Replace(" ", "").ToUpperInvariant()) + "%");
                    using (var r = cmd.ExecuteReader()) while (r.Read()) lista.Add(Mapear(r));
                }
            }
            return lista;
        }

        private static string Escapar(string valor) => (valor ?? "").Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");

        public Proveedor ObtenerPorId(int id)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT * FROM Proveedor WHERE IdProveedor=@id;"))
                {
                    cmd.AddParam("@id", id);
                    using (var r = cmd.ExecuteReader()) return r.Read() ? Mapear(r) : null;
                }
            }
        }

        public int Guardar(Proveedor p, bool nuevo, LogMovimiento log)
        {
            try
            {
                using (var con = GetConnection())
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        using (var cmd = con.Comando(nuevo
                            ? @"INSERT INTO Proveedor (Nombre,Rut,Contacto,Telefono,Correo,Direccion,Notas,Activo,FechaCreacion,FechaActualizacion)
                                VALUES (@nombre,@rut,@contacto,@telefono,@correo,@direccion,@notas,1,@fecha,@fecha); " + Dialecto.UltimoId
                            : @"UPDATE Proveedor SET Nombre=@nombre,Rut=@rut,Contacto=@contacto,Telefono=@telefono,
                                Correo=@correo,Direccion=@direccion,Notas=@notas,FechaActualizacion=@fecha WHERE IdProveedor=@id;", tx))
                        {
                            cmd.AddParam("@nombre", p.Nombre); cmd.AddParam("@rut", string.IsNullOrEmpty(p.Rut) ? (object)DBNull.Value : p.Rut);
                            cmd.AddParam("@contacto", p.Contacto); cmd.AddParam("@telefono", p.Telefono); cmd.AddParam("@correo", p.Correo);
                            cmd.AddParam("@direccion", p.Direccion); cmd.AddParam("@notas", p.Notas);
                            cmd.AddParam("@fecha", log.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.AddParam("@id", p.IdProveedor);
                            if (nuevo) p.IdProveedor = Convert.ToInt32(cmd.ExecuteScalar());
                            else if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("El proveedor ya no existe. Actualiza el listado.");
                        }
                        Auditar(con, tx, p.IdProveedor, log);
                        tx.Commit();
                        return p.IdProveedor;
                    }
                }
            }
            catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == 2067)
            { throw new InvalidOperationException("Ya existe un proveedor con ese RUT, incluso si está inactivo.", ex); }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            { throw new InvalidOperationException("Ya existe un proveedor con ese RUT, incluso si está inactivo.", ex); }
        }

        public void CambiarEstado(int id, bool activo, LogMovimiento log)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    using (var cmd = con.Comando("UPDATE Proveedor SET Activo=@activo,FechaActualizacion=@fecha WHERE IdProveedor=@id AND Activo<>@activo;", tx))
                    {
                        cmd.AddParam("@id", id); cmd.AddParam("@activo", activo ? 1 : 0); cmd.AddParam("@fecha", log.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("El proveedor no existe o su estado ya cambió. Actualiza el listado.");
                    }
                    Auditar(con, tx, id, log); tx.Commit();
                }
            }
        }

        private static void Auditar(DbConnection con, DbTransaction tx, int id, LogMovimiento log)
        {
            using (var cmd = con.Comando(@"INSERT INTO LogMovimiento (Fecha,IdUsuario,NombreUsuario,Modulo,Accion,Detalle)
                VALUES (@fecha,@usuario,@nombre,@modulo,@accion,@detalle);", tx))
            {
                cmd.AddParam("@fecha", log.Fecha.ToString("yyyy-MM-dd HH:mm:ss")); cmd.AddParam("@usuario", log.IdUsuario);
                cmd.AddParam("@nombre", log.NombreUsuario); cmd.AddParam("@modulo", ModuloLog.Proveedores);
                cmd.AddParam("@accion", log.Accion); cmd.AddParam("@detalle", "Proveedor N°" + id + " | " + log.Detalle);
                cmd.ExecuteNonQuery();
            }
        }

        private static Proveedor Mapear(DbDataReader r) => new Proveedor
        {
            IdProveedor = Convert.ToInt32(r["IdProveedor"]), Nombre = Convert.ToString(r["Nombre"]), Rut = Convert.ToString(r["Rut"]),
            Contacto = Convert.ToString(r["Contacto"]), Telefono = Convert.ToString(r["Telefono"]), Correo = Convert.ToString(r["Correo"]),
            Direccion = Convert.ToString(r["Direccion"]), Notas = Convert.ToString(r["Notas"]), Activo = Convert.ToInt32(r["Activo"]) != 0,
            FechaCreacion = Persistencia.LeerFecha(Convert.ToString(r["FechaCreacion"])), FechaActualizacion = Persistencia.LeerFecha(Convert.ToString(r["FechaActualizacion"]))
        };
    }
}

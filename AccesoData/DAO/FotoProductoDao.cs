using System;
using System.Data;
using System.Data.Common;
using Entidades;

namespace AccesoData.DAO
{
    public sealed class FotoProductoDao : ConexionBD
    {
        public byte[] ObtenerImagen(int idProducto) => Obtener(idProducto, false);
        public byte[] ObtenerMiniatura(int idProducto) => Obtener(idProducto, true);

        private byte[] Obtener(int idProducto, bool miniatura)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT " + (miniatura ? "Miniatura" : "Imagen") + " FROM FotoProducto WHERE IdProducto = @id;"))
                {
                    cmd.AddParam("@id", idProducto);
                    var valor = cmd.ExecuteScalar();
                    return valor == null || valor == DBNull.Value ? null : (byte[])valor;
                }
            }
        }

        // Reemplazo y auditoría atómicos. Una foto null quita la foto.
        public void Guardar(int idProducto, FotoProducto foto, LogMovimiento log)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    using (var cmd = con.Comando("DELETE FROM FotoProducto WHERE IdProducto = @id;", tx))
                    {
                        cmd.AddParam("@id", idProducto);
                        cmd.ExecuteNonQuery();
                    }
                    if (foto != null)
                        using (var cmd = con.Comando("INSERT INTO FotoProducto (IdProducto, Imagen, Miniatura) VALUES (@id, @imagen, @miniatura);", tx))
                        {
                            cmd.AddParam("@id", idProducto);
                            Binario(cmd, "@imagen", foto.Imagen);
                            Binario(cmd, "@miniatura", foto.Miniatura);
                            cmd.ExecuteNonQuery();
                        }
                    using (var cmd = con.Comando(@"INSERT INTO LogMovimiento (Fecha, IdUsuario, NombreUsuario, Modulo, Accion, Detalle)
                        VALUES (@fecha, @usuario, @nombre, @modulo, @accion, @detalle);", tx))
                    {
                        cmd.AddParam("@fecha", log.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.AddParam("@usuario", log.IdUsuario);
                        cmd.AddParam("@nombre", log.NombreUsuario);
                        cmd.AddParam("@modulo", log.Modulo);
                        cmd.AddParam("@accion", log.Accion);
                        cmd.AddParam("@detalle", log.Detalle);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        private static void Binario(DbCommand cmd, string nombre, byte[] contenido)
        {
            var parametro = cmd.CreateParameter();
            parametro.ParameterName = nombre;
            parametro.DbType = DbType.Binary;
            parametro.Size = Dialecto.EsSqlServer ? -1 : contenido.Length;
            parametro.Value = contenido;
            cmd.Parameters.Add(parametro);
        }
    }
}

using System.Data.Common;
using Entidades;

namespace AccesoData.DAO
{
    public class UsuarioDao : ConexionSqlite
    {
        public Usuario Login(string loginNombre, string pass)
        {
            using (var con = GetConnection())
            {
                con.Open();

                Usuario usuario = null;
                string hashGuardado = null;
                string sql = @"
                    SELECT IdUsuario, Nombre, LoginNombre, Pass, Rol, Activo
                    FROM Usuario
                    WHERE LoginNombre = @login AND Activo = 1;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@login", loginNombre);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hashGuardado = reader.GetString(3);
                            usuario = new Usuario
                            {
                                IdUsuario = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                LoginNombre = reader.GetString(2),
                                Pass = hashGuardado,
                                Rol = reader.GetString(4),
                                Activo = reader.GetInt32(5) == 1
                            };
                        }
                    }
                }

                if (usuario == null || !Seguridad.Verificar(pass, hashGuardado))
                {
                    Log.Advertencia("Login FALLIDO para usuario '" + loginNombre + "'");
                    return null;
                }

                // Migración transparente: si el hash es del formato antiguo, lo regeneramos con PBKDF2
                if (Seguridad.NecesitaRehash(hashGuardado))
                {
                    ActualizarPass(con, usuario.IdUsuario, Seguridad.Hash(pass));
                    Log.Info("Contraseña migrada a PBKDF2 para '" + loginNombre + "'");
                }

                Log.Info("Login exitoso: '" + usuario.LoginNombre + "' (rol " + usuario.Rol + ")");
                return usuario;
            }
        }

        private void ActualizarPass(DbConnection con, int idUsuario, string nuevoHash)
        {
            using (var cmd = con.Comando("UPDATE Usuario SET Pass = @pass WHERE IdUsuario = @id;"))
            {
                cmd.AddParam("@pass", nuevoHash);
                cmd.AddParam("@id", idUsuario);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

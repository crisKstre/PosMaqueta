using System;
using System.Collections.Generic;
using System.Data.Common;
using Entidades;

namespace AccesoData.DAO
{
    public class UsuarioDao : ConexionBD
    {
        // Columnas y orden usados por todos los SELECT de este DAO.
        private const string Columnas =
            "IdUsuario, Nombre, LoginNombre, Pass, Rol, Activo, DebeCambiarPassword";

        public Usuario Login(string loginNombre, string pass)
        {
            using (var con = GetConnection())
            {
                con.Open();

                Usuario usuario = null;
                string hashGuardado = null;
                string sql = @"
                    SELECT " + Columnas + @"
                    FROM Usuario
                    WHERE LoginNombre = @login AND Activo = 1;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@login", loginNombre);
                    using (var reader = cmd.ExecuteReader())
                        if (reader.Read())
                        {
                            usuario = Mapear(reader);
                            hashGuardado = usuario.Pass;
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

        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "SELECT " + Columnas + " FROM Usuario ORDER BY Activo DESC, Nombre;";
                using (var cmd = con.Comando(sql))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        lista.Add(Mapear(reader));
            }
            return lista;
        }

        public Usuario ObtenerPorId(int idUsuario)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("SELECT " + Columnas + " FROM Usuario WHERE IdUsuario = @id;"))
                {
                    cmd.AddParam("@id", idUsuario);
                    using (var reader = cmd.ExecuteReader())
                        return reader.Read() ? Mapear(reader) : null;
                }
            }
        }

        // True si el login ya existe en otro usuario (idExcluir permite editar sin chocar consigo mismo).
        public bool ExisteLogin(string loginNombre, int idExcluir = 0)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(
                    "SELECT COUNT(*) FROM Usuario WHERE LoginNombre = @login AND IdUsuario <> @id;"))
                {
                    cmd.AddParam("@login", loginNombre);
                    cmd.AddParam("@id", idExcluir);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public int ContarAdminsActivos()
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(
                    "SELECT COUNT(*) FROM Usuario WHERE Rol = @rol AND Activo = 1;"))
                {
                    cmd.AddParam("@rol", RolUsuario.Admin);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // u.Pass debe venir ya hasheado por el servicio (Seguridad.Hash).
        public int Insertar(Usuario u)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    INSERT INTO Usuario (Nombre, LoginNombre, Pass, Rol, Activo, DebeCambiarPassword)
                    VALUES (@nombre, @login, @pass, @rol, @activo, @debe);
                    " + Dialecto.UltimoId;

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@nombre", u.Nombre);
                    cmd.AddParam("@login", u.LoginNombre);
                    cmd.AddParam("@pass", u.Pass);
                    cmd.AddParam("@rol", u.Rol);
                    cmd.AddParam("@activo", u.Activo ? 1 : 0);
                    cmd.AddParam("@debe", u.DebeCambiarPassword ? 1 : 0);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // Actualiza datos de perfil (no toca la contraseña; para eso está CambiarPass).
        public bool Actualizar(Usuario u)
        {
            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"
                    UPDATE Usuario SET
                        Nombre      = @nombre,
                        LoginNombre = @login,
                        Rol         = @rol,
                        Activo      = @activo
                    WHERE IdUsuario = @id;";

                using (var cmd = con.Comando(sql))
                {
                    cmd.AddParam("@nombre", u.Nombre);
                    cmd.AddParam("@login", u.LoginNombre);
                    cmd.AddParam("@rol", u.Rol);
                    cmd.AddParam("@activo", u.Activo ? 1 : 0);
                    cmd.AddParam("@id", u.IdUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CambiarEstado(int idUsuario, bool activo)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando("UPDATE Usuario SET Activo = @activo WHERE IdUsuario = @id;"))
                {
                    cmd.AddParam("@activo", activo ? 1 : 0);
                    cmd.AddParam("@id", idUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // nuevoHash ya viene calculado por el servicio. debeCambiar=true en un reseteo por admin
        // (obliga a cambiarla en el próximo ingreso); false en un cambio voluntario propio.
        public bool CambiarPass(int idUsuario, string nuevoHash, bool debeCambiar)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = con.Comando(
                    "UPDATE Usuario SET Pass = @pass, DebeCambiarPassword = @debe WHERE IdUsuario = @id;"))
                {
                    cmd.AddParam("@pass", nuevoHash);
                    cmd.AddParam("@debe", debeCambiar ? 1 : 0);
                    cmd.AddParam("@id", idUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Usado solo por la migración transparente de hash dentro de Login (reusa la conexión abierta).
        private void ActualizarPass(DbConnection con, int idUsuario, string nuevoHash)
        {
            using (var cmd = con.Comando("UPDATE Usuario SET Pass = @pass WHERE IdUsuario = @id;"))
            {
                cmd.AddParam("@pass", nuevoHash);
                cmd.AddParam("@id", idUsuario);
                cmd.ExecuteNonQuery();
            }
        }

        private Usuario Mapear(DbDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                LoginNombre = reader.GetString(2),
                Pass = reader.GetString(3),
                Rol = reader.GetString(4),
                Activo = reader.GetInt32(5) == 1,
                DebeCambiarPassword = reader.GetInt32(6) == 1
            };
        }
    }
}

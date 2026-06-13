using Microsoft.Data.Sqlite;
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
                string sql = @"
                    SELECT IdUsuario, Nombre, LoginNombre, Pass, Rol, Activo
                    FROM Usuario
                    WHERE LoginNombre = @login AND Pass = @pass AND Activo = 1;";

                using (var cmd = new SqliteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@login", loginNombre);
                    cmd.Parameters.AddWithValue("@pass", Seguridad.Hash(pass));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                IdUsuario = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                LoginNombre = reader.GetString(2),
                                Pass = reader.GetString(3),
                                Rol = reader.GetString(4),
                                Activo = reader.GetInt32(5) == 1
                            };
                        }
                        return null;
                    }
                }
            }
        }
    }
}

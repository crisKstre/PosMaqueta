using AccesoData.DAO;
using Entidades;

namespace Dominio.Servicios
{
    public class UsuarioService
    {
        private readonly UsuarioDao usuarioDao = new UsuarioDao();

        public Usuario Login(string loginNombre, string pass)
        {
            return usuarioDao.Login(loginNombre, pass);
        }
    }
}

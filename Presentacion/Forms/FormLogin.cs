using System;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormLogin : Form
    {
        private readonly UsuarioService usuarioService = new UsuarioService();

        public FormLogin()
        {
            InitializeComponent();
            this.Load   += (s, e) => CentrarCard();
            this.Resize += (s, e) => CentrarCard();
            pnlFondo.SizeChanged += (s, e) => CentrarCard();
        }

        private void CentrarCard()
        {
            pnlCard.Location = new System.Drawing.Point(
                (pnlFondo.ClientSize.Width  - pnlCard.Width)  / 2,
                (pnlFondo.ClientSize.Height - pnlCard.Height) / 2
            );
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string pass    = txtPass.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(pass))
            { MostrarError("Ingresa usuario y contraseña."); return; }

            Usuario logueado = usuarioService.Login(usuario, pass);

            if (logueado == null)
            {
                MostrarError("Usuario o contraseña incorrectos.");
                txtPass.Clear();
                txtUsuario.Focus();
                return;
            }

            // Si la cuenta está marcada para cambio de contraseña (admin por defecto, usuario
            // recién creado o reseteado por un admin), se obliga a cambiarla antes de entrar.
            if (usuarioService.RequiereCambioPassword(logueado) && !ForzarCambioPassword(logueado, pass))
            {
                txtPass.Clear();
                txtUsuario.Focus();
                return;   // canceló el cambio obligatorio: no se inicia sesión
            }

            Sesion.UsuarioActual = logueado;

            var principal = new FormPrincipal();
            principal.FormClosed += (s, args) => this.Close();
            this.Hide();
            principal.Show();
        }

        // Obliga al usuario a definir una nueva contraseña distinta de la actual.
        // passwordActual es la recién validada en el login. Devuelve true si la cambió.
        private bool ForzarCambioPassword(Usuario u, string passwordActual)
        {
            while (true)
            {
                using (var dlg = new FormCambiarPassword("Cambia tu contraseña",
                    "Por seguridad debes definir una nueva contraseña para «" + u.LoginNombre + "» antes de continuar.",
                    pedirActual: false, obligatorio: true))
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return false;
                    try
                    {
                        usuarioService.CambiarPasswordPropia(u.IdUsuario, passwordActual, dlg.PasswordNueva);
                        u.DebeCambiarPassword = false;
                        return true;
                    }
                    catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo cambiar"); }
                }
            }
        }

        private void MostrarError(string msg)
        {
            lblError.Text    = msg;
            lblError.Visible = true;
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { btnIngresar_Click(sender, e); e.SuppressKeyPress = true; }
        }
    }
}

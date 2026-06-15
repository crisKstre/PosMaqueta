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

            Sesion.UsuarioActual = logueado;

            var principal = new FormPrincipal();
            principal.FormClosed += (s, args) => this.Close();
            this.Hide();
            principal.Show();
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

using System;
using System.Drawing;
using System.Windows.Forms;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormPrincipal : Form
    {
        private Button botonActivo;
        private Form formHijoActual;
        private bool cerrandoSesion = false;

        public FormPrincipal()
        {
            InitializeComponent();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = Sesion.UsuarioActual.Nombre;
            lblRol.Text = Sesion.UsuarioActual.Rol;

            // Control de acceso por rol: el cajero no ve la gestión de productos
            btnProductos.Visible = Sesion.EsAdmin;
        }

        private static readonly Color ColorSidebar = Color.FromArgb(28, 28, 30);
        private static readonly Color ColorActivo = Color.FromArgb(58, 58, 60);
        private static readonly Color ColorTextoIdle = Color.FromArgb(199, 199, 204);

        private void ActivarBoton(Button boton)
        {
            if (botonActivo != null)
            {
                botonActivo.BackColor = ColorSidebar;
                botonActivo.ForeColor = ColorTextoIdle;
            }
            botonActivo = boton;
            boton.BackColor = ColorActivo;
            boton.ForeColor = Color.White;
        }

        private void AbrirFormHijo(Form formHijo, Button boton)
        {
            formHijoActual?.Close();
            ActivarBoton(boton);

            formHijoActual = formHijo;
            formHijo.TopLevel = false;
            formHijo.FormBorderStyle = FormBorderStyle.None;
            formHijo.Dock = DockStyle.Fill;

            pnlContenido.Controls.Add(formHijo);
            pnlContenido.Tag = formHijo;
            formHijo.BringToFront();
            formHijo.Show();

            lblTituloSeccion.Text = formHijo.Text;
        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            AbrirFormHijo(new FormVentas(), btnVentas);
        }

        private void btnProductos_Click(object sender, EventArgs e)
        {
            AbrirFormHijo(new FormProductos(), btnProductos);
        }

        private void btnCaja_Click(object sender, EventArgs e)
        {
            AbrirFormHijo(new FormCaja(), btnCaja);
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            cerrandoSesion = true;
            Sesion.Cerrar();
            var login = new FormLogin();
            login.Show();
            this.Close();
        }

        private void FormPrincipal_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!cerrandoSesion)
                Application.Exit();
        }
    }
}

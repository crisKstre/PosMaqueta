using System;
using System.Drawing;
using System.Windows.Forms;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormPrincipal : Form
    {
        private Button botonActivo;
        private Form   formHijoActual;
        private bool   cerrandoSesion = false;
        private Timer  timerReloj;

        public FormPrincipal()
        {
            InitializeComponent();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = Sesion.UsuarioActual.Nombre;
            lblRol.Text     = Sesion.UsuarioActual.Rol;

            btnProductos.Visible = Sesion.EsAdmin;

            IniciarReloj();
            AbrirFormHijo(new FormVentas(), btnVentas);
        }

        private void IniciarReloj()
        {
            timerReloj          = new Timer();
            timerReloj.Interval = 1000;
            timerReloj.Tick    += (s, e) => ActualizarReloj();
            timerReloj.Start();
            ActualizarReloj();
        }

        private void ActualizarReloj()
        {
            lblReloj.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
        }

        private void ActivarBoton(Button boton)
        {
            if (botonActivo != null)
            {
                botonActivo.BackColor = EstiloPos.Sidebar;
                botonActivo.ForeColor = Color.FromArgb(160, 160, 175);
            }
            botonActivo           = boton;
            boton.BackColor       = EstiloPos.SidebarActivo;
            boton.ForeColor       = Color.White;
            // Acento: borde izquierdo visible usando padding
            boton.Padding         = new Padding(4, 0, 0, 0);
            boton.FlatAppearance.BorderSize  = 0;
        }

        private void AbrirFormHijo(Form formHijo, Button boton)
        {
            formHijoActual?.Close();
            ActivarBoton(boton);

            formHijo.TopLevel         = false;
            formHijo.FormBorderStyle  = FormBorderStyle.None;
            formHijo.Dock             = DockStyle.Fill;

            pnlContenido.Controls.Add(formHijo);
            formHijo.BringToFront();
            formHijo.Show();

            lblTituloSeccion.Text = formHijo.Text;
            formHijoActual        = formHijo;
        }

        private void btnVentas_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormVentas(), btnVentas);

        private void btnProductos_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormProductos(), btnProductos);

        private void btnCaja_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormCaja(), btnCaja);

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            timerReloj?.Stop();
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

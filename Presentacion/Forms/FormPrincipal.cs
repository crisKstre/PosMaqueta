using System;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Servicios;
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

            btnProductos.Visible = true;   // todos ven el inventario; los empleados en modo solo lectura
            btnReportes.Visible  = Sesion.EsAdmin;   // reportes de negocio: solo administrador

            EstilizarSidebar();
            IniciarReloj();
            AbrirFormHijo(new FormDashboard(), btnDashboard);
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

        // ── Sidebar estilo POS (ícono + texto a la izquierda, activo con barra de acento) ──

        private void EstilizarSidebar()
        {
            ConfigItemSidebar(btnDashboard, "🏠", "Inicio");
            ConfigItemSidebar(btnVentas,    "🛒", "Ventas");
            ConfigItemSidebar(btnProductos, "📦", "Productos");
            ConfigItemSidebar(btnCaja,      "💵", "Caja");
            ConfigItemSidebar(btnReportes,  "📊", "Reportes");

            btnCerrarSesion.Text      = "🚪   Cerrar sesión";
            btnCerrarSesion.TextAlign = ContentAlignment.MiddleLeft;
            btnCerrarSesion.Padding   = new Padding(16, 0, 0, 0);
            btnCerrarSesion.Font      = EstiloPos.FontSidebar;
            btnCerrarSesion.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 52, 64);
        }

        private void ConfigItemSidebar(Button btn, string icono, string texto)
        {
            btn.Text      = icono + "   " + texto;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding   = new Padding(16, 0, 0, 0);
            btn.Font      = EstiloPos.FontSidebar;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 52);
            btn.Paint += SidebarBtn_Paint;
        }

        // Dibuja la barra de acento azul a la izquierda del ítem activo
        private void SidebarBtn_Paint(object sender, PaintEventArgs e)
        {
            var btn = (Button)sender;
            if (botonActivo == btn)
                using (var br = new SolidBrush(EstiloPos.SidebarAcento))
                    e.Graphics.FillRectangle(br, 0, 0, 4, btn.Height);
        }

        private void ActivarBoton(Button boton)
        {
            if (botonActivo != null)
            {
                botonActivo.BackColor = EstiloPos.Sidebar;
                botonActivo.ForeColor = Color.FromArgb(160, 160, 175);
                botonActivo.Invalidate();
            }
            botonActivo           = boton;
            boton.BackColor       = EstiloPos.SidebarActivo;
            boton.ForeColor       = Color.White;
            boton.FlatAppearance.BorderSize  = 0;
            boton.Invalidate();   // repinta la barra de acento
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

        private void btnDashboard_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormDashboard(), btnDashboard);

        private void btnVentas_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormVentas(), btnVentas);

        private void btnProductos_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormProductos(), btnProductos);

        private void btnCaja_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormCaja(), btnCaja);

        private void btnReportes_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormReportes(), btnReportes);

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            timerReloj?.Stop();
            cerrandoSesion = true;
            AccesoData.Log.Info("Cierre de sesión: " + (Sesion.UsuarioActual?.LoginNombre ?? "?"));
            VentaService.ReiniciarVentasEnCurso();   // las ventas en curso son del cajero que sale
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

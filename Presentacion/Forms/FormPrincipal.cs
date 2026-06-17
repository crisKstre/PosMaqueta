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
        private ToolTip toolTipNav = new ToolTip();
        private readonly UsuarioService usuarioService = new UsuarioService();

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
            btnUsuarios.Visible  = Sesion.EsAdmin;   // gestión de usuarios: solo administrador
            btnRespaldos.Visible = Sesion.EsAdmin;   // respaldos / restauración: solo administrador

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
            ConfigItemSidebar(btnUsuarios,  "👤", "Usuarios");
            ConfigItemSidebar(btnRespaldos, "💾", "Respaldos");

            toolTipNav.SetToolTip(btnDashboard, "Inicio (Ctrl+1)");
            toolTipNav.SetToolTip(btnVentas,    "Ventas (Ctrl+2)");
            toolTipNav.SetToolTip(btnProductos, "Productos (Ctrl+3)");
            toolTipNav.SetToolTip(btnCaja,      "Caja (Ctrl+4)");
            toolTipNav.SetToolTip(btnReportes,  "Reportes (Ctrl+5)");
            toolTipNav.SetToolTip(btnUsuarios,  "Usuarios (Ctrl+6)");
            toolTipNav.SetToolTip(btnRespaldos, "Respaldos (Ctrl+7)");

            btnCambiarPass.Text      = "🔑   Cambiar contraseña";
            btnCambiarPass.TextAlign = ContentAlignment.MiddleLeft;
            btnCambiarPass.Padding   = new Padding(16, 0, 0, 0);
            btnCambiarPass.Font      = EstiloPos.FontSidebar;
            btnCambiarPass.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 52, 64);

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
            // Los forms hijo son no-top-level: Close() NO dispara FormClosed ni los libera. Hay que
            // quitarlos del panel y disponerlos a mano, o quedan vivos (fugas + handlers/timers huérfanos).
            if (formHijoActual != null)
            {
                pnlContenido.Controls.Remove(formHijoActual);
                formHijoActual.Dispose();
            }
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

        private void btnUsuarios_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormUsuarios(), btnUsuarios);

        private void btnRespaldos_Click(object sender, EventArgs e)
            => AbrirFormHijo(new FormRespaldos(), btnRespaldos);

        // Cambio de la propia contraseña (cualquier rol). Reintenta si el servicio rechaza.
        private void btnCambiarPass_Click(object sender, EventArgs e)
        {
            var u = Sesion.UsuarioActual;
            if (u == null) return;
            while (true)
            {
                using (var dlg = new FormCambiarPassword("Cambiar contraseña",
                    "Cambia la contraseña de tu cuenta «" + u.LoginNombre + "».",
                    pedirActual: true, obligatorio: false))
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;
                    try
                    {
                        usuarioService.CambiarPasswordPropia(u.IdUsuario, dlg.PasswordActual, dlg.PasswordNueva);
                        Aviso.Exito(this, "Tu contraseña se cambió correctamente.", "Listo");
                        return;
                    }
                    catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo"); }
                }
            }
        }

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

        // Navegación global entre módulos con Ctrl+1..5 (desde cualquier pantalla).
        // Los forms hijo procesan sus propias teclas primero; estas combinaciones no chocan.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Button destino = null;
            switch (keyData)
            {
                case Keys.Control | Keys.D1: destino = btnDashboard; break;
                case Keys.Control | Keys.D2: destino = btnVentas;    break;
                case Keys.Control | Keys.D3: destino = btnProductos; break;
                case Keys.Control | Keys.D4: destino = btnCaja;      break;
                case Keys.Control | Keys.D5: destino = btnReportes;  break;
                case Keys.Control | Keys.D6: destino = btnUsuarios;  break;
                case Keys.Control | Keys.D7: destino = btnRespaldos; break;
            }
            if (destino != null)
            {
                if (destino.Visible && destino != botonActivo)   // no reabrir el módulo actual
                    destino.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FormPrincipal_FormClosed(object sender, FormClosedEventArgs e)
        {
            toolTipNav?.Dispose();
            if (!cerrandoSesion)
                Application.Exit();
        }
    }
}

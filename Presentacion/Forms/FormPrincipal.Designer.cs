using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        private Panel  pnlSidebar;
        private Label  lblAppNombre;
        private Label  lblAppSub;
        private Panel  pnlSepUsuario;
        private Label  lblUsuario;
        private Label  lblRol;
        private Panel  pnlSepMenu;
        private Button btnDashboard;
        private Button btnVentas;
        private Button btnProductos;
        private Button btnCaja;
        private Button btnReportes;
        private Button btnUsuarios;
        private Button btnCambiarPass;
        private Button btnCerrarSesion;
        private Panel  pnlTop;
        private Label  lblTituloSeccion;
        private Label  lblReloj;
        private Panel  pnlContenido;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSidebar     = new Panel();
            this.lblAppNombre   = new Label();
            this.lblAppSub      = new Label();
            this.pnlSepUsuario  = new Panel();
            this.lblUsuario     = new Label();
            this.lblRol         = new Label();
            this.pnlSepMenu     = new Panel();
            this.btnDashboard   = new Button();
            this.btnVentas      = new Button();
            this.btnProductos   = new Button();
            this.btnCaja        = new Button();
            this.btnReportes    = new Button();
            this.btnUsuarios    = new Button();
            this.btnCambiarPass = new Button();
            this.btnCerrarSesion = new Button();
            this.pnlTop         = new Panel();
            this.lblTituloSeccion = new Label();
            this.lblReloj       = new Label();
            this.pnlContenido   = new Panel();
            this.pnlSidebar.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();

            // ── Sidebar ──────────────────────────────────────────────
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.pnlSidebar.Dock      = DockStyle.Left;
            this.pnlSidebar.Width     = 200;
            this.pnlSidebar.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblAppNombre, lblAppSub, pnlSepUsuario,
                lblUsuario, lblRol, pnlSepMenu,
                btnDashboard, btnVentas, btnProductos, btnCaja, btnReportes, btnUsuarios,
                btnCambiarPass, btnCerrarSesion });

            // Nombre de la app
            this.lblAppNombre.AutoSize  = false;
            this.lblAppNombre.Text      = "Sistema POS";
            this.lblAppNombre.Font      = new Font("Segoe UI", 15F, FontStyle.Bold);
            this.lblAppNombre.ForeColor = Color.White;
            this.lblAppNombre.Location  = new Point(0, 22);
            this.lblAppNombre.Size      = new Size(200, 28);
            this.lblAppNombre.TextAlign = ContentAlignment.MiddleCenter;

            this.lblAppSub.AutoSize    = false;
            this.lblAppSub.Text        = "Punto de venta";
            this.lblAppSub.Font        = new Font("Segoe UI", 9F);
            this.lblAppSub.ForeColor   = Color.FromArgb(120, 120, 135);
            this.lblAppSub.Location    = new Point(0, 50);
            this.lblAppSub.Size        = new Size(200, 18);
            this.lblAppSub.TextAlign   = ContentAlignment.MiddleCenter;

            // Separador
            this.pnlSepUsuario.BackColor = Color.FromArgb(40, 40, 52);
            this.pnlSepUsuario.Location  = new Point(16, 76);
            this.pnlSepUsuario.Size      = new Size(168, 1);

            // Usuario
            this.lblUsuario.AutoSize  = false;
            this.lblUsuario.Font      = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lblUsuario.ForeColor = Color.FromArgb(230, 230, 235);
            this.lblUsuario.Location  = new Point(0, 86);
            this.lblUsuario.Size      = new Size(200, 22);
            this.lblUsuario.TextAlign = ContentAlignment.MiddleCenter;
            this.lblUsuario.Text      = "Usuario";

            this.lblRol.AutoSize  = false;
            this.lblRol.Font      = new Font("Segoe UI", 9F);
            this.lblRol.ForeColor = Color.FromArgb(110, 110, 130);
            this.lblRol.Location  = new Point(0, 108);
            this.lblRol.Size      = new Size(200, 18);
            this.lblRol.TextAlign = ContentAlignment.MiddleCenter;
            this.lblRol.Text      = "Rol";

            // Separador menú
            this.pnlSepMenu.BackColor = Color.FromArgb(40, 40, 52);
            this.pnlSepMenu.Location  = new Point(16, 134);
            this.pnlSepMenu.Size      = new Size(168, 1);

            // Botones menú — centrados
            // btnDashboard
            this.btnDashboard.BackColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.btnDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashboard.FlatAppearance.BorderSize = 0;
            this.btnDashboard.Font      = new System.Drawing.Font("Segoe UI", 13F);
            this.btnDashboard.ForeColor = System.Drawing.Color.FromArgb(160, 160, 175);
            this.btnDashboard.Location  = new System.Drawing.Point(0, 148);
            this.btnDashboard.Name      = "btnDashboard";
            this.btnDashboard.Size      = new System.Drawing.Size(200, 58);
            this.btnDashboard.Text      = "Inicio";
            this.btnDashboard.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnDashboard.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnDashboard.UseVisualStyleBackColor = false;
            this.btnDashboard.Click += new System.EventHandler(this.btnDashboard_Click);
            // btnVentas
            this.btnVentas.BackColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.btnVentas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVentas.FlatAppearance.BorderSize = 0;
            this.btnVentas.Font      = new System.Drawing.Font("Segoe UI", 13F);
            this.btnVentas.ForeColor = System.Drawing.Color.FromArgb(160, 160, 175);
            this.btnVentas.Location  = new System.Drawing.Point(0, 208);
            this.btnVentas.Name      = "btnVentas";
            this.btnVentas.Size      = new System.Drawing.Size(200, 58);
            this.btnVentas.Text      = "Ventas";
            this.btnVentas.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnVentas.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnVentas.UseVisualStyleBackColor = false;
            this.btnVentas.Click += new System.EventHandler(this.btnVentas_Click);
            // btnProductos
            this.btnProductos.BackColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.btnProductos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProductos.FlatAppearance.BorderSize = 0;
            this.btnProductos.Font      = new System.Drawing.Font("Segoe UI", 13F);
            this.btnProductos.ForeColor = System.Drawing.Color.FromArgb(160, 160, 175);
            this.btnProductos.Location  = new System.Drawing.Point(0, 268);
            this.btnProductos.Name      = "btnProductos";
            this.btnProductos.Size      = new System.Drawing.Size(200, 58);
            this.btnProductos.Text      = "Productos";
            this.btnProductos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProductos.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnProductos.UseVisualStyleBackColor = false;
            this.btnProductos.Click += new System.EventHandler(this.btnProductos_Click);
            // btnCaja
            this.btnCaja.BackColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.btnCaja.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCaja.FlatAppearance.BorderSize = 0;
            this.btnCaja.Font      = new System.Drawing.Font("Segoe UI", 13F);
            this.btnCaja.ForeColor = System.Drawing.Color.FromArgb(160, 160, 175);
            this.btnCaja.Location  = new System.Drawing.Point(0, 328);
            this.btnCaja.Name      = "btnCaja";
            this.btnCaja.Size      = new System.Drawing.Size(200, 58);
            this.btnCaja.Text      = "Caja";
            this.btnCaja.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnCaja.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCaja.UseVisualStyleBackColor = false;
            this.btnCaja.Click += new System.EventHandler(this.btnCaja_Click);
            // btnReportes
            this.btnReportes.BackColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.btnReportes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReportes.FlatAppearance.BorderSize = 0;
            this.btnReportes.Font      = new System.Drawing.Font("Segoe UI", 13F);
            this.btnReportes.ForeColor = System.Drawing.Color.FromArgb(160, 160, 175);
            this.btnReportes.Location  = new System.Drawing.Point(0, 388);
            this.btnReportes.Name      = "btnReportes";
            this.btnReportes.Size      = new System.Drawing.Size(200, 58);
            this.btnReportes.Text      = "Reportes";
            this.btnReportes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnReportes.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnReportes.UseVisualStyleBackColor = false;
            this.btnReportes.Click += new System.EventHandler(this.btnReportes_Click);
            // btnUsuarios (solo admin; su visibilidad se fija en FormPrincipal_Load)
            this.btnUsuarios.BackColor = System.Drawing.Color.FromArgb(20, 20, 25);
            this.btnUsuarios.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsuarios.FlatAppearance.BorderSize = 0;
            this.btnUsuarios.Font      = new System.Drawing.Font("Segoe UI", 13F);
            this.btnUsuarios.ForeColor = System.Drawing.Color.FromArgb(160, 160, 175);
            this.btnUsuarios.Location  = new System.Drawing.Point(0, 448);
            this.btnUsuarios.Name      = "btnUsuarios";
            this.btnUsuarios.Size      = new System.Drawing.Size(200, 58);
            this.btnUsuarios.Text      = "Usuarios";
            this.btnUsuarios.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnUsuarios.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnUsuarios.UseVisualStyleBackColor = false;
            this.btnUsuarios.Click += new System.EventHandler(this.btnUsuarios_Click);

            // Cambiar mi contraseña (disponible para todos los roles)
            this.btnCambiarPass.BackColor  = Color.FromArgb(28, 28, 36);
            this.btnCambiarPass.FlatStyle  = FlatStyle.Flat;
            this.btnCambiarPass.FlatAppearance.BorderSize = 0;
            this.btnCambiarPass.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 62);
            this.btnCambiarPass.Font       = new Font("Segoe UI", 11F);
            this.btnCambiarPass.ForeColor  = Color.FromArgb(160, 160, 175);
            this.btnCambiarPass.Dock       = DockStyle.Bottom;
            this.btnCambiarPass.Height     = 46;
            this.btnCambiarPass.Text       = "Cambiar contraseña";
            this.btnCambiarPass.TextAlign  = ContentAlignment.MiddleCenter;
            this.btnCambiarPass.Cursor     = Cursors.Hand;
            this.btnCambiarPass.UseVisualStyleBackColor = false;
            this.btnCambiarPass.Click      += new System.EventHandler(this.btnCambiarPass_Click);

            // Cerrar sesión
            this.btnCerrarSesion.BackColor  = Color.FromArgb(28, 28, 36);
            this.btnCerrarSesion.FlatStyle  = FlatStyle.Flat;
            this.btnCerrarSesion.FlatAppearance.BorderSize = 0;
            this.btnCerrarSesion.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 62);
            this.btnCerrarSesion.Font       = new Font("Segoe UI", 11F);
            this.btnCerrarSesion.ForeColor  = Color.FromArgb(160, 160, 175);
            this.btnCerrarSesion.Dock       = DockStyle.Bottom;
            this.btnCerrarSesion.Height     = 52;
            this.btnCerrarSesion.Text       = "Cerrar sesión";
            this.btnCerrarSesion.TextAlign  = ContentAlignment.MiddleCenter;
            this.btnCerrarSesion.Cursor     = Cursors.Hand;
            this.btnCerrarSesion.UseVisualStyleBackColor = false;
            this.btnCerrarSesion.Click      += new System.EventHandler(this.btnCerrarSesion_Click);

            // ── Topbar ───────────────────────────────────────────────
            this.pnlTop.BackColor  = System.Drawing.Color.White;
            this.pnlTop.Dock       = DockStyle.Top;
            this.pnlTop.Height     = 52;
            this.pnlTop.Controls.Add(this.lblTituloSeccion);
            this.pnlTop.Controls.Add(this.lblReloj);

            this.lblTituloSeccion.AutoSize  = false;
            this.lblTituloSeccion.Font      = new System.Drawing.Font("Segoe UI", 17F, System.Drawing.FontStyle.Bold);
            this.lblTituloSeccion.ForeColor = System.Drawing.Color.FromArgb(14, 14, 18);
            this.lblTituloSeccion.Location  = new Point(22, 0);
            this.lblTituloSeccion.Size      = new Size(600, 52);
            this.lblTituloSeccion.TextAlign = ContentAlignment.MiddleLeft;
            this.lblTituloSeccion.Text      = "Bienvenido";

            this.lblReloj.AutoSize  = false;
            this.lblReloj.Font      = new Font("Segoe UI", 10F);
            this.lblReloj.ForeColor = System.Drawing.Color.FromArgb(143, 143, 154);
            this.lblReloj.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
            this.lblReloj.Location  = new Point(700, 0);
            this.lblReloj.Size      = new Size(260, 52);
            this.lblReloj.TextAlign = ContentAlignment.MiddleRight;

            // ── Área de contenido ────────────────────────────────────
            this.pnlContenido.BackColor = System.Drawing.Color.FromArgb(245, 244, 241);
            this.pnlContenido.Dock      = DockStyle.Fill;

            // ── FormPrincipal ────────────────────────────────────────
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize    = new Size(1280, 800);
            this.Controls.Add(this.pnlContenido);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlSidebar);
            this.Name             = "FormPrincipal";
            this.StartPosition    = FormStartPosition.CenterScreen;
            this.Text             = "Sistema POS";
            this.WindowState      = FormWindowState.Maximized;
            this.Load             += new System.EventHandler(this.FormPrincipal_Load);
            this.FormClosed       += new System.Windows.Forms.FormClosedEventHandler(this.FormPrincipal_FormClosed);
            this.pnlSidebar.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.ResumeLayout(false);
        }

    }
}

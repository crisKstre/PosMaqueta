using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlSidebar;
        private Label lblTituloApp;
        private Label lblUsuario;
        private Label lblRol;
        private Button btnVentas;
        private Button btnProductos;
        private Button btnCaja;
        private Button btnCerrarSesion;
        private Panel pnlTop;
        private Label lblTituloSeccion;
        private Panel pnlContenido;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSidebar = new Panel();
            this.lblTituloApp = new Label();
            this.lblUsuario = new Label();
            this.lblRol = new Label();
            this.btnVentas = new Button();
            this.btnProductos = new Button();
            this.btnCaja = new Button();
            this.btnCerrarSesion = new Button();
            this.pnlTop = new Panel();
            this.lblTituloSeccion = new Label();
            this.pnlContenido = new Panel();
            this.pnlSidebar.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlSidebar
            //
            this.pnlSidebar.BackColor = Color.FromArgb(28, 28, 30);
            this.pnlSidebar.Controls.Add(this.lblTituloApp);
            this.pnlSidebar.Controls.Add(this.lblUsuario);
            this.pnlSidebar.Controls.Add(this.lblRol);
            this.pnlSidebar.Controls.Add(this.btnVentas);
            this.pnlSidebar.Controls.Add(this.btnProductos);
            this.pnlSidebar.Controls.Add(this.btnCaja);
            this.pnlSidebar.Controls.Add(this.btnCerrarSesion);
            this.pnlSidebar.Dock = DockStyle.Left;
            this.pnlSidebar.Width = 230;
            //
            // lblTituloApp
            //
            this.lblTituloApp.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold);
            this.lblTituloApp.ForeColor = Color.FromArgb(245, 245, 244);
            this.lblTituloApp.Location = new Point(0, 28);
            this.lblTituloApp.Size = new Size(230, 32);
            this.lblTituloApp.Text = "Sistema POS";
            this.lblTituloApp.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblUsuario
            //
            this.lblUsuario.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this.lblUsuario.ForeColor = Color.FromArgb(229, 229, 228);
            this.lblUsuario.Location = new Point(0, 72);
            this.lblUsuario.Size = new Size(230, 24);
            this.lblUsuario.Text = "Usuario";
            this.lblUsuario.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblRol
            //
            this.lblRol.Font = new Font("Segoe UI", 9F);
            this.lblRol.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblRol.Location = new Point(0, 96);
            this.lblRol.Size = new Size(230, 20);
            this.lblRol.Text = "Rol";
            this.lblRol.TextAlign = ContentAlignment.MiddleCenter;
            //
            // btnVentas
            //
            ConfigurarBotonMenu(this.btnVentas, "Ventas", 150);
            this.btnVentas.Click += this.btnVentas_Click;
            //
            // btnProductos
            //
            ConfigurarBotonMenu(this.btnProductos, "Productos", 200);
            this.btnProductos.Click += this.btnProductos_Click;
            //
            // btnCaja
            //
            ConfigurarBotonMenu(this.btnCaja, "Caja", 250);
            this.btnCaja.Click += this.btnCaja_Click;
            //
            // btnCerrarSesion
            //
            this.btnCerrarSesion.BackColor = Color.FromArgb(28, 28, 30);
            this.btnCerrarSesion.FlatStyle = FlatStyle.Flat;
            this.btnCerrarSesion.FlatAppearance.BorderSize = 0;
            this.btnCerrarSesion.FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 58, 60);
            this.btnCerrarSesion.Font = new Font("Segoe UI", 10.5F);
            this.btnCerrarSesion.ForeColor = Color.FromArgb(199, 199, 204);
            this.btnCerrarSesion.Dock = DockStyle.Bottom;
            this.btnCerrarSesion.Height = 52;
            this.btnCerrarSesion.Text = "Cerrar sesión";
            this.btnCerrarSesion.TextAlign = ContentAlignment.MiddleCenter;
            this.btnCerrarSesion.Cursor = Cursors.Hand;
            this.btnCerrarSesion.UseVisualStyleBackColor = false;
            this.btnCerrarSesion.Click += this.btnCerrarSesion_Click;
            //
            // pnlTop
            //
            this.pnlTop.BackColor = Color.White;
            this.pnlTop.Controls.Add(this.lblTituloSeccion);
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Height = 60;
            //
            // lblTituloSeccion
            //
            this.lblTituloSeccion.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
            this.lblTituloSeccion.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblTituloSeccion.Location = new Point(24, 0);
            this.lblTituloSeccion.Size = new Size(500, 60);
            this.lblTituloSeccion.TextAlign = ContentAlignment.MiddleLeft;
            this.lblTituloSeccion.Text = "Bienvenido";
            //
            // pnlContenido
            //
            this.pnlContenido.BackColor = Color.FromArgb(250, 250, 249);
            this.pnlContenido.Dock = DockStyle.Fill;
            //
            // FormPrincipal
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(1000, 620);
            this.Controls.Add(this.pnlContenido);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlSidebar);
            this.Name = "FormPrincipal";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Sistema POS";
            this.WindowState = FormWindowState.Maximized;
            this.Load += this.FormPrincipal_Load;
            this.FormClosed += this.FormPrincipal_FormClosed;
            this.pnlSidebar.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void ConfigurarBotonMenu(Button boton, string texto, int top)
        {
            boton.BackColor = Color.FromArgb(28, 28, 30);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 58, 60);
            boton.Font = new Font("Segoe UI", 10.5F);
            boton.ForeColor = Color.FromArgb(199, 199, 204);
            boton.Location = new Point(0, top);
            boton.Size = new Size(230, 48);
            boton.Text = "   " + texto;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
        }
    }
}

using System;
using System.Windows.Forms;
using System.Drawing;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    /// <summary>
    /// Diálogo modal que solicita la contraseña de un administrador.
    /// Devuelve el Usuario autenticado si fue correcto, null si se canceló.
    /// </summary>
    public class FormVerificarAdmin : Form
    {
        private readonly UsuarioService usuarioService = new UsuarioService();
        private Label   lblMensaje;
        private Label   lblUsuario;
        private TextBox txtUsuario;
        private Label   lblPass;
        private TextBox txtPass;
        private Label   lblError;
        private Button  btnConfirmar;
        private Button  btnCancelar;

        public Usuario AdminVerificado { get; private set; }

        public FormVerificarAdmin(string motivo)
        {
            InitUI(motivo);
        }

        private void InitUI(string motivo)
        {
            this.Text = "Verificación de administrador";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(460, 520);
            this.BackColor = Color.White;

            int y = 20;

            var ico = new Label { Text = "🔒", Font = new Font("Segoe UI", 26F),
                Location = new Point(0, y), Size = new Size(460, 52),
                TextAlign = ContentAlignment.MiddleCenter };
            y += 62;

            var lblTit = new Label { Text = "Se requiere autorización",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15,15,17), AutoSize = false,
                Size = new Size(420, 30), Location = new Point(20, y),
                TextAlign = ContentAlignment.MiddleCenter };
            y += 38;

            lblMensaje = new Label { Text = motivo,
                Font = new Font("Segoe UI", 10.5F), ForeColor = Color.FromArgb(80,80,85),
                AutoSize = false, Size = new Size(420, 52), Location = new Point(20, y),
                TextAlign = ContentAlignment.MiddleCenter };
            y += 62;

            lblUsuario = new Label { Text = "Usuario administrador",
                Font = new Font("Segoe UI", 10.5F), ForeColor = Color.FromArgb(50,50,55),
                AutoSize = true, Location = new Point(30, y) };
            y += 26;

            txtUsuario = new TextBox { Font = new Font("Segoe UI", 13F),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(30, y), Size = new Size(398, 40),
                ForeColor = Color.FromArgb(15,15,17) };
            y += 52;

            lblPass = new Label { Text = "Contraseña",
                Font = new Font("Segoe UI", 10.5F), ForeColor = Color.FromArgb(50,50,55),
                AutoSize = true, Location = new Point(30, y) };
            y += 26;

            txtPass = new TextBox { Font = new Font("Segoe UI", 13F),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true,
                Location = new Point(30, y), Size = new Size(398, 40),
                ForeColor = Color.FromArgb(15,15,17) };
            y += 54;

            lblError = new Label { Text = "Usuario o contraseña incorrectos.",
                Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(180,30,30),
                AutoSize = true, Location = new Point(30, y), Visible = false };
            y += 30;

            btnConfirmar = new Button { Text = "Confirmar",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(15,15,17), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Location = new Point(30, y), Size = new Size(194, 46) };
            btnConfirmar.FlatAppearance.BorderSize = 0;

            btnCancelar = new Button { Text = "Cancelar",
                Font = new Font("Segoe UI", 12F),
                BackColor = Color.White, ForeColor = Color.FromArgb(60,60,65),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Location = new Point(234, y), Size = new Size(194, 46) };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(200,200,200);

            btnConfirmar.Click += (s, e) => Confirmar();
            btnCancelar.Click  += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            txtPass.KeyDown    += (s, e) => { if (e.KeyCode == Keys.Enter) Confirmar(); };

            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                ico, lblTit, lblMensaje, lblUsuario, txtUsuario,
                lblPass, txtPass, lblError, btnConfirmar, btnCancelar });
        }

        private void Confirmar()
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPass.Text;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            { lblError.Text = "Ingresa usuario y contraseña."; lblError.Visible = true; return; }

            var admin = usuarioService.Login(user, pass);
            if (admin == null || admin.Rol != RolUsuario.Admin)
            { lblError.Visible = true; lblError.Text = "Usuario o contraseña incorrectos."; txtPass.Clear(); return; }

            AdminVerificado = admin;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

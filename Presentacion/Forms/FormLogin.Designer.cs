using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlCentro;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblUsuario;
        private TextBox txtUsuario;
        private Label lblPassword;
        private TextBox txtPass;
        private Button btnIngresar;
        private Label lblError;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlCentro = new Panel();
            this.lblTitulo = new Label();
            this.lblSubtitulo = new Label();
            this.lblUsuario = new Label();
            this.txtUsuario = new TextBox();
            this.lblPassword = new Label();
            this.txtPass = new TextBox();
            this.btnIngresar = new Button();
            this.lblError = new Label();
            this.pnlCentro.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlCentro
            //
            this.pnlCentro.BackColor = Color.White;
            this.pnlCentro.BorderStyle = BorderStyle.FixedSingle;
            this.pnlCentro.Controls.Add(this.lblTitulo);
            this.pnlCentro.Controls.Add(this.lblSubtitulo);
            this.pnlCentro.Controls.Add(this.lblUsuario);
            this.pnlCentro.Controls.Add(this.txtUsuario);
            this.pnlCentro.Controls.Add(this.lblPassword);
            this.pnlCentro.Controls.Add(this.txtPass);
            this.pnlCentro.Controls.Add(this.btnIngresar);
            this.pnlCentro.Controls.Add(this.lblError);
            this.pnlCentro.Anchor = AnchorStyles.None;
            this.pnlCentro.Location = new Point(150, 75);
            this.pnlCentro.Name = "pnlCentro";
            this.pnlCentro.Size = new Size(340, 330);
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = false;
            this.lblTitulo.Font = new Font("Segoe UI Semibold", 19F, FontStyle.Bold);
            this.lblTitulo.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblTitulo.Location = new Point(0, 36);
            this.lblTitulo.Size = new Size(338, 36);
            this.lblTitulo.Text = "Sistema POS";
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblSubtitulo
            //
            this.lblSubtitulo.AutoSize = false;
            this.lblSubtitulo.Font = new Font("Segoe UI", 9.5F);
            this.lblSubtitulo.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblSubtitulo.Location = new Point(0, 72);
            this.lblSubtitulo.Size = new Size(338, 20);
            this.lblSubtitulo.Text = "Inicia sesión para continuar";
            this.lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblUsuario
            //
            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font = new Font("Segoe UI", 9.5F);
            this.lblUsuario.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblUsuario.Location = new Point(34, 108);
            this.lblUsuario.Text = "Usuario";
            //
            // txtUsuario
            //
            this.txtUsuario.BorderStyle = BorderStyle.FixedSingle;
            this.txtUsuario.Font = new Font("Segoe UI", 11F);
            this.txtUsuario.Location = new Point(34, 130);
            this.txtUsuario.Size = new Size(272, 27);
            //
            // lblPassword
            //
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new Font("Segoe UI", 9.5F);
            this.lblPassword.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblPassword.Location = new Point(34, 168);
            this.lblPassword.Text = "Contraseña";
            //
            // txtPass
            //
            this.txtPass.BorderStyle = BorderStyle.FixedSingle;
            this.txtPass.Font = new Font("Segoe UI", 11F);
            this.txtPass.Location = new Point(34, 190);
            this.txtPass.Size = new Size(272, 27);
            this.txtPass.UseSystemPasswordChar = true;
            this.txtPass.KeyDown += this.txtPass_KeyDown;
            //
            // btnIngresar
            //
            this.btnIngresar.BackColor = Color.FromArgb(28, 28, 30);
            this.btnIngresar.FlatStyle = FlatStyle.Flat;
            this.btnIngresar.FlatAppearance.BorderSize = 0;
            this.btnIngresar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnIngresar.ForeColor = Color.White;
            this.btnIngresar.Cursor = Cursors.Hand;
            this.btnIngresar.Location = new Point(34, 235);
            this.btnIngresar.Size = new Size(272, 42);
            this.btnIngresar.Text = "Ingresar";
            this.btnIngresar.UseVisualStyleBackColor = false;
            this.btnIngresar.Click += this.btnIngresar_Click;
            //
            // lblError
            //
            this.lblError.ForeColor = Color.FromArgb(163, 45, 45);
            this.lblError.Font = new Font("Segoe UI", 9F);
            this.lblError.Location = new Point(34, 285);
            this.lblError.Size = new Size(272, 35);
            this.lblError.TextAlign = ContentAlignment.MiddleCenter;
            this.lblError.Visible = false;
            //
            // FormLogin
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.FromArgb(250, 250, 249);
            this.ClientSize = new Size(640, 480);
            this.Controls.Add(this.pnlCentro);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormLogin";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "POS - Inicio de sesión";
            this.pnlCentro.ResumeLayout(false);
            this.pnlCentro.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

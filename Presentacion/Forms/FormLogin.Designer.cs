using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;
        private Panel   pnlFondo;
        private Panel   pnlCard;
        private Panel   pnlLogoWrap;
        private Label   lblLogoTxt;
        private Label   lblTitulo;
        private Label   lblSubtitulo;
        private Label   lblUsuario;
        private TextBox txtUsuario;
        private Label   lblPassword;
        private TextBox txtPass;
        private Button  btnIngresar;
        private Label   lblError;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlFondo    = new Panel();
            this.pnlCard     = new Panel();
            this.pnlLogoWrap = new Panel();
            this.lblLogoTxt  = new Label();
            this.lblTitulo   = new Label();
            this.lblSubtitulo= new Label();
            this.lblUsuario  = new Label();
            this.txtUsuario  = new TextBox();
            this.lblPassword = new Label();
            this.txtPass     = new TextBox();
            this.btnIngresar = new Button();
            this.lblError    = new Label();
            this.pnlFondo.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.pnlLogoWrap.SuspendLayout();
            this.SuspendLayout();

            // ── Fondo ─────────────────────────────────────────────────
            this.pnlFondo.BackColor = System.Drawing.Color.FromArgb(245, 244, 241);
            this.pnlFondo.Dock      = DockStyle.Fill;
            this.pnlFondo.Controls.Add(this.pnlCard);

            // ── Card centrada ─────────────────────────────────────────
            this.pnlCard.BackColor   = System.Drawing.Color.White;
            this.pnlCard.BorderStyle = BorderStyle.FixedSingle;
            this.pnlCard.Anchor      = AnchorStyles.None;
            this.pnlCard.Size        = new Size(380, 440);
            this.pnlCard.Controls.AddRange(new System.Windows.Forms.Control[] {
                pnlLogoWrap, lblTitulo, lblSubtitulo,
                lblUsuario, txtUsuario, lblPassword, txtPass,
                btnIngresar, lblError });

            // Logo cuadrado
            this.pnlLogoWrap.BackColor  = System.Drawing.Color.FromArgb(14, 14, 18);
            this.pnlLogoWrap.Location   = new Point(166, 28);
            this.pnlLogoWrap.Size       = new Size(46, 46);
            this.pnlLogoWrap.Controls.Add(this.lblLogoTxt);

            this.lblLogoTxt.AutoSize  = false;
            this.lblLogoTxt.Text      = "P";
            this.lblLogoTxt.Font      = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblLogoTxt.ForeColor = Color.White;
            this.lblLogoTxt.Dock      = DockStyle.Fill;
            this.lblLogoTxt.TextAlign = ContentAlignment.MiddleCenter;

            // Título
            this.lblTitulo.AutoSize  = false;
            this.lblTitulo.Font      = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(14, 14, 18);
            this.lblTitulo.Location  = new Point(0, 90);
            this.lblTitulo.Size      = new Size(378, 36);
            this.lblTitulo.Text      = "Sistema POS";
            this.lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            // Subtítulo
            this.lblSubtitulo.AutoSize  = false;
            this.lblSubtitulo.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(143, 143, 154);
            this.lblSubtitulo.Location  = new Point(0, 126);
            this.lblSubtitulo.Size      = new Size(378, 20);
            this.lblSubtitulo.Text      = "Inicia sesión para continuar";
            this.lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;

            // lblUsuario
            this.lblUsuario.AutoSize  = true;
            this.lblUsuario.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUsuario.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.lblUsuario.Location  = new System.Drawing.Point(36, 162);
            this.lblUsuario.Name      = "lblUsuario";
            this.lblUsuario.Text      = "USUARIO";
            // txtUsuario
            this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsuario.Font        = new System.Drawing.Font("Segoe UI", 12F);
            this.txtUsuario.Location    = new System.Drawing.Point(36, 184);
            this.txtUsuario.Name        = "txtUsuario";
            this.txtUsuario.Size        = new System.Drawing.Size(306, 38);
            // lblPassword
            this.lblPassword.AutoSize  = true;
            this.lblPassword.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.lblPassword.Location  = new System.Drawing.Point(36, 234);
            this.lblPassword.Name      = "lblPassword";
            this.lblPassword.Text      = "CONTRASEÑA";
            // txtPass
            this.txtPass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPass.Font        = new System.Drawing.Font("Segoe UI", 12F);
            this.txtPass.Location    = new System.Drawing.Point(36, 256);
            this.txtPass.Name        = "txtPass";
            this.txtPass.Size        = new System.Drawing.Size(306, 38);
            this.txtPass.UseSystemPasswordChar = true;
            this.txtPass.KeyDown    += new System.Windows.Forms.KeyEventHandler(this.txtPass_KeyDown);

            // Botón ingresar
            this.btnIngresar.BackColor = System.Drawing.Color.FromArgb(14, 14, 18);
            this.btnIngresar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIngresar.FlatAppearance.BorderSize = 0;
            this.btnIngresar.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnIngresar.ForeColor = System.Drawing.Color.White;
            this.btnIngresar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnIngresar.Location  = new System.Drawing.Point(36, 316);
            this.btnIngresar.Name      = "btnIngresar";
            this.btnIngresar.Size      = new System.Drawing.Size(306, 50);
            this.btnIngresar.Text      = "Ingresar al sistema";
            this.btnIngresar.UseVisualStyleBackColor = false;
            this.btnIngresar.Click    += new System.EventHandler(this.btnIngresar_Click);

            // Error
            this.lblError.AutoSize  = false;
            this.lblError.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.lblError.Location  = new System.Drawing.Point(36, 374);
            this.lblError.Name      = "lblError";
            this.lblError.Size      = new System.Drawing.Size(306, 36);
            this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblError.Visible   = false;

            // ── FormLogin ─────────────────────────────────────────────
            this.AutoScaleMode   = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize      = new System.Drawing.Size(760, 630);
            this.Controls.Add(this.pnlFondo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "FormLogin";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "POS — Inicio de sesión";
            this.pnlFondo.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.pnlLogoWrap.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

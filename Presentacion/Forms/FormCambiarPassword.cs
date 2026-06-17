using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    /// <summary>
    /// Diálogo modal con campos de contraseña enmascarados. NO toca la base de datos:
    /// recoge los valores y valida en pantalla (no vacíos y que coincidan). El llamador
    /// ejecuta el servicio que corresponda (cambio propio, forzado o reseteo por admin).
    /// </summary>
    public class FormCambiarPassword : Form
    {
        private readonly bool pedirActual;
        private TextBox txtActual;
        private TextBox txtNueva;
        private TextBox txtConfirmar;
        private Label   lblError;

        public string PasswordActual { get { return txtActual != null ? txtActual.Text : ""; } }
        public string PasswordNueva  { get { return txtNueva.Text; } }

        public FormCambiarPassword(string titulo, string subtitulo, bool pedirActual, bool obligatorio)
        {
            this.pedirActual = pedirActual;
            InitUI(titulo, subtitulo, obligatorio);
        }

        private void InitUI(string titulo, string subtitulo, bool obligatorio)
        {
            const int ancho = 460;
            this.Text            = titulo;
            this.FormBorderStyle  = FormBorderStyle.FixedDialog;
            this.MaximizeBox      = false;
            this.MinimizeBox      = false;
            this.StartPosition    = FormStartPosition.CenterParent;
            this.BackColor        = Color.White;
            this.ControlBox       = !obligatorio;   // si es obligatorio, sin botón de cierre en la barra

            int y = 20;

            var ico = new Label { Text = "🔑", Font = new Font("Segoe UI", 26F),
                Location = new Point(0, y), Size = new Size(ancho, 52),
                TextAlign = ContentAlignment.MiddleCenter };
            y += 60;

            var lblTit = new Label { Text = titulo, Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 15, 17), AutoSize = false,
                Size = new Size(ancho - 40, 28), Location = new Point(20, y),
                TextAlign = ContentAlignment.MiddleCenter };
            y += 34;

            var lblSub = new Label { Text = subtitulo, Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(80, 80, 85), AutoSize = false,
                Size = new Size(ancho - 40, 44), Location = new Point(20, y),
                TextAlign = ContentAlignment.MiddleCenter };
            y += 52;

            this.Controls.Add(ico);
            this.Controls.Add(lblTit);
            this.Controls.Add(lblSub);

            if (pedirActual)
                y = AgregarCampo("Contraseña actual", out txtActual, y, ancho);
            y = AgregarCampo("Nueva contraseña", out txtNueva, y, ancho);
            y = AgregarCampo("Repetir nueva contraseña", out txtConfirmar, y, ancho);

            lblError = new Label { Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(180, 30, 30),
                AutoSize = false, Size = new Size(ancho - 60, 38), Location = new Point(30, y), Visible = false };
            y += 44;

            var btnConfirmar = new Button { Text = "Guardar",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(15, 15, 17), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Location = new Point(30, y), Size = new Size(194, 46) };
            btnConfirmar.FlatAppearance.BorderSize = 0;

            var btnCancelar = new Button { Text = "Cancelar",
                Font = new Font("Segoe UI", 12F),
                BackColor = Color.White, ForeColor = Color.FromArgb(60, 60, 65),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Location = new Point(234, y), Size = new Size(194, 46) };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            y += 60;

            btnConfirmar.Click  += (s, e) => Confirmar();
            btnCancelar.Click   += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            txtConfirmar.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Confirmar(); };

            this.Controls.Add(lblError);
            this.Controls.Add(btnConfirmar);
            this.Controls.Add(btnCancelar);

            this.ClientSize = new Size(ancho, y + 12);
        }

        private int AgregarCampo(string etiqueta, out TextBox txt, int y, int ancho)
        {
            var lbl = new Label { Text = etiqueta, Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(50, 50, 55), AutoSize = true, Location = new Point(30, y) };
            y += 24;
            txt = new TextBox { Font = new Font("Segoe UI", 13F), BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true, Location = new Point(30, y), Size = new Size(ancho - 62, 40),
                ForeColor = Color.FromArgb(15, 15, 17) };
            y += 50;
            this.Controls.Add(lbl);
            this.Controls.Add(txt);
            return y;
        }

        private void Confirmar()
        {
            if (pedirActual && string.IsNullOrEmpty(txtActual.Text))
            { MostrarError("Ingresa tu contraseña actual."); return; }
            if (string.IsNullOrEmpty(txtNueva.Text))
            { MostrarError("Ingresa la nueva contraseña."); return; }
            if (txtNueva.Text != txtConfirmar.Text)
            { MostrarError("Las contraseñas no coinciden."); txtConfirmar.SelectAll(); txtConfirmar.Focus(); return; }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void MostrarError(string msg) { lblError.Text = msg; lblError.Visible = true; }
    }
}

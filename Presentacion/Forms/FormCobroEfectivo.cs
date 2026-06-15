using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    /// <summary>Diálogo de cobro en efectivo: ingresa con cuánto paga el cliente y calcula el vuelto.</summary>
    public class FormCobroEfectivo : Form
    {
        public decimal Vuelto { get; private set; }

        private readonly decimal total;
        private TextBox txtRecibido;
        private Label lblVuelto;
        private Button btnConfirmar;

        public FormCobroEfectivo(decimal total)
        {
            this.total = total;
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "Cobro en efectivo";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.ShowIcon = false; this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(420, 430);
            this.BackColor = EstiloPos.Surface;

            const int x = 28, ancho = 364;

            var lblTotalTit = new Label { Text = "TOTAL A PAGAR", Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink3,
                AutoSize = true, Location = new Point(x, 22) };
            var lblTotal = new Label { Text = "$" + total.ToString("N0"),
                Font = new Font("Segoe UI", 30F, FontStyle.Bold), ForeColor = EstiloPos.Ink1,
                AutoSize = false, Size = new Size(ancho, 52), Location = new Point(x, 42),
                TextAlign = ContentAlignment.MiddleLeft };

            var lblPaga = new Label { Text = "Paga con", Font = EstiloPos.FontLabel, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(x, 108) };
            txtRecibido = new TextBox { Font = new Font("Segoe UI", 18F), BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(x, 132), Size = new Size(ancho, 44), TextAlign = HorizontalAlignment.Right,
                ForeColor = EstiloPos.Ink1 };

            // Montos rápidos
            string[] labels  = { "Exacto", "$5.000", "$10.000", "$20.000" };
            decimal[] valores = { total, 5000m, 10000m, 20000m };
            int bx = x, by = 186;
            for (int i = 0; i < labels.Length; i++)
            {
                decimal monto = valores[i];
                var b = new Button { Text = labels[i], Size = new Size(86, 36), Location = new Point(bx, by),
                    FlatStyle = FlatStyle.Flat, Font = EstiloPos.FontSmall, BackColor = EstiloPos.Surface,
                    ForeColor = EstiloPos.Ink2, Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
                b.FlatAppearance.BorderColor = EstiloPos.Border;
                b.FlatAppearance.MouseOverBackColor = EstiloPos.Hover;
                b.Click += (s, e) => { txtRecibido.Text = ((long)monto).ToString(); txtRecibido.Focus(); };
                this.Controls.Add(b);
                bx += 90;
            }

            var lblVueltoTit = new Label { Text = "VUELTO", Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink3,
                AutoSize = true, Location = new Point(x, 244) };
            lblVuelto = new Label { Text = "$0", Font = new Font("Segoe UI", 30F, FontStyle.Bold), ForeColor = EstiloPos.Verde,
                AutoSize = false, Size = new Size(ancho, 52), Location = new Point(x, 264),
                TextAlign = ContentAlignment.MiddleLeft };

            btnConfirmar = new Button { Text = "Confirmar cobro", Size = new Size(232, 46), Location = new Point(x, 336) };
            EstiloPos.AplicarBotonPrimario(btnConfirmar, grande: true);
            btnConfirmar.Click += (s, e) => Confirmar();

            var btnCancelar = new Button { Text = "Cancelar", Size = new Size(122, 46), Location = new Point(x + 242, 336) };
            EstiloPos.AplicarBotonSecundario(btnCancelar);
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            txtRecibido.KeyPress += (s, e) =>
            { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            txtRecibido.TextChanged += (s, e) => Calcular();
            txtRecibido.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { Confirmar(); e.SuppressKeyPress = true; } };

            this.Controls.AddRange(new Control[] {
                lblTotalTit, lblTotal, lblPaga, txtRecibido, lblVueltoTit, lblVuelto, btnConfirmar, btnCancelar });
            this.AcceptButton = btnConfirmar;
            this.CancelButton = btnCancelar;

            Calcular();
            this.Shown += (s, e) => txtRecibido.Focus();
        }

        private void Calcular()
        {
            decimal.TryParse(txtRecibido.Text, out decimal recibido);
            decimal v = recibido - total;
            if (v >= 0)
            {
                lblVuelto.Text = "$" + v.ToString("N0");
                lblVuelto.ForeColor = EstiloPos.Verde;
                btnConfirmar.Enabled = true;
            }
            else
            {
                lblVuelto.Text = "Falta $" + Math.Abs(v).ToString("N0");
                lblVuelto.ForeColor = EstiloPos.Rojo;
                btnConfirmar.Enabled = false;
            }
        }

        private void Confirmar()
        {
            if (!decimal.TryParse(txtRecibido.Text, out decimal recibido) || recibido < total) return;
            Vuelto = recibido - total;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

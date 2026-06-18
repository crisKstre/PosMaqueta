using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Entidades;

namespace Presentacion.Forms
{
    /// <summary>
    /// Cobro con PAGO MIXTO: el cajero reparte el total entre tarjeta, transferencia y efectivo.
    /// El efectivo es el resto; si el cliente paga ese resto con más efectivo, calcula el vuelto.
    /// Devuelve la lista de pagos (cada uno &gt; 0) cuya suma es el total.
    /// </summary>
    public class FormCobroMixto : Form
    {
        public List<PagoVenta> Pagos { get; private set; } = new List<PagoVenta>();
        public decimal Vuelto { get; private set; }

        private readonly decimal total;
        private TextBox txtTarjeta, txtTransferencia, txtRecibido;
        private Label lblEfectivo, lblRecibidoTit, lblEstado;
        private Button btnConfirmar;

        public FormCobroMixto(decimal total)
        {
            this.total = total;
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "Pago mixto";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.ShowIcon = false; this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(440, 478);
            this.BackColor = EstiloPos.Surface;

            const int x = 28, ancho = 384;

            var lblTotalTit = new Label { Text = "TOTAL A PAGAR", Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink3,
                AutoSize = true, Location = new Point(x, 20) };
            var lblTotal = new Label { Text = "$" + total.ToString("N0"),
                Font = new Font("Segoe UI", 28F, FontStyle.Bold), ForeColor = EstiloPos.Ink1,
                AutoSize = false, Size = new Size(ancho, 48), Location = new Point(x, 40),
                TextAlign = ContentAlignment.MiddleLeft };

            int y = 100;
            txtTarjeta       = Campo("Tarjeta", x, ref y, ancho);
            txtTransferencia = Campo("Transferencia", x, ref y, ancho);

            var lblEfecTit = new Label { Text = "Efectivo (resto)", Font = EstiloPos.FontLabel, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(x, y) };
            lblEfectivo = new Label { Text = "$0", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = EstiloPos.Ink1,
                AutoSize = false, Size = new Size(ancho, 30), Location = new Point(x, y + 22) };
            y += 60;

            lblRecibidoTit = new Label { Text = "Paga el efectivo con", Font = EstiloPos.FontLabel, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(x, y) };
            txtRecibido = new TextBox { Font = new Font("Segoe UI", 15F), BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(x, y + 24), Size = new Size(ancho, 40), TextAlign = HorizontalAlignment.Right,
                ForeColor = EstiloPos.Ink1 };
            y += 78;

            lblEstado = new Label { Text = "", Font = new Font("Segoe UI", 13F, FontStyle.Bold), AutoSize = false,
                Size = new Size(ancho, 28), Location = new Point(x, y) };
            y += 38;

            btnConfirmar = new Button { Text = "Confirmar cobro", Size = new Size(252, 46), Location = new Point(x, y) };
            EstiloPos.AplicarBotonPrimario(btnConfirmar, grande: true);
            btnConfirmar.Click += (s, e) => Confirmar();
            var btnCancelar = new Button { Text = "Cancelar", Size = new Size(122, 46), Location = new Point(x + 262, y) };
            EstiloPos.AplicarBotonSecundario(btnCancelar);
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            foreach (var t in new[] { txtTarjeta, txtTransferencia, txtRecibido })
            {
                t.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
                t.TextChanged += (s, e) => Calcular();
            }

            this.Controls.AddRange(new Control[] { lblTotalTit, lblTotal, lblEfecTit, lblEfectivo,
                lblRecibidoTit, txtRecibido, lblEstado, btnConfirmar, btnCancelar });
            this.AcceptButton = btnConfirmar;
            this.CancelButton = btnCancelar;

            Calcular();
        }

        private TextBox Campo(string etiqueta, int x, ref int y, int ancho)
        {
            var lbl = new Label { Text = etiqueta, Font = EstiloPos.FontLabel, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(x, y) };
            var txt = new TextBox { Font = new Font("Segoe UI", 15F), BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(x, y + 24), Size = new Size(ancho, 40), TextAlign = HorizontalAlignment.Right,
                ForeColor = EstiloPos.Ink1 };
            this.Controls.Add(lbl);
            this.Controls.Add(txt);
            y += 72;
            return txt;
        }

        private static decimal Parse(TextBox t) { decimal.TryParse(t.Text, out var v); return v < 0 ? 0 : v; }

        private void Calcular()
        {
            decimal tarjeta = Parse(txtTarjeta);
            decimal transf  = Parse(txtTransferencia);
            decimal noEfectivo = tarjeta + transf;
            decimal resto = total - noEfectivo;

            bool excede = noEfectivo > total;
            lblEfectivo.Text = excede ? "—" : "$" + resto.ToString("N0");

            bool hayEfectivo = !excede && resto > 0;
            lblRecibidoTit.Visible = hayEfectivo;
            txtRecibido.Visible    = hayEfectivo;

            if (excede)
            {
                lblEstado.Text = "Tarjeta + transferencia exceden el total.";
                lblEstado.ForeColor = EstiloPos.Rojo;
                btnConfirmar.Enabled = false;
                return;
            }

            if (resto == 0)   // todo cubierto sin efectivo
            {
                Vuelto = 0;
                lblEstado.Text = "Pago cubierto.";
                lblEstado.ForeColor = EstiloPos.Verde;
                btnConfirmar.Enabled = true;
                return;
            }

            decimal recibido = Parse(txtRecibido);
            if (recibido >= resto)
            {
                Vuelto = recibido - resto;
                lblEstado.Text = Vuelto > 0 ? "Vuelto: $" + Vuelto.ToString("N0") : "Pago exacto.";
                lblEstado.ForeColor = EstiloPos.Verde;
                btnConfirmar.Enabled = true;
            }
            else
            {
                lblEstado.Text = "Falta efectivo: $" + (resto - recibido).ToString("N0");
                lblEstado.ForeColor = EstiloPos.Rojo;
                btnConfirmar.Enabled = false;
            }
        }

        private void Confirmar()
        {
            decimal tarjeta = Parse(txtTarjeta);
            decimal transf  = Parse(txtTransferencia);
            decimal resto   = total - (tarjeta + transf);
            if (resto < 0) return;

            var pagos = new List<PagoVenta>();
            if (resto   > 0) pagos.Add(new PagoVenta { MedioPago = MedioPago.Efectivo,      Monto = resto });
            if (tarjeta > 0) pagos.Add(new PagoVenta { MedioPago = MedioPago.Tarjeta,       Monto = tarjeta });
            if (transf  > 0) pagos.Add(new PagoVenta { MedioPago = MedioPago.Transferencia, Monto = transf });
            if (pagos.Count == 0) return;   // nada que cobrar

            Pagos = pagos;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

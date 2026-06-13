using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormCaja
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTituloEstado;
        private Label lblMensaje;

        private Panel pnlAbrir;
        private Label lblSinCaja;
        private Label lblMontoInicial;
        private TextBox txtMontoInicial;
        private Button btnAbrir;
        private Label lblPermisoAbrir;

        private Panel pnlAbierta;
        private Label lblBadge;
        private Label lblInfoApertura;
        private Panel cardVentas;
        private Label lblVentasTit;
        private Label lblVentasVal;
        private Panel cardTotal;
        private Label lblTotalTit;
        private Label lblTotalVal;
        private Panel cardEfectivo;
        private Label lblEfectivoTit;
        private Label lblEfectivoVal;
        private Label lblDesgloseTit;
        private Label lblDesgloseVal;
        private Label lblMontoReal;
        private TextBox txtMontoReal;
        private Button btnCerrar;
        private Label lblPermisoCerrar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTituloEstado = new Label();
            this.lblMensaje = new Label();
            this.pnlAbrir = new Panel();
            this.lblSinCaja = new Label();
            this.lblMontoInicial = new Label();
            this.txtMontoInicial = new TextBox();
            this.btnAbrir = new Button();
            this.lblPermisoAbrir = new Label();
            this.pnlAbierta = new Panel();
            this.lblBadge = new Label();
            this.lblInfoApertura = new Label();
            this.cardVentas = new Panel();
            this.lblVentasTit = new Label();
            this.lblVentasVal = new Label();
            this.cardTotal = new Panel();
            this.lblTotalTit = new Label();
            this.lblTotalVal = new Label();
            this.cardEfectivo = new Panel();
            this.lblEfectivoTit = new Label();
            this.lblEfectivoVal = new Label();
            this.lblDesgloseTit = new Label();
            this.lblDesgloseVal = new Label();
            this.lblMontoReal = new Label();
            this.txtMontoReal = new TextBox();
            this.btnCerrar = new Button();
            this.lblPermisoCerrar = new Label();
            this.pnlAbrir.SuspendLayout();
            this.pnlAbierta.SuspendLayout();
            this.cardVentas.SuspendLayout();
            this.cardTotal.SuspendLayout();
            this.cardEfectivo.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTituloEstado
            //
            this.lblTituloEstado.AutoSize = true;
            this.lblTituloEstado.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            this.lblTituloEstado.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblTituloEstado.Location = new Point(28, 24);
            this.lblTituloEstado.Text = "Caja";
            //
            // lblMensaje
            //
            this.lblMensaje.AutoSize = true;
            this.lblMensaje.Font = new Font("Segoe UI", 9.5F);
            this.lblMensaje.ForeColor = Color.FromArgb(163, 45, 45);
            this.lblMensaje.Location = new Point(28, 58);
            this.lblMensaje.Visible = false;
            //
            // ===================== Panel ABRIR =====================
            //
            this.pnlAbrir.BackColor = Color.White;
            this.pnlAbrir.BorderStyle = BorderStyle.FixedSingle;
            this.pnlAbrir.Controls.Add(this.lblSinCaja);
            this.pnlAbrir.Controls.Add(this.lblMontoInicial);
            this.pnlAbrir.Controls.Add(this.txtMontoInicial);
            this.pnlAbrir.Controls.Add(this.btnAbrir);
            this.pnlAbrir.Controls.Add(this.lblPermisoAbrir);
            this.pnlAbrir.Location = new Point(28, 90);
            this.pnlAbrir.Size = new Size(640, 200);
            //
            // lblSinCaja
            //
            this.lblSinCaja.AutoSize = true;
            this.lblSinCaja.Font = new Font("Segoe UI", 10F);
            this.lblSinCaja.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblSinCaja.Location = new Point(24, 24);
            this.lblSinCaja.Text = "No hay una caja abierta. Abre una para registrar las ventas del turno.";
            //
            // lblMontoInicial
            //
            this.lblMontoInicial.AutoSize = true;
            this.lblMontoInicial.Font = new Font("Segoe UI", 9F);
            this.lblMontoInicial.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblMontoInicial.Location = new Point(24, 70);
            this.lblMontoInicial.Text = "Monto inicial (fondo de caja)";
            //
            // txtMontoInicial
            //
            this.txtMontoInicial.BorderStyle = BorderStyle.FixedSingle;
            this.txtMontoInicial.Font = new Font("Segoe UI", 12F);
            this.txtMontoInicial.Location = new Point(24, 92);
            this.txtMontoInicial.Size = new Size(200, 30);
            //
            // btnAbrir
            //
            this.btnAbrir.BackColor = Color.FromArgb(28, 28, 30);
            this.btnAbrir.FlatStyle = FlatStyle.Flat;
            this.btnAbrir.FlatAppearance.BorderSize = 0;
            this.btnAbrir.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnAbrir.ForeColor = Color.White;
            this.btnAbrir.Cursor = Cursors.Hand;
            this.btnAbrir.Location = new Point(240, 90);
            this.btnAbrir.Size = new Size(140, 34);
            this.btnAbrir.Text = "Abrir caja";
            this.btnAbrir.UseVisualStyleBackColor = false;
            this.btnAbrir.Click += this.btnAbrir_Click;
            //
            // lblPermisoAbrir
            //
            this.lblPermisoAbrir.AutoSize = true;
            this.lblPermisoAbrir.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            this.lblPermisoAbrir.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblPermisoAbrir.Location = new Point(24, 140);
            this.lblPermisoAbrir.Text = "Solo un administrador puede abrir la caja.";
            this.lblPermisoAbrir.Visible = false;
            //
            // ===================== Panel ABIERTA =====================
            //
            this.pnlAbierta.BackColor = Color.White;
            this.pnlAbierta.BorderStyle = BorderStyle.FixedSingle;
            this.pnlAbierta.Controls.Add(this.lblBadge);
            this.pnlAbierta.Controls.Add(this.lblInfoApertura);
            this.pnlAbierta.Controls.Add(this.cardVentas);
            this.pnlAbierta.Controls.Add(this.cardTotal);
            this.pnlAbierta.Controls.Add(this.cardEfectivo);
            this.pnlAbierta.Controls.Add(this.lblDesgloseTit);
            this.pnlAbierta.Controls.Add(this.lblDesgloseVal);
            this.pnlAbierta.Controls.Add(this.lblMontoReal);
            this.pnlAbierta.Controls.Add(this.txtMontoReal);
            this.pnlAbierta.Controls.Add(this.btnCerrar);
            this.pnlAbierta.Controls.Add(this.lblPermisoCerrar);
            this.pnlAbierta.Location = new Point(28, 90);
            this.pnlAbierta.Size = new Size(700, 340);
            this.pnlAbierta.Visible = false;
            //
            // lblBadge
            //
            this.lblBadge.AutoSize = true;
            this.lblBadge.BackColor = Color.FromArgb(234, 243, 222);
            this.lblBadge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblBadge.ForeColor = Color.FromArgb(59, 109, 17);
            this.lblBadge.Location = new Point(24, 22);
            this.lblBadge.Padding = new Padding(8, 4, 8, 4);
            this.lblBadge.Text = "● Abierta";
            //
            // lblInfoApertura
            //
            this.lblInfoApertura.AutoSize = true;
            this.lblInfoApertura.Font = new Font("Segoe UI", 9.5F);
            this.lblInfoApertura.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblInfoApertura.Location = new Point(120, 26);
            this.lblInfoApertura.Text = "Abierta ...";
            //
            // cardVentas
            //
            this.cardVentas.BackColor = Color.FromArgb(250, 250, 249);
            this.cardVentas.Controls.Add(this.lblVentasTit);
            this.cardVentas.Controls.Add(this.lblVentasVal);
            this.cardVentas.Location = new Point(24, 64);
            this.cardVentas.Size = new Size(200, 80);
            //
            // lblVentasTit
            //
            this.lblVentasTit.AutoSize = true;
            this.lblVentasTit.Font = new Font("Segoe UI", 9F);
            this.lblVentasTit.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblVentasTit.Location = new Point(14, 12);
            this.lblVentasTit.Text = "Ventas del turno";
            //
            // lblVentasVal
            //
            this.lblVentasVal.AutoSize = true;
            this.lblVentasVal.Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
            this.lblVentasVal.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblVentasVal.Location = new Point(12, 34);
            this.lblVentasVal.Text = "0";
            //
            // cardTotal
            //
            this.cardTotal.BackColor = Color.FromArgb(250, 250, 249);
            this.cardTotal.Controls.Add(this.lblTotalTit);
            this.cardTotal.Controls.Add(this.lblTotalVal);
            this.cardTotal.Location = new Point(240, 64);
            this.cardTotal.Size = new Size(200, 80);
            //
            // lblTotalTit
            //
            this.lblTotalTit.AutoSize = true;
            this.lblTotalTit.Font = new Font("Segoe UI", 9F);
            this.lblTotalTit.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblTotalTit.Location = new Point(14, 12);
            this.lblTotalTit.Text = "Total vendido";
            //
            // lblTotalVal
            //
            this.lblTotalVal.AutoSize = true;
            this.lblTotalVal.Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
            this.lblTotalVal.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblTotalVal.Location = new Point(12, 34);
            this.lblTotalVal.Text = "$0";
            //
            // cardEfectivo
            //
            this.cardEfectivo.BackColor = Color.FromArgb(250, 250, 249);
            this.cardEfectivo.Controls.Add(this.lblEfectivoTit);
            this.cardEfectivo.Controls.Add(this.lblEfectivoVal);
            this.cardEfectivo.Location = new Point(456, 64);
            this.cardEfectivo.Size = new Size(200, 80);
            //
            // lblEfectivoTit
            //
            this.lblEfectivoTit.AutoSize = true;
            this.lblEfectivoTit.Font = new Font("Segoe UI", 9F);
            this.lblEfectivoTit.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblEfectivoTit.Location = new Point(14, 12);
            this.lblEfectivoTit.Text = "Efectivo esperado";
            //
            // lblEfectivoVal
            //
            this.lblEfectivoVal.AutoSize = true;
            this.lblEfectivoVal.Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
            this.lblEfectivoVal.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblEfectivoVal.Location = new Point(12, 34);
            this.lblEfectivoVal.Text = "$0";
            //
            // lblDesgloseTit
            //
            this.lblDesgloseTit.AutoSize = true;
            this.lblDesgloseTit.Font = new Font("Segoe UI", 9F);
            this.lblDesgloseTit.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblDesgloseTit.Location = new Point(24, 158);
            this.lblDesgloseTit.Text = "Desglose por medio de pago";
            //
            // lblDesgloseVal
            //
            this.lblDesgloseVal.AutoSize = true;
            this.lblDesgloseVal.Font = new Font("Segoe UI", 10F);
            this.lblDesgloseVal.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblDesgloseVal.Location = new Point(24, 178);
            this.lblDesgloseVal.Text = "Efectivo: $0    Tarjeta: $0    Transferencia: $0";
            //
            // lblMontoReal
            //
            this.lblMontoReal.AutoSize = true;
            this.lblMontoReal.Font = new Font("Segoe UI", 9F);
            this.lblMontoReal.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblMontoReal.Location = new Point(24, 232);
            this.lblMontoReal.Text = "Monto real contado (efectivo en caja al cerrar)";
            //
            // txtMontoReal
            //
            this.txtMontoReal.BorderStyle = BorderStyle.FixedSingle;
            this.txtMontoReal.Font = new Font("Segoe UI", 12F);
            this.txtMontoReal.Location = new Point(24, 254);
            this.txtMontoReal.Size = new Size(200, 30);
            //
            // btnCerrar
            //
            this.btnCerrar.BackColor = Color.FromArgb(28, 28, 30);
            this.btnCerrar.FlatStyle = FlatStyle.Flat;
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnCerrar.ForeColor = Color.White;
            this.btnCerrar.Cursor = Cursors.Hand;
            this.btnCerrar.Location = new Point(240, 252);
            this.btnCerrar.Size = new Size(140, 34);
            this.btnCerrar.Text = "Cerrar caja";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += this.btnCerrar_Click;
            //
            // lblPermisoCerrar
            //
            this.lblPermisoCerrar.AutoSize = true;
            this.lblPermisoCerrar.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            this.lblPermisoCerrar.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblPermisoCerrar.Location = new Point(24, 300);
            this.lblPermisoCerrar.Text = "Solo un administrador puede cerrar la caja.";
            this.lblPermisoCerrar.Visible = false;
            //
            // FormCaja
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.FromArgb(250, 250, 249);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.lblTituloEstado);
            this.Controls.Add(this.pnlAbrir);
            this.Controls.Add(this.pnlAbierta);
            this.Name = "FormCaja";
            this.Text = "Caja";
            this.Load += this.FormCaja_Load;
            this.FormClosed += this.FormCaja_FormClosed;
            this.pnlAbrir.ResumeLayout(false);
            this.pnlAbrir.PerformLayout();
            this.pnlAbierta.ResumeLayout(false);
            this.pnlAbierta.PerformLayout();
            this.cardVentas.ResumeLayout(false);
            this.cardVentas.PerformLayout();
            this.cardTotal.ResumeLayout(false);
            this.cardTotal.PerformLayout();
            this.cardEfectivo.ResumeLayout(false);
            this.cardEfectivo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

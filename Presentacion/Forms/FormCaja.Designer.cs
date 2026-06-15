using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormCaja
    {
        private System.ComponentModel.IContainer components = null;

        private Label   lblTituloEstado;
        private Label   lblMensaje;
        private Panel   pnlAbrir;
        private Label   lblSinCaja;
        private Label   lblMontoInicial;
        private TextBox txtMontoInicial;
        private Button  btnAbrir;
        private Label   lblPermisoAbrir;
        private Panel   pnlAbierta;
        private Label   lblBadge;
        private Label   lblInfoApertura;
        internal Panel  pnlCards;
        internal Panel  cardVentas;
        private Label   lblVentasTit;
        internal Label  lblVentasVal;
        internal Panel  cardTotal;
        private Label   lblTotalTit;
        internal Label  lblTotalVal;
        internal Panel  cardEfectivo;
        private Label   lblEfectivoTit;
        internal Label  lblEfectivoVal;
        private Label   lblDesgloseTit;
        internal Label  lblDesgloseVal;
        private Label   lblMontoReal;
        private TextBox txtMontoReal;
        private Button  btnCerrar;
        private Label   lblPermisoCerrar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTituloEstado  = new System.Windows.Forms.Label();
            this.lblMensaje       = new System.Windows.Forms.Label();
            this.pnlAbrir         = new System.Windows.Forms.Panel();
            this.lblSinCaja       = new System.Windows.Forms.Label();
            this.lblMontoInicial  = new System.Windows.Forms.Label();
            this.txtMontoInicial  = new System.Windows.Forms.TextBox();
            this.btnAbrir         = new System.Windows.Forms.Button();
            this.lblPermisoAbrir  = new System.Windows.Forms.Label();
            this.pnlAbierta       = new System.Windows.Forms.Panel();
            this.lblBadge         = new System.Windows.Forms.Label();
            this.lblInfoApertura  = new System.Windows.Forms.Label();
            this.pnlCards         = new System.Windows.Forms.Panel();
            this.cardVentas       = new System.Windows.Forms.Panel();
            this.lblVentasTit     = new System.Windows.Forms.Label();
            this.lblVentasVal     = new System.Windows.Forms.Label();
            this.cardTotal        = new System.Windows.Forms.Panel();
            this.lblTotalTit      = new System.Windows.Forms.Label();
            this.lblTotalVal      = new System.Windows.Forms.Label();
            this.cardEfectivo     = new System.Windows.Forms.Panel();
            this.lblEfectivoTit   = new System.Windows.Forms.Label();
            this.lblEfectivoVal   = new System.Windows.Forms.Label();
            this.lblDesgloseTit   = new System.Windows.Forms.Label();
            this.lblDesgloseVal   = new System.Windows.Forms.Label();
            this.lblMontoReal     = new System.Windows.Forms.Label();
            this.txtMontoReal     = new System.Windows.Forms.TextBox();
            this.btnCerrar        = new System.Windows.Forms.Button();
            this.lblPermisoCerrar = new System.Windows.Forms.Label();
            this.pnlAbrir.SuspendLayout();
            this.pnlAbierta.SuspendLayout();
            this.pnlCards.SuspendLayout();
            this.cardVentas.SuspendLayout();
            this.cardTotal.SuspendLayout();
            this.cardEfectivo.SuspendLayout();
            this.SuspendLayout();
            // lblTituloEstado
            this.lblTituloEstado.AutoSize = true;
            this.lblTituloEstado.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTituloEstado.ForeColor = System.Drawing.Color.FromArgb(15, 15, 17);
            this.lblTituloEstado.Location = new System.Drawing.Point(40, 28);
            this.lblTituloEstado.Name = "lblTituloEstado";
            this.lblTituloEstado.Text = "Caja";
            // lblMensaje
            this.lblMensaje.AutoSize = true;
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblMensaje.ForeColor = System.Drawing.Color.FromArgb(180, 30, 30);
            this.lblMensaje.Location = new System.Drawing.Point(40, 70);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Visible = false;
            // pnlAbrir
            this.pnlAbrir.BackColor = System.Drawing.Color.White;
            this.pnlAbrir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAbrir.Controls.Add(this.lblSinCaja);
            this.pnlAbrir.Controls.Add(this.lblMontoInicial);
            this.pnlAbrir.Controls.Add(this.txtMontoInicial);
            this.pnlAbrir.Controls.Add(this.btnAbrir);
            this.pnlAbrir.Controls.Add(this.lblPermisoAbrir);
            this.pnlAbrir.Location = new System.Drawing.Point(0, 0);
            this.pnlAbrir.Name = "pnlAbrir";
            this.pnlAbrir.Size = new System.Drawing.Size(860, 240);
            // lblSinCaja
            this.lblSinCaja.AutoSize = false;
            this.lblSinCaja.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblSinCaja.ForeColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.lblSinCaja.Location = new System.Drawing.Point(0, 32);
            this.lblSinCaja.Name = "lblSinCaja";
            this.lblSinCaja.Size = new System.Drawing.Size(858, 30);
            this.lblSinCaja.Text = "No hay una caja abierta. Ingresa el monto inicial para comenzar el turno.";
            this.lblSinCaja.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // lblMontoInicial
            this.lblMontoInicial.AutoSize = false;
            this.lblMontoInicial.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblMontoInicial.ForeColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.lblMontoInicial.Location = new System.Drawing.Point(0, 82);
            this.lblMontoInicial.Name = "lblMontoInicial";
            this.lblMontoInicial.Size = new System.Drawing.Size(858, 24);
            this.lblMontoInicial.Text = "Monto inicial (fondo de caja)";
            this.lblMontoInicial.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // txtMontoInicial
            this.txtMontoInicial.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMontoInicial.Font = new System.Drawing.Font("Segoe UI", 15F);
            this.txtMontoInicial.Location = new System.Drawing.Point(260, 114);
            this.txtMontoInicial.Name = "txtMontoInicial";
            this.txtMontoInicial.Size = new System.Drawing.Size(200, 42);
            this.txtMontoInicial.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // btnAbrir
            this.btnAbrir.BackColor = System.Drawing.Color.FromArgb(15, 15, 17);
            this.btnAbrir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbrir.FlatAppearance.BorderSize = 0;
            this.btnAbrir.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnAbrir.ForeColor = System.Drawing.Color.White;
            this.btnAbrir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAbrir.Location = new System.Drawing.Point(470, 112);
            this.btnAbrir.Name = "btnAbrir";
            this.btnAbrir.Size = new System.Drawing.Size(160, 46);
            this.btnAbrir.Text = "Abrir caja";
            this.btnAbrir.UseVisualStyleBackColor = false;
            this.btnAbrir.Click += new System.EventHandler(this.btnAbrir_Click);
            // lblPermisoAbrir
            this.lblPermisoAbrir.AutoSize = false;
            this.lblPermisoAbrir.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Italic);
            this.lblPermisoAbrir.ForeColor = System.Drawing.Color.FromArgb(140, 140, 145);
            this.lblPermisoAbrir.Location = new System.Drawing.Point(0, 174);
            this.lblPermisoAbrir.Name = "lblPermisoAbrir";
            this.lblPermisoAbrir.Size = new System.Drawing.Size(858, 24);
            this.lblPermisoAbrir.Text = "Solo un administrador puede abrir la caja.";
            this.lblPermisoAbrir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPermisoAbrir.Visible = false;
            // pnlAbierta
            this.pnlAbierta.BackColor = System.Drawing.Color.White;
            this.pnlAbierta.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAbierta.Controls.Add(this.lblBadge);
            this.pnlAbierta.Controls.Add(this.lblInfoApertura);
            this.pnlAbierta.Controls.Add(this.pnlCards);
            this.pnlAbierta.Controls.Add(this.lblDesgloseTit);
            this.pnlAbierta.Controls.Add(this.lblDesgloseVal);
            this.pnlAbierta.Controls.Add(this.lblMontoReal);
            this.pnlAbierta.Controls.Add(this.txtMontoReal);
            this.pnlAbierta.Controls.Add(this.btnCerrar);
            this.pnlAbierta.Controls.Add(this.lblPermisoCerrar);
            this.pnlAbierta.Location = new System.Drawing.Point(0, 0);
            this.pnlAbierta.Name = "pnlAbierta";
            this.pnlAbierta.Size = new System.Drawing.Size(860, 480);
            this.pnlAbierta.Visible = false;
            // lblBadge
            this.lblBadge.AutoSize = true;
            this.lblBadge.BackColor = System.Drawing.Color.FromArgb(220, 244, 210);
            this.lblBadge.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblBadge.ForeColor = System.Drawing.Color.FromArgb(20, 100, 20);
            this.lblBadge.Location = new System.Drawing.Point(28, 26);
            this.lblBadge.Name = "lblBadge";
            this.lblBadge.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.lblBadge.Text = "● Abierta";
            // lblInfoApertura
            this.lblInfoApertura.AutoSize = true;
            this.lblInfoApertura.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblInfoApertura.ForeColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.lblInfoApertura.Location = new System.Drawing.Point(160, 32);
            this.lblInfoApertura.Name = "lblInfoApertura";
            this.lblInfoApertura.Text = "Abierta ...";
            // pnlCards
            this.pnlCards.BackColor = System.Drawing.Color.FromArgb(245, 245, 244);
            this.pnlCards.Controls.Add(this.cardVentas);
            this.pnlCards.Controls.Add(this.cardTotal);
            this.pnlCards.Controls.Add(this.cardEfectivo);
            this.pnlCards.Location = new System.Drawing.Point(28, 72);
            this.pnlCards.Name = "pnlCards";
            this.pnlCards.Size = new System.Drawing.Size(800, 114);
            // cardVentas
            this.cardVentas.BackColor = System.Drawing.Color.White;
            this.cardVentas.Controls.Add(this.lblVentasTit);
            this.cardVentas.Controls.Add(this.lblVentasVal);
            this.cardVentas.Location = new System.Drawing.Point(0, 0);
            this.cardVentas.Name = "cardVentas";
            this.cardVentas.Size = new System.Drawing.Size(256, 114);
            // lblVentasTit
            this.lblVentasTit.AutoSize = true;
            this.lblVentasTit.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblVentasTit.ForeColor = System.Drawing.Color.FromArgb(100, 100, 105);
            this.lblVentasTit.Location = new System.Drawing.Point(18, 16);
            this.lblVentasTit.Name = "lblVentasTit";
            this.lblVentasTit.Text = "Ventas del turno";
            // lblVentasVal
            this.lblVentasVal.AutoSize = true;
            this.lblVentasVal.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblVentasVal.ForeColor = System.Drawing.Color.FromArgb(15, 15, 17);
            this.lblVentasVal.Location = new System.Drawing.Point(16, 42);
            this.lblVentasVal.Name = "lblVentasVal";
            this.lblVentasVal.Text = "0";
            // cardTotal
            this.cardTotal.BackColor = System.Drawing.Color.White;
            this.cardTotal.Controls.Add(this.lblTotalTit);
            this.cardTotal.Controls.Add(this.lblTotalVal);
            this.cardTotal.Location = new System.Drawing.Point(270, 0);
            this.cardTotal.Name = "cardTotal";
            this.cardTotal.Size = new System.Drawing.Size(256, 114);
            // lblTotalTit
            this.lblTotalTit.AutoSize = true;
            this.lblTotalTit.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblTotalTit.ForeColor = System.Drawing.Color.FromArgb(100, 100, 105);
            this.lblTotalTit.Location = new System.Drawing.Point(18, 16);
            this.lblTotalTit.Name = "lblTotalTit";
            this.lblTotalTit.Text = "Total vendido";
            // lblTotalVal
            this.lblTotalVal.AutoSize = true;
            this.lblTotalVal.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblTotalVal.ForeColor = System.Drawing.Color.FromArgb(15, 15, 17);
            this.lblTotalVal.Location = new System.Drawing.Point(16, 42);
            this.lblTotalVal.Name = "lblTotalVal";
            this.lblTotalVal.Text = "$0";
            // cardEfectivo
            this.cardEfectivo.BackColor = System.Drawing.Color.White;
            this.cardEfectivo.Controls.Add(this.lblEfectivoTit);
            this.cardEfectivo.Controls.Add(this.lblEfectivoVal);
            this.cardEfectivo.Location = new System.Drawing.Point(540, 0);
            this.cardEfectivo.Name = "cardEfectivo";
            this.cardEfectivo.Size = new System.Drawing.Size(256, 114);
            // lblEfectivoTit
            this.lblEfectivoTit.AutoSize = true;
            this.lblEfectivoTit.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblEfectivoTit.ForeColor = System.Drawing.Color.FromArgb(100, 100, 105);
            this.lblEfectivoTit.Location = new System.Drawing.Point(18, 16);
            this.lblEfectivoTit.Name = "lblEfectivoTit";
            this.lblEfectivoTit.Text = "Efectivo esperado";
            // lblEfectivoVal
            this.lblEfectivoVal.AutoSize = true;
            this.lblEfectivoVal.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblEfectivoVal.ForeColor = System.Drawing.Color.FromArgb(15, 15, 17);
            this.lblEfectivoVal.Location = new System.Drawing.Point(16, 42);
            this.lblEfectivoVal.Name = "lblEfectivoVal";
            this.lblEfectivoVal.Text = "$0";
            // lblDesgloseTit
            this.lblDesgloseTit.AutoSize = false;
            this.lblDesgloseTit.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblDesgloseTit.ForeColor = System.Drawing.Color.FromArgb(100, 100, 105);
            this.lblDesgloseTit.Location = new System.Drawing.Point(0, 206);
            this.lblDesgloseTit.Name = "lblDesgloseTit";
            this.lblDesgloseTit.Size = new System.Drawing.Size(858, 24);
            this.lblDesgloseTit.Text = "Desglose por medio de pago";
            this.lblDesgloseTit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // lblDesgloseVal
            this.lblDesgloseVal.AutoSize = false;
            this.lblDesgloseVal.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblDesgloseVal.ForeColor = System.Drawing.Color.FromArgb(15, 15, 17);
            this.lblDesgloseVal.Location = new System.Drawing.Point(0, 234);
            this.lblDesgloseVal.Name = "lblDesgloseVal";
            this.lblDesgloseVal.Size = new System.Drawing.Size(858, 30);
            this.lblDesgloseVal.Text = "Efectivo: $0     Tarjeta: $0     Transferencia: $0";
            this.lblDesgloseVal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // lblMontoReal
            this.lblMontoReal.AutoSize = false;
            this.lblMontoReal.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblMontoReal.ForeColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.lblMontoReal.Location = new System.Drawing.Point(0, 294);
            this.lblMontoReal.Name = "lblMontoReal";
            this.lblMontoReal.Size = new System.Drawing.Size(858, 26);
            this.lblMontoReal.Text = "Monto real contado al cerrar (efectivo físico en cajón)";
            this.lblMontoReal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // txtMontoReal
            this.txtMontoReal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMontoReal.Font = new System.Drawing.Font("Segoe UI", 15F);
            this.txtMontoReal.Location = new System.Drawing.Point(260, 328);
            this.txtMontoReal.Name = "txtMontoReal";
            this.txtMontoReal.Size = new System.Drawing.Size(200, 42);
            this.txtMontoReal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // btnCerrar
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(15, 15, 17);
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.Location = new System.Drawing.Point(470, 326);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(160, 46);
            this.btnCerrar.Text = "Cerrar caja";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // lblPermisoCerrar
            this.lblPermisoCerrar.AutoSize = false;
            this.lblPermisoCerrar.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Italic);
            this.lblPermisoCerrar.ForeColor = System.Drawing.Color.FromArgb(140, 140, 145);
            this.lblPermisoCerrar.Location = new System.Drawing.Point(0, 386);
            this.lblPermisoCerrar.Name = "lblPermisoCerrar";
            this.lblPermisoCerrar.Size = new System.Drawing.Size(858, 24);
            this.lblPermisoCerrar.Text = "Solo un administrador puede cerrar la caja.";
            this.lblPermisoCerrar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPermisoCerrar.Visible = false;
            // FormCaja
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(245, 244, 241);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.lblTituloEstado);
            this.Controls.Add(this.pnlAbrir);
            this.Controls.Add(this.pnlAbierta);
            this.Name = "FormCaja";
            this.Text = "Caja";
            this.Load      += new System.EventHandler(this.FormCaja_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormCaja_FormClosed);
            this.Resize     += new System.EventHandler(this.FormCaja_Resize);
            this.pnlAbrir.ResumeLayout(false);
            this.pnlAbrir.PerformLayout();
            this.pnlAbierta.ResumeLayout(false);
            this.pnlAbierta.PerformLayout();
            this.pnlCards.ResumeLayout(false);
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

using System;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Eventos;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormCaja : Form
    {
        private readonly CajaService cajaService = new CajaService();

        public FormCaja() { InitializeComponent(); }

        private void FormCaja_Load(object sender, EventArgs e)
        {
            AplicarEstilos();
            CrearBotonHistorial();
            NotificadorCambios.Cambio += OnCambioDatos;
            RefrescarEstado();
        }

        // Botón "Histórico de cajas" (solo admin), anclado arriba a la derecha.
        private void CrearBotonHistorial()
        {
            if (!Sesion.EsAdmin) return;
            var btn = new Button
            {
                Text     = "📋  Histórico de cajas",
                Size     = new Size(220, 38),
                Anchor   = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.ClientSize.Width - 220 - 24, 22)
            };
            EstiloPos.AplicarBotonSecundario(btn);
            btn.Click += (s, e) => { using (var f = new FormHistorialCajas()) f.ShowDialog(this); };
            this.Controls.Add(btn);
            btn.BringToFront();
        }

        private void FormCaja_FormClosed(object sender, FormClosedEventArgs e)
        {
            NotificadorCambios.Cambio -= OnCambioDatos;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            CentrarContenido();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CentrarContenido();
        }

        private void FormCaja_Resize(object sender, EventArgs e) { CentrarContenido(); }

        // ── Estilos runtime ───────────────────────────────────────────

        private void AplicarEstilos()
        {
            this.BackColor = EstiloPos.Fondo;

            // Título
            lblTituloEstado.Font      = EstiloPos.FontTitulo;
            lblTituloEstado.ForeColor = EstiloPos.Ink1;

            // Mensaje error
            lblMensaje.Font      = EstiloPos.FontBody;
            lblMensaje.ForeColor = EstiloPos.Rojo;

            // ── Panel ABRIR ──────────────────────────────────────────
            pnlAbrir.BackColor   = EstiloPos.Surface;

            lblSinCaja.Font      = new Font("Segoe UI", 12F);
            lblSinCaja.ForeColor = EstiloPos.Ink2;

            lblMontoInicial.Font      = EstiloPos.FontLabel;
            lblMontoInicial.ForeColor = EstiloPos.Ink2;

            EstiloPos.AplicarInput(txtMontoInicial, grande: true);
            txtMontoInicial.TextAlign = HorizontalAlignment.Center;
            txtMontoInicial.Width     = 220;

            EstiloPos.AplicarBotonPrimario(btnAbrir, grande: true);
            btnAbrir.Width = 180;

            lblPermisoAbrir.Font      = new Font("Segoe UI", 10.5F, FontStyle.Italic);
            lblPermisoAbrir.ForeColor = EstiloPos.Ink3;

            // ── Panel ABIERTA ────────────────────────────────────────
            pnlAbierta.BackColor = EstiloPos.Surface;

            lblBadge.Font      = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblBadge.BackColor = Color.FromArgb(220, 244, 210);
            lblBadge.ForeColor = Color.FromArgb(20, 100, 20);

            lblInfoApertura.Font      = EstiloPos.FontBody;
            lblInfoApertura.ForeColor = EstiloPos.Ink2;

            // Cards
            pnlCards.BackColor = EstiloPos.Surface;
            EstiloCard(cardVentas,   lblVentasTit,   "Ventas del turno",  lblVentasVal,   EstiloPos.Azul);
            EstiloCard(cardTotal,    lblTotalTit,     "Total vendido",     lblTotalVal,    EstiloPos.Verde);
            EstiloCard(cardEfectivo, lblEfectivoTit,  "Efectivo esperado", lblEfectivoVal, EstiloPos.Amber);

            // Desglose
            lblDesgloseTit.Font      = EstiloPos.FontSmall;
            lblDesgloseTit.ForeColor = EstiloPos.Ink3;

            lblDesgloseVal.Font      = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblDesgloseVal.ForeColor = EstiloPos.Ink1;

            // Cierre
            lblMontoReal.Font      = EstiloPos.FontLabel;
            lblMontoReal.ForeColor = EstiloPos.Ink2;

            EstiloPos.AplicarInput(txtMontoReal, grande: true);
            txtMontoReal.TextAlign = HorizontalAlignment.Center;
            txtMontoReal.Width     = 220;

            EstiloPos.AplicarBotonPrimario(btnCerrar, grande: true);
            btnCerrar.Width = 180;

            lblPermisoCerrar.Font      = new Font("Segoe UI", 10.5F, FontStyle.Italic);
            lblPermisoCerrar.ForeColor = EstiloPos.Ink3;
        }

        private void EstiloCard(Panel card, Label titulo, string textoTit, Label valor, Color acento)
        {
            card.BackColor   = EstiloPos.Surface;

            titulo.Font      = EstiloPos.FontSmall;
            titulo.ForeColor = EstiloPos.Ink3;
            titulo.Text      = textoTit;
            titulo.Location  = new Point(20, 16);

            valor.Font      = EstiloPos.FontMetrica;
            valor.ForeColor = EstiloPos.Ink1;
            valor.Location  = new Point(18, 44);

            // Borde sutil + barra de acento (consistente con el Dashboard)
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(EstiloPos.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var br = new SolidBrush(acento))
                    e.Graphics.FillRectangle(br, 0, 0, 4, card.Height);
            };
        }

        // ── Lógica de estado ─────────────────────────────────────────

        private void OnCambioDatos(string entidad)
        {
            if (entidad == Entidad.Caja || entidad == Entidad.Venta)
                RefrescarEstado();
        }

        private void RefrescarEstado()
        {
            var caja    = cajaService.ObtenerCajaAbierta();
            bool esAdmin = Sesion.EsAdmin;

            if (caja == null)
            {
                pnlAbierta.Visible = false;
                pnlAbrir.Visible   = true;
                lblTituloEstado.Text = "Caja — sin turno abierto";

                lblPermisoAbrir.Visible  = !esAdmin;
                txtMontoInicial.Enabled  = esAdmin;
                btnAbrir.Enabled         = esAdmin;
                btnAbrir.BackColor       = esAdmin ? EstiloPos.Ink1 : EstiloPos.Ink3;
            }
            else
            {
                pnlAbrir.Visible   = false;
                pnlAbierta.Visible = true;
                lblTituloEstado.Text = "Caja — turno abierto";

                var resumen           = cajaService.ObtenerResumen(caja.IdCaja);
                decimal efectivoEsperado = cajaService.CalcularEfectivoEsperado(caja, resumen);

                lblInfoApertura.Text =
                    "Abierta el " + caja.FechaApertura.ToString("dd/MM/yyyy HH:mm") +
                    "   ·   Fondo inicial: $" + caja.MontoInicial.ToString("N0");

                lblVentasVal.Text   = resumen.CantidadVentas.ToString();
                lblTotalVal.Text    = "$" + resumen.TotalVendido.ToString("N0");
                lblEfectivoVal.Text = "$" + efectivoEsperado.ToString("N0");

                lblDesgloseVal.Text =
                    "Efectivo: $"      + resumen.TotalEfectivo.ToString("N0") +
                    "     Tarjeta: $"  + resumen.TotalTarjeta.ToString("N0") +
                    "     Transferencia: $" + resumen.TotalTransferencia.ToString("N0");

                lblPermisoCerrar.Visible = !esAdmin;
                txtMontoReal.Enabled     = esAdmin;
                btnCerrar.Enabled        = esAdmin;
                btnCerrar.BackColor      = esAdmin ? EstiloPos.Ink1 : EstiloPos.Ink3;
            }

            lblMensaje.Visible = false;
            CentrarContenido();
        }

        // ── Centrado responsive ───────────────────────────────────────

        private void CentrarContenido()
        {
            if (this.ClientSize.Width == 0) return;

            int cw = this.ClientSize.Width;
            int ch = this.ClientSize.Height;

            Panel panelActivo = (pnlAbrir.Visible || !pnlAbierta.Visible)
                ? pnlAbrir : pnlAbierta;

            int pw    = Math.Min(960, cw - 80);
            int titH  = lblTituloEstado.Height;
            int titY  = Math.Max(28, (ch - panelActivo.Height - titH - 32) / 3);

            lblTituloEstado.Location = new Point((cw - lblTituloEstado.Width) / 2, titY);

            int panelX = (cw - pw) / 2;
            int panelY = lblTituloEstado.Bottom + 22;
            panelActivo.Location = new Point(panelX, panelY);
            panelActivo.Width    = pw;

            // Etiquetas de ancho completo: centrarlas respecto al ancho real del panel
            int wInt = panelActivo.ClientSize.Width;
            if (pnlAbrir.Visible)
                foreach (var l in new[] { lblSinCaja, lblMontoInicial, lblPermisoAbrir })
                { l.Left = 0; l.Width = wInt; }
            else
                foreach (var l in new[] { lblDesgloseTit, lblDesgloseVal, lblMontoReal, lblPermisoCerrar })
                { l.Left = 0; l.Width = wInt; }

            // Centrar contenido interno del panel ABRIR
            if (pnlAbrir.Visible)
            {
                int cx = pw / 2;
                int inputW = txtMontoInicial.Width;
                int btnW   = btnAbrir.Width;
                int gap    = 16;
                int totalW = inputW + gap + btnW;
                txtMontoInicial.Location = new Point(cx - totalW / 2, 112);
                btnAbrir.Location        = new Point(txtMontoInicial.Right + gap, 110);
                lblPermisoAbrir.Location = new Point(0, 174);
                lblSinCaja.Location      = new Point(0, 32);
                lblMontoInicial.Location = new Point(0, 84);
            }

            // Centrar contenido interno del panel ABIERTA
            if (pnlAbierta.Visible && pnlCards != null)
            {
                int cardArea = pw - 56;
                pnlCards.Location = new Point(28, 72);
                pnlCards.Width    = cardArea;
                int w = (cardArea - 20) / 3;
                if (w > 0)
                {
                    cardVentas.Size   = new Size(w, 110); cardVentas.Location   = new Point(0,        0);
                    cardTotal.Size    = new Size(w, 110); cardTotal.Location    = new Point(w + 10,   0);
                    cardEfectivo.Size = new Size(w, 110); cardEfectivo.Location = new Point((w+10)*2, 0);
                }

                // Centrar campo de cierre
                int cx2   = pw / 2;
                int inputW = txtMontoReal.Width;
                int btnW2  = btnCerrar.Width;
                int totalW2 = inputW + 16 + btnW2;
                txtMontoReal.Location = new Point(cx2 - totalW2 / 2, 328);
                btnCerrar.Location    = new Point(txtMontoReal.Right + 16, 326);
            }

            lblMensaje.Location = new Point(
                (cw - lblMensaje.Width) / 2, panelActivo.Bottom + 12);
        }

        // ── Abrir caja ────────────────────────────────────────────────

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMontoInicial.Text, out decimal monto) || monto < 0)
            { MostrarMensaje("Ingresa un monto inicial válido."); return; }
            try
            {
                cajaService.AbrirCaja(Sesion.UsuarioActual.IdUsuario, monto);
                txtMontoInicial.Clear();
                RefrescarEstado();
            }
            catch (Exception ex) { MostrarMensaje(Errores.Usuario(ex)); }
        }

        // ── Cerrar caja con arqueo ────────────────────────────────────

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMontoReal.Text, out decimal montoReal) || montoReal < 0)
            { MostrarMensaje("Ingresa el monto real contado."); return; }

            var caja     = cajaService.ObtenerCajaAbierta();
            var resumen  = cajaService.ObtenerResumen(caja.IdCaja);
            decimal esperado   = cajaService.CalcularEfectivoEsperado(caja, resumen);
            decimal diferencia = montoReal - esperado;

            // Faltante → requiere autorización de administrador
            if (diferencia < 0)
            {
                using (var dlg = new FormVerificarAdmin(
                    "Hay un FALTANTE de $" + Math.Abs(diferencia).ToString("N0") + " en la caja.\n" +
                    "Para continuar el cierre se requiere autorización de un administrador."))
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK)
                    { MostrarMensaje("Cierre cancelado. El faltante debe ser resuelto."); return; }
                }
            }

            if (!Aviso.Confirmar(this, "Se registrará el cierre con $" + montoReal.ToString("N0") + " contados en caja.",
                    "¿Cerrar la caja?", "Cerrar caja"))
                return;

            try
            {
                cajaService.CerrarCaja(montoReal);
                MostrarArqueo(diferencia);
                txtMontoReal.Clear();
                RefrescarEstado();
            }
            catch (Exception ex) { MostrarMensaje(Errores.Usuario(ex)); }
        }

        private void MostrarArqueo(decimal diferencia)
        {
            if (diferencia == 0)
                Aviso.Exito(this, "El arqueo cuadró exactamente. No hay diferencias.", "Caja cerrada");
            else if (diferencia > 0)
                Aviso.Info(this,
                    "Sobrante de $" + diferencia.ToString("N0") + ".\nHay más efectivo del esperado — conviene revisarlo con el cajero.",
                    "Caja cerrada");
            else
                Aviso.Advertencia(this,
                    "Faltante de $" + Math.Abs(diferencia).ToString("N0") + ".\nQueda registrado para seguimiento administrativo.",
                    "Caja cerrada");
        }

        private void MostrarMensaje(string msg)
        {
            lblMensaje.Text    = msg;
            lblMensaje.Visible = true;
        }
    }
}

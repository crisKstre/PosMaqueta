using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public class FormReportes : Form
    {
        private readonly VentaService ventaService = new VentaService();
        private readonly ProductoService productoService = new ProductoService();

        private DateTimePicker dtpDesde, dtpHasta;
        private Label lblVentasVal, lblTotalVal, lblIvaVal, lblTicketVal, lblDesgloseVal;
        private Label lblUtilVal, lblMargenVal, lblInvVal;
        private DataGridView dgvTop, dgvVentas;
        private List<Venta> ventasPeriodo = new List<Venta>();

        public FormReportes()
        {
            this.Text = "Reportes";
            InitUI();
        }

        private void InitUI()
        {
            this.BackColor = EstiloPos.Fondo;
            // A 720p el header + tarjetas dejan poco alto para las tablas. AutoScrollMinSize fuerza un
            // área virtual mínima: si la pantalla es más baja aparece scroll vertical y las tablas se ven
            // completas; en pantallas altas (1080p) no hay scroll y todo se ve directo.
            this.AutoScroll = true;
            this.AutoScrollMinSize = new Size(0, 726);
            this.Load += (s, e) => SetRango(DateTime.Today, DateTime.Today);

            // ── Header + filtro de fechas ──────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 124, BackColor = EstiloPos.Fondo };

            var lblTitulo = new Label
            {
                Text = "Reportes de ventas",
                Font = EstiloPos.FontTitulo, ForeColor = EstiloPos.Ink1,
                AutoSize = true, Location = new Point(24, 16)
            };

            var lblDesde = new Label { Text = "Desde", Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(24, 62) };
            dtpDesde = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = EstiloPos.FontBody,
                Location = new Point(24, 84), Size = new Size(150, 30) };

            var lblHasta = new Label { Text = "Hasta", Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(190, 62) };
            dtpHasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = EstiloPos.FontBody,
                Location = new Point(190, 84), Size = new Size(150, 30) };

            var btnVer = new Button { Text = "Ver", Location = new Point(356, 83), Size = new Size(90, 32) };
            EstiloPos.AplicarBotonPrimario(btnVer);
            btnVer.Click += (s, e) => Generar();

            var btnHoy    = CrearChip("Hoy",      () => SetRango(DateTime.Today, DateTime.Today));
            var btnSemana = CrearChip("7 días",   () => SetRango(DateTime.Today.AddDays(-6), DateTime.Today));
            var btnMes    = CrearChip("Este mes", () => SetRango(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today));
            btnHoy.Location    = new Point(470, 84);
            btnSemana.Location = new Point(btnHoy.Right + 8, 84);
            btnMes.Location    = new Point(btnSemana.Right + 8, 84);

            var btnAnular = new Button { Text = "Anular venta", Size = new Size(150, 34) };
            EstiloPos.AplicarBotonSecundario(btnAnular, EstiloPos.Rojo);
            btnAnular.Click += (s, e) => AnularSeleccionada();

            var btnDevolver = new Button { Text = "Devolución", Size = new Size(140, 34) };
            EstiloPos.AplicarBotonSecundario(btnDevolver, EstiloPos.Amber);
            btnDevolver.Click += (s, e) => DevolverSeleccionada();

            pnlHeader.Controls.AddRange(new Control[] {
                lblTitulo, lblDesde, dtpDesde, lblHasta, dtpHasta, btnVer, btnHoy, btnSemana, btnMes, btnAnular, btnDevolver });
            pnlHeader.Resize += (s, e) =>
            {
                btnAnular.Location   = new Point(pnlHeader.ClientSize.Width - btnAnular.Width - 24, 80);
                btnDevolver.Location = new Point(btnAnular.Left - btnDevolver.Width - 8, 80);
            };

            // ── Cards ──────────────────────────────────────────────────────
            var pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 232, Padding = new Padding(24, 0, 0, 14),
                BackColor = EstiloPos.Fondo, FlowDirection = FlowDirection.LeftToRight, WrapContents = true
            };
            pnlCards.Controls.Add(CrearCard("Ventas",          EstiloPos.Azul,  out lblVentasVal));
            pnlCards.Controls.Add(CrearCard("Total vendido",   EstiloPos.Verde, out lblTotalVal));
            pnlCards.Controls.Add(CrearCard("Utilidad",        EstiloPos.Verde, out lblUtilVal));
            pnlCards.Controls.Add(CrearCard("Margen",          EstiloPos.Azul,  out lblMargenVal));
            pnlCards.Controls.Add(CrearCard("IVA (19%) incl.", EstiloPos.Ink2,  out lblIvaVal));
            pnlCards.Controls.Add(CrearCard("Ticket promedio", EstiloPos.Amber, out lblTicketVal));
            pnlCards.Controls.Add(CrearCard("Inventario a costo", EstiloPos.Ink2, out lblInvVal));
            pnlCards.Controls.Add(CrearCard("Medios de pago",  EstiloPos.Ink2,  out lblDesgloseVal));
            lblDesgloseVal.Font      = EstiloPos.FontSmall;
            lblDesgloseVal.ForeColor = EstiloPos.Ink1;
            lblDesgloseVal.TextAlign = ContentAlignment.TopLeft;
            lblDesgloseVal.Location  = new Point(16, 26);
            lblDesgloseVal.Size      = new Size(170, 70);

            // ── Tablas ─────────────────────────────────────────────────────
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                Padding = new Padding(24, 0, 24, 44), BackColor = EstiloPos.Fondo
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var pnlTopProd = CrearPanelTabla("Top productos por utilidad", out dgvTop, new Padding(0, 0, 10, 0));
            dgvTop.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", FillWeight = 120 });
            dgvTop.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cant.",    FillWeight = 40 });
            dgvTop.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total",    FillWeight = 50 });
            dgvTop.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Utilidad", FillWeight = 55 });
            EstiloPos.AplicarGrid(dgvTop);

            var pnlListaVentas = CrearPanelTabla("Ventas del período  ·  doble clic para ver detalle", out dgvVentas, new Padding(10, 0, 0, 0));
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVtaId", Visible = false });
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "N°",            FillWeight = 28 });
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha",         FillWeight = 65 });
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Medio de pago", FillWeight = 62 });
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total",         FillWeight = 50 });
            EstiloPos.AplicarGrid(dgvVentas);
            dgvVentas.CellDoubleClick += DgvVentas_CellDoubleClick;

            split.Controls.Add(pnlTopProd, 0, 0);
            split.Controls.Add(pnlListaVentas, 1, 0);

            this.Controls.Add(split);
            this.Controls.Add(pnlCards);
            this.Controls.Add(pnlHeader);
        }

        private void SetRango(DateTime desde, DateTime hasta)
        {
            dtpDesde.Value = desde;
            dtpHasta.Value = hasta;
            Generar();
        }

        // Atajo: F5 vuelve a generar el reporte con el rango actual
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5) { Generar(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Generar()
        {
            DateTime desde = dtpDesde.Value.Date, hasta = dtpHasta.Value.Date;
            if (hasta < desde) { var t = desde; desde = hasta; hasta = t; }

            var r = ventaService.ObtenerResumenVentas(desde, hasta);
            lblVentasVal.Text   = r.CantidadVentas.ToString();
            lblTotalVal.Text    = "$" + r.TotalVendido.ToString("N0");
            lblIvaVal.Text      = "$" + Impuestos.Iva(r.TotalVendido).ToString("N0");
            lblTicketVal.Text   = "$" + r.TicketPromedio.ToString("N0");
            lblDesgloseVal.Text =
                "Efectivo:  $" + r.TotalEfectivo.ToString("N0") + "\n" +
                "Tarjeta:   $" + r.TotalTarjeta.ToString("N0") + "\n" +
                "Transfer.: $" + r.TotalTransferencia.ToString("N0") + "\n" +
                "Devoluc.: -$" + r.TotalDevoluciones.ToString("N0");
            lblUtilVal.Text   = "$" + r.Utilidad.ToString("N0");
            lblMargenVal.Text = r.MargenPorcentaje.ToString("0") + "%";
            lblInvVal.Text    = "$" + productoService.ValorInventarioACosto().ToString("N0");

            dgvTop.Rows.Clear();
            foreach (var p in ventaService.ObtenerTopUtilidad(desde, hasta, 12))
                dgvTop.Rows.Add(p.Nombre, p.Cantidad.ToString("0.##"), "$" + p.Total.ToString("N0"), "$" + p.Utilidad.ToString("N0"));

            ventasPeriodo = ventaService.ObtenerVentas(desde, hasta);
            dgvVentas.Rows.Clear();
            foreach (var v in ventasPeriodo)
                dgvVentas.Rows.Add(v.IdVenta, "#" + v.IdVenta, v.Fecha.ToString("dd/MM HH:mm"), v.MedioPago, "$" + v.Total.ToString("N0"));
        }

        private Venta VentaDeFila(int rowIndex)
        {
            if (rowIndex < 0) return null;
            int id = Convert.ToInt32(dgvVentas.Rows[rowIndex].Cells["colVtaId"].Value);
            return ventasPeriodo.Find(x => x.IdVenta == id);
        }

        private void DgvVentas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var v = VentaDeFila(e.RowIndex);
            if (v != null)
                using (var f = new FormDetalleVenta(v)) f.ShowDialog(this);
        }

        private void DevolverSeleccionada()
        {
            if (dgvVentas.CurrentRow == null)
            { Aviso.Info(this, "Selecciona una venta de la lista para hacer una devolución."); return; }
            var v = VentaDeFila(dgvVentas.CurrentRow.Index);
            if (v == null) return;
            using (var f = new FormDevolucion(v.IdVenta))
                if (f.ShowDialog(this) == DialogResult.OK) Generar();   // refresca stock/efectivo
        }

        private void AnularSeleccionada()
        {
            if (dgvVentas.CurrentRow == null)
            { Aviso.Info(this, "Selecciona una venta de la lista para anularla."); return; }

            var v = VentaDeFila(dgvVentas.CurrentRow.Index);
            if (v == null) return;

            if (!Aviso.Confirmar(this,
                    "Se anulará la venta N°" + v.IdVenta + " ($" + v.Total.ToString("N0") + ") y su stock volverá al inventario.\n" +
                    "Esta acción no se puede deshacer.",
                    "¿Anular venta?", "Anular", TipoAviso.Error))
                return;

            try
            {
                ventaService.AnularVenta(v.IdVenta);
                Aviso.Exito(this, "La venta N°" + v.IdVenta + " fue anulada y el stock devuelto al inventario.", "Venta anulada");
                Generar();
            }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex)); }
        }

        // ── Helpers visuales (mismo lenguaje que el Dashboard) ─────────────

        private Button CrearChip(string texto, Action onClick)
        {
            int w = TextRenderer.MeasureText(texto, EstiloPos.FontSmall).Width + 28;
            var b = new Button
            {
                Text = texto, AutoSize = false, Size = new Size(w, 32),
                FlatStyle = FlatStyle.Flat, Font = EstiloPos.FontSmall,
                BackColor = EstiloPos.Surface, ForeColor = EstiloPos.Ink2,
                Cursor = Cursors.Hand, UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderColor = EstiloPos.Border;
            b.FlatAppearance.MouseOverBackColor = EstiloPos.Hover;
            b.Click += (s, e) => onClick();
            return b;
        }

        private Panel CrearCard(string titulo, Color acento, out Label valor)
        {
            var card = new Panel { Width = 192, Height = 100, Margin = new Padding(0, 0, 14, 12), BackColor = EstiloPos.Surface };
            var barra = new Panel { Width = 4, Dock = DockStyle.Left, BackColor = acento };

            var lblTit = new Label
            {
                Text = titulo, AutoSize = false, Location = new Point(16, 12), Size = new Size(170, 20),
                ForeColor = EstiloPos.Ink2, Font = EstiloPos.FontSmall
            };
            valor = new Label
            {
                Text = "—", AutoSize = false, Location = new Point(16, 36), Size = new Size(170, 54),
                ForeColor = acento, TextAlign = ContentAlignment.MiddleLeft, Font = EstiloPos.FontMetrica
            };

            card.Controls.Add(barra);
            card.Controls.Add(lblTit);
            card.Controls.Add(valor);
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(EstiloPos.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            return card;
        }

        private Panel CrearPanelTabla(string titulo, out DataGridView dgv, Padding margen)
        {
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = EstiloPos.Fondo, Padding = margen };
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = EstiloPos.Surface };
            inner.Paint += (s, e) =>
            {
                using (var pen = new Pen(EstiloPos.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, inner.Width - 1, inner.Height - 1);
            };

            var pnlTit = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = EstiloPos.Surface };
            pnlTit.Paint += (s, e) =>
            {
                using (var pen = new Pen(EstiloPos.Border))
                    e.Graphics.DrawLine(pen, 0, pnlTit.Height - 1, pnlTit.Width, pnlTit.Height - 1);
            };
            var lbl = new Label
            {
                Text = titulo, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = EstiloPos.Ink1, Font = EstiloPos.FontLabel, Padding = new Padding(16, 0, 0, 0)
            };
            pnlTit.Controls.Add(lbl);

            dgv = new DataGridView { Dock = DockStyle.Fill };

            inner.Controls.Add(dgv);
            inner.Controls.Add(pnlTit);
            outer.Controls.Add(inner);
            return outer;
        }
    }
}

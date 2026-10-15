using System;
using System.Collections.Generic;
using System.Linq;
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
        private Label lblVentasVal, lblTotalVal, lblIvaVal, lblTicketVal;
        private Label lblCobroEfectivo, lblCobroTarjeta, lblCobroTransferencia, lblReembolsos;
        private Label lblUtilVal, lblMargenVal, lblInvVal, lblInvValTot, lblTotalProd;
        private DataGridView dgvTop, dgvVentas;
        private List<Venta> ventasPeriodo = new List<Venta>();
        private ResumenVentas resumenMostrado;
        private List<Producto> inventarioMostrado = new List<Producto>();
        private string periodoMostrado;

        public FormReportes()
        {
            this.Text = "Reportes";
            InitUI();
        }

        private void InitUI()
        {
            this.BackColor = EstiloPos.Fondo;
            // El cuerpo desplaza tarjetas y tablas; los filtros permanecen accesibles.
            this.AutoScroll = false;
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
            // Dos filas estables: las columnas absorben los cambios de ancho.
            var pnlCards = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 334, ColumnCount = 5, RowCount = 2,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                Padding = new Padding(24, 8, 24, 14), BackColor = EstiloPos.Fondo
            };
            // La tercera columna contiene el desglose y necesita espacio para concepto + importe.
            foreach (float ancho in new[] { 18F, 18F, 28F, 18F, 18F })
                pnlCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, ancho));
            pnlCards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlCards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlCards.Controls.Add(CrearCard("Ventas",          EstiloPos.Azul,  out lblVentasVal));
            pnlCards.Controls.Add(CrearCard("Venta neta",      EstiloPos.Verde, out lblTotalVal));
            pnlCards.Controls.Add(CrearCard("Inventario total", EstiloPos.Ink2, out lblInvValTot));
            pnlCards.Controls.Add(CrearCard("Margen", EstiloPos.Azul, out lblMargenVal));
            pnlCards.Controls.Add(CrearCard("IVA de ventas (bruto)", EstiloPos.Ink2, out lblIvaVal));
            pnlCards.Controls.Add(CrearCard("Ticket promedio", EstiloPos.Amber, out lblTicketVal));
            pnlCards.Controls.Add(CrearCard("Inventario a costo", EstiloPos.Ink2, out lblInvVal));
            pnlCards.Controls.Add(CrearCardCobros());
            pnlCards.Controls.Add(CrearCard("Utilidad", EstiloPos.Verde, out lblUtilVal));
            pnlCards.Controls.Add(CrearCard("Total productos", EstiloPos.Ink2, out lblTotalProd));
            // Todas las tarjetas llenan su celda y mantienen alineados los bordes.
            foreach (Control card in pnlCards.Controls) card.Dock = DockStyle.Fill;
            ConectarTarjeta(lblVentasVal.Parent, IndicadorReporte.Ventas);
            ConectarTarjeta(lblTotalVal.Parent, IndicadorReporte.VentaNeta);
            ConectarTarjeta(lblInvValTot.Parent, IndicadorReporte.Inventario);
            ConectarTarjeta(lblMargenVal.Parent, IndicadorReporte.Margen);
            ConectarTarjeta(lblIvaVal.Parent, IndicadorReporte.Iva);
            ConectarTarjeta(lblTicketVal.Parent, IndicadorReporte.Ticket);
            ConectarTarjeta(lblInvVal.Parent, IndicadorReporte.InventarioCosto);
            ConectarTarjeta(lblCobroEfectivo.Parent.Parent, IndicadorReporte.Cobros);
            ConectarTarjeta(lblUtilVal.Parent, IndicadorReporte.Utilidad);
            ConectarTarjeta(lblTotalProd.Parent, IndicadorReporte.Productos);

            // ── Tablas ─────────────────────────────────────────────────────
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                Padding = new Padding(24, 0, 24, 10), BackColor = EstiloPos.Fondo
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var pnlTopProd = CrearPanelTabla("Productos por utilidad tras devoluciones", out dgvTop, new Padding(0, 0, 10, 0));
            dgvTop.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", FillWeight = 120 });
            dgvTop.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cant. neta", FillWeight = 40 });
            dgvTop.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Venta neta", FillWeight = 50 });
            dgvTop.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Utilidad", FillWeight = 55 });
            EstiloPos.AplicarGrid(dgvTop);
            DetallesGrid.Conectar(this, dgvTop, fila => DetallesGrid.Rendimiento(fila.Tag as ProductoVendido, dgvTop.Tag as string));

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

            var cuerpo = new Panel
            {
                Name = "cuerpoReportes", Dock = DockStyle.Fill,
                AutoScroll = true, BackColor = EstiloPos.Fondo
            };
            var contenido = new Panel { Dock = DockStyle.Top, BackColor = EstiloPos.Fondo };
            contenido.Controls.Add(split);
            contenido.Controls.Add(pnlCards);
            cuerpo.Controls.Add(contenido);

            bool mostrarResumen = true;
            Action ajustarAlto = () =>
            {
                // Al bajar, las tablas pueden ocupar todo el alto disponible del cuerpo.
                int alto = (mostrarResumen ? pnlCards.Height : 0) + Math.Max(420, cuerpo.ClientSize.Height);
                if (contenido.Height != alto) contenido.Height = alto;
            };
            var btnResumen = new Button
            {
                Name = "btnResumen", Text = "Ocultar resumen", Size = new Size(180, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            EstiloPos.AplicarBotonSecundario(btnResumen, EstiloPos.Ink2);
            btnResumen.Click += (s, e) =>
            {
                cuerpo.AutoScrollPosition = Point.Empty;
                mostrarResumen = !mostrarResumen;
                pnlCards.Visible = mostrarResumen;
                btnResumen.Text = mostrarResumen ? "Ocultar resumen" : "Mostrar resumen";
                ajustarAlto();
            };
            pnlHeader.Controls.Add(btnResumen);
            pnlHeader.Resize += (s, e) =>
                btnResumen.Location = new Point(pnlHeader.ClientSize.Width - btnResumen.Width - 24, 18);
            cuerpo.ClientSizeChanged += (s, e) => ajustarAlto();

            this.Controls.Add(cuerpo);
            this.Controls.Add(pnlHeader);
            ajustarAlto();
        }

        private void ConectarTarjeta(Control tarjeta, IndicadorReporte indicador)
        {
            DetallesTarjetas.Conectar(this, tarjeta, () =>
                DetallesTarjetas.Reporte(indicador, resumenMostrado, periodoMostrado, ventasPeriodo, inventarioMostrado));
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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FormReportes
            // 
            this.ClientSize = new System.Drawing.Size(1256, 666);
            this.Name = "FormReportes";
            this.ResumeLayout(false);

        }

        private void Generar()
        {
            try { GenerarReporte(); }
            catch (Exception ex)
            {
                resumenMostrado = null;
                inventarioMostrado.Clear();
                foreach (var label in new[] { lblVentasVal, lblTotalVal, lblIvaVal, lblTicketVal,
                    lblCobroEfectivo, lblCobroTarjeta, lblCobroTransferencia, lblReembolsos, lblUtilVal, lblMargenVal, lblInvVal, lblInvValTot, lblTotalProd })
                    label.Text = "—";
                dgvTop.Rows.Clear();
                dgvVentas.Rows.Clear();
                ventasPeriodo.Clear();
                Aviso.Error(this, Errores.Usuario(ex), "No se pudo generar el reporte");
            }
        }

        private void GenerarReporte()
        {
            DateTime desde = dtpDesde.Value.Date, hasta = dtpHasta.Value.Date;
            if (hasta < desde) { var t = desde; desde = hasta; hasta = t; }


            var r = ventaService.ObtenerResumenVentas(desde, hasta);
            // Misma instantánea para los indicadores de inventario y sus listados de detalle.
            var inventario = productoService.ObtenerTodos();
            lblVentasVal.Text   = r.CantidadVentas.ToString();
            lblTotalVal.Text    = "$" + r.TotalNeto.ToString("N0");
            lblIvaVal.Text      = "$" + Impuestos.Iva(r.TotalVendido).ToString("N0");
            lblTicketVal.Text   = "$" + r.TicketPromedio.ToString("N0");
            lblTotalProd.Text   = inventario.Count.ToString();

            lblCobroEfectivo.Text = "$" + r.TotalEfectivo.ToString("N0");
            lblCobroTarjeta.Text = "$" + r.TotalTarjeta.ToString("N0");
            lblCobroTransferencia.Text = "$" + r.TotalTransferencia.ToString("N0");
            lblInvValTot.Text = "$" + inventario.Where(p => p.Activo).Sum(p => p.Stock * p.Precio).ToString("N0");
            lblUtilVal.Text   = "$" + r.Utilidad.ToString("N0");
            lblMargenVal.Text = r.TieneMargen ? r.MargenPorcentaje.ToString("0") + "%" : "—";
            lblInvVal.Text    = "$" + inventario.Where(p => p.Activo).Sum(p => p.Stock * p.Costo).ToString("N0");

            lblReembolsos.Text = "$" + r.TotalDevoluciones.ToString("N0");

            dgvTop.Rows.Clear();
            dgvTop.Tag = "Período: " + desde.ToString("dd/MM/yyyy") + " al " + hasta.ToString("dd/MM/yyyy");
            foreach (var p in ventaService.ObtenerTopUtilidad(desde, hasta, 12))
            {
                int fila = dgvTop.Rows.Add(p.Nombre, p.Cantidad.ToString("0.###"), "$" + p.Total.ToString("N0"), "$" + p.Utilidad.ToString("N0"));
                dgvTop.Rows[fila].Tag = p;
            }

            ventasPeriodo = ventaService.ObtenerVentas(desde, hasta);
            dgvVentas.Rows.Clear();
            foreach (var v in ventasPeriodo)
                dgvVentas.Rows.Add(v.IdVenta, "#" + v.IdVenta, v.Fecha.ToString("dd/MM HH:mm"), v.MedioPago, "$" + v.Total.ToString("N0"));
            resumenMostrado = r;
            inventarioMostrado = inventario;
            periodoMostrado = dgvTop.Tag as string;
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

        private Panel CrearCardCobros()
        {
            Label marcador;
            var card = CrearCard("Cobros / reembolsos", EstiloPos.Ink2, out marcador);
            card.Controls.Remove(marcador);
            marcador.Dispose();
            card.Size = new Size(320, 144);
            foreach (Control c in card.Controls) if (c is Label) c.Height = 20;
            var filas = new TableLayoutPanel
            {
                Location = new Point(16, 36), Size = new Size(288, 96),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 2, RowCount = 4, Margin = Padding.Empty, Padding = Padding.Empty
            };
            filas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            filas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            string[] conceptos = { "Efectivo", "Tarjeta", "Transferencia", "Reembolsos" };
            var importes = new Label[conceptos.Length];
            for (int i = 0; i < conceptos.Length; i++)
            {
                filas.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
                filas.Controls.Add(new Label
                {
                    Text = conceptos[i], Dock = DockStyle.Fill, AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft, Font = EstiloPos.FontSmall,
                    ForeColor = EstiloPos.Ink2, Margin = Padding.Empty
                }, 0, i);
                importes[i] = new Label
                {
                    Text = "—", Dock = DockStyle.Fill, AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight, Font = EstiloPos.FontSmall,
                    ForeColor = i == 3 ? EstiloPos.Rojo : EstiloPos.Ink1, Margin = Padding.Empty
                };
                filas.Controls.Add(importes[i], 1, i);
            }
            lblCobroEfectivo = importes[0]; lblCobroTarjeta = importes[1];
            lblCobroTransferencia = importes[2]; lblReembolsos = importes[3];
            card.Controls.Add(filas);
            return card;
        }

        private Panel CrearCard(string titulo, Color acento, out Label valor)
        {
            var card = new Panel { Width = 230, Height = 144, Margin = new Padding(0, 0, 12, 12), BackColor = EstiloPos.Surface };
            var barra = new Panel { Width = 4, Dock = DockStyle.Left, BackColor = acento };

            var lblTit = new Label
            {
                Text = titulo, AutoSize = false, Location = new Point(16, 12), Size = new Size(198, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = EstiloPos.Ink2, Font = EstiloPos.FontSmall
            };
            valor = new Label
            {
                Text = "—", AutoSize = false, Location = new Point(16, 56), Size = new Size(198, 62),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = acento, TextAlign = ContentAlignment.MiddleLeft, Font = EstiloPos.FontMetrica
            };

            card.Controls.Add(barra);
            card.Controls.Add(lblTit);
            card.Controls.Add(valor);
            var valorCard = valor;
            card.Resize += (s, e) => AjustarAnchoValor(valorCard);
            valorCard.TextChanged += (s, e) => AjustarAnchoValor(valorCard);
            valorCard.Disposed += (s, e) => (valorCard.Tag as Font)?.Dispose();
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(EstiloPos.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            return card;
        }

        private static void AjustarAnchoValor(Label valor)
        {
            if (valor.IsDisposed || valor.ClientSize.Width <= 0) return;
            float puntos = EstiloPos.FontMetrica.Size;
            while (puntos > 10F)
            {
                using (var prueba = new Font(EstiloPos.FontMetrica.FontFamily, puntos, EstiloPos.FontMetrica.Style))
                    if (TextRenderer.MeasureText(valor.Text, prueba).Width <= valor.ClientSize.Width) break;
                puntos -= 1F;
            }
            var anterior = valor.Tag as Font;
            if (anterior != null && anterior.Size == puntos) return;
            var fuente = new Font(EstiloPos.FontMetrica.FontFamily, puntos, EstiloPos.FontMetrica.Style);
            valor.Font = fuente;
            valor.Tag = fuente;
            anterior?.Dispose();
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

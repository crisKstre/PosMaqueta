using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dominio.Eventos;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public class FormDashboard : Form
    {
        private readonly VentaService    ventaService    = new VentaService();
        private readonly ProductoService productoService = new ProductoService();
        private readonly CajaService     cajaService     = new CajaService();

        private Label        lblCantVentas;
        private Label        lblTotalDia;
        private Label        lblBajoStockCount;
        private Label        lblEstadoCaja;
        private DataGridView dgvVentas;
        private DataGridView dgvStock;

        public FormDashboard()
        {
            this.Text = "Inicio";
            InitUI();
            NotificadorCambios.Cambio += OnCambioDatos;
        }

        private void InitUI()
        {
            this.BackColor = EstiloPos.Fondo;
            this.Load += FormDashboard_Load;

            // ── Saludo ────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = EstiloPos.Fondo };

            var lblFechaHoy = new Label
            {
                Text      = DateTime.Now.ToString("dddd d 'de' MMMM"),
                AutoSize  = false,
                Dock      = DockStyle.Right,
                Width     = 340,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = EstiloPos.Ink3,
                Font      = EstiloPos.FontBody,
                Padding   = new Padding(0, 0, 24, 0)
            };

            var lblSaludo = new Label
            {
                Text      = "Bienvenido, " + (Sesion.UsuarioActual?.Nombre ?? ""),
                AutoSize  = false,
                Dock      = DockStyle.Left,
                Width     = 500,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = EstiloPos.Ink1,
                Font      = EstiloPos.FontSubtitulo,
                Padding   = new Padding(24, 0, 0, 0)
            };

            pnlHeader.Controls.Add(lblFechaHoy);
            pnlHeader.Controls.Add(lblSaludo);

            // ── Cards ─────────────────────────────────────────────────────
            var pnlCards = new FlowLayoutPanel
            {
                Dock          = DockStyle.Top,
                Height        = 120,
                Padding       = new Padding(24, 0, 0, 20),
                BackColor     = EstiloPos.Fondo,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };

            Label t1, t2, t3, t4;
            var cVentas = CrearCard("Ventas hoy",    EstiloPos.Azul,  out t1, out lblCantVentas);
            var cTotal  = CrearCard("Total del dia", EstiloPos.Verde, out t2, out lblTotalDia);
            var cStock  = CrearCard("Bajo stock",    EstiloPos.Rojo,  out t3, out lblBajoStockCount);
            var cCaja   = CrearCard("Estado caja",   EstiloPos.Amber, out t4, out lblEstadoCaja);

            lblEstadoCaja.Font = EstiloPos.FontSubtitulo;

            pnlCards.Controls.Add(cVentas);
            pnlCards.Controls.Add(cTotal);
            pnlCards.Controls.Add(cStock);
            pnlCards.Controls.Add(cCaja);

            // ── Tablas ────────────────────────────────────────────────────
            var split = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                Padding     = new Padding(24, 0, 24, 24),
                BackColor   = EstiloPos.Fondo
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var pnlV = CrearPanelTabla("Ultimas ventas de hoy", out dgvVentas, new Padding(0, 0, 10, 0));
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "N°",            FillWeight = 25 });
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hora",          FillWeight = 30 });
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Medio de pago", FillWeight = 65 });
            dgvVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total",         FillWeight = 55 });
            EstiloPos.AplicarGrid(dgvVentas);

            var pnlS = CrearPanelTabla("Productos bajo stock minimo", out dgvStock, new Padding(10, 0, 0, 0));
            dgvStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", FillWeight = 120 });
            dgvStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock",    FillWeight = 55  });
            dgvStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Minimo",   FillWeight = 45  });
            EstiloPos.AplicarGrid(dgvStock);

            split.Controls.Add(pnlV, 0, 0);
            split.Controls.Add(pnlS, 1, 0);

            this.Controls.Add(split);
            this.Controls.Add(pnlCards);
            this.Controls.Add(pnlHeader);
        }

        private Panel CrearCard(string titulo, Color colorAcento, out Label lblTitulo, out Label lblValor)
        {
            var card = new Panel
            {
                Width     = 224,
                Height    = 90,
                Margin    = new Padding(0, 0, 16, 0),
                BackColor = EstiloPos.Surface
            };

            var barra = new Panel { Width = 4, Dock = DockStyle.Left, BackColor = colorAcento };

            lblTitulo = new Label
            {
                Text      = titulo,
                AutoSize  = false,
                Location  = new Point(16, 10),
                Size      = new Size(202, 20),
                ForeColor = EstiloPos.Ink2,
                Font      = EstiloPos.FontSmall
            };

            lblValor = new Label
            {
                Text      = "—",
                AutoSize  = false,
                Location  = new Point(16, 34),
                Size      = new Size(202, 48),
                ForeColor = colorAcento,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = EstiloPos.FontMetrica
            };

            card.Controls.Add(barra);
            card.Controls.Add(lblTitulo);
            card.Controls.Add(lblValor);
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
                Text      = titulo,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = EstiloPos.Ink1,
                Font      = EstiloPos.FontLabel,
                Padding   = new Padding(16, 0, 0, 0)
            };
            pnlTit.Controls.Add(lbl);

            dgv = new DataGridView { Dock = DockStyle.Fill };

            inner.Controls.Add(dgv);
            inner.Controls.Add(pnlTit);
            outer.Controls.Add(inner);
            return outer;
        }

        private void FormDashboard_Load(object sender, EventArgs e) => CargarDatos();

        private void CargarDatos()
        {
            CargarVentasHoy();
            CargarBajoStock();
            ActualizarEstadoCaja();
        }

        private void CargarVentasHoy()
        {
            var ventas = ventaService.ObtenerVentasHoy();
            dgvVentas.Rows.Clear();
            foreach (var v in ventas)
                dgvVentas.Rows.Add("#" + v.IdVenta, v.Fecha.ToString("HH:mm"), v.MedioPago, "$" + v.Total.ToString("N0"));
            lblCantVentas.Text = ventas.Count.ToString();
            lblTotalDia.Text   = "$" + ventas.Sum(v => v.Total).ToString("N0");
        }

        private void CargarBajoStock()
        {
            var productos = productoService.ObtenerBajoStock();
            dgvStock.Rows.Clear();
            foreach (var p in productos)
                dgvStock.Rows.Add(p.Nombre, p.Stock.ToString("0.##") + " " + p.UnidadMedida, p.StockMinimo.ToString("0.##"));
            lblBajoStockCount.Text      = productos.Count.ToString();
            lblBajoStockCount.ForeColor = productos.Count > 0 ? EstiloPos.Rojo : EstiloPos.Verde;
        }

        private void ActualizarEstadoCaja()
        {
            var caja = cajaService.ObtenerCajaAbierta();
            lblEstadoCaja.Text      = caja != null ? "Abierta" : "Cerrada";
            lblEstadoCaja.ForeColor = caja != null ? EstiloPos.Verde : EstiloPos.Rojo;
        }

        private void OnCambioDatos(string entidad)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (entidad == Entidad.Venta || entidad == Entidad.Producto || entidad == Entidad.Caja)
                BeginInvoke(new Action(CargarDatos));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            NotificadorCambios.Cambio -= OnCambioDatos;
            base.OnFormClosed(e);
        }
    }
}

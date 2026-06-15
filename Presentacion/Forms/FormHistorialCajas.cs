using System;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    /// <summary>Diálogo con el histórico de turnos de caja y sus arqueos (solo lectura).</summary>
    public class FormHistorialCajas : Form
    {
        private readonly CajaService cajaService = new CajaService();
        private DateTimePicker dtpDesde, dtpHasta;
        private DataGridView dgv;

        public FormHistorialCajas()
        {
            InitUI();
            // Por defecto: el mes en curso
            SetRango(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today);
        }

        private void InitUI()
        {
            this.Text = "Histórico de cajas";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowIcon = false; this.ShowInTaskbar = false;
            this.Size = new Size(1000, 580);
            this.MinimumSize = new Size(760, 420);
            this.BackColor = EstiloPos.Fondo;

            // ── Header con filtro de fechas ────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 116, BackColor = EstiloPos.Fondo };

            var lblTit = new Label
            {
                Text = "Histórico de cajas",
                Font = EstiloPos.FontTitulo, ForeColor = EstiloPos.Ink1,
                AutoSize = true, Location = new Point(24, 16)
            };

            var lblDesde = new Label { Text = "Desde", Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(24, 58) };
            dtpDesde = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = EstiloPos.FontBody,
                Location = new Point(24, 78), Size = new Size(150, 30) };

            var lblHasta = new Label { Text = "Hasta", Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(190, 58) };
            dtpHasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = EstiloPos.FontBody,
                Location = new Point(190, 78), Size = new Size(150, 30) };

            var btnVer = new Button { Text = "Ver", Location = new Point(356, 77), Size = new Size(90, 32) };
            EstiloPos.AplicarBotonPrimario(btnVer);
            btnVer.Click += (s, e) => Cargar();

            var btnHoy    = CrearChip("Hoy",      () => SetRango(DateTime.Today, DateTime.Today));
            var btnSemana = CrearChip("7 días",   () => SetRango(DateTime.Today.AddDays(-6), DateTime.Today));
            var btnMes    = CrearChip("Este mes", () => SetRango(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today));
            var btnTodo   = CrearChip("Todo",     () => SetRango(new DateTime(2000, 1, 1), DateTime.Today));
            btnHoy.Location    = new Point(470, 78);
            btnSemana.Location = new Point(btnHoy.Right + 8, 78);
            btnMes.Location    = new Point(btnSemana.Right + 8, 78);
            btnTodo.Location   = new Point(btnMes.Right + 8, 78);

            pnlHeader.Controls.AddRange(new Control[] {
                lblTit, lblDesde, dtpDesde, lblHasta, dtpHasta, btnVer, btnHoy, btnSemana, btnMes, btnTodo });

            // ── Tabla ──────────────────────────────────────────────────────
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = EstiloPos.Fondo, Padding = new Padding(24, 0, 24, 24) };
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = EstiloPos.Surface };
            inner.Paint += (s, e) =>
            {
                using (var pen = new Pen(EstiloPos.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, inner.Width - 1, inner.Height - 1);
            };

            dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.Columns.Add(Col("Apertura",   95));
            dgv.Columns.Add(Col("Cierre",     95));
            dgv.Columns.Add(Col("Usuario",    90));
            dgv.Columns.Add(Col("Inicial",    65));
            dgv.Columns.Add(Col("Esperado",   70));
            dgv.Columns.Add(Col("Contado",    70));
            dgv.Columns.Add(Col("Diferencia", 80));
            dgv.Columns.Add(Col("Estado",     60));
            EstiloPos.AplicarGrid(dgv);

            inner.Controls.Add(dgv);
            outer.Controls.Add(inner);
            this.Controls.Add(outer);
            this.Controls.Add(pnlHeader);
        }

        private DataGridViewTextBoxColumn Col(string header, int peso)
            => new DataGridViewTextBoxColumn { HeaderText = header, FillWeight = peso };

        private void SetRango(DateTime desde, DateTime hasta)
        {
            dtpDesde.Value = desde;
            dtpHasta.Value = hasta;
            Cargar();
        }

        private void Cargar()
        {
            DateTime desde = dtpDesde.Value.Date, hasta = dtpHasta.Value.Date;
            if (hasta < desde) { var t = desde; desde = hasta; hasta = t; }

            dgv.Rows.Clear();
            foreach (var c in cajaService.ObtenerHistorial(desde, hasta))
            {
                bool cerrada = c.Estado == EstadoCaja.Cerrada;
                decimal dif = c.MontoReal - c.MontoEsperado;
                string difTxt = !cerrada ? "—"
                    : dif == 0 ? "$0"
                    : dif > 0  ? "+$" + dif.ToString("N0")
                    :            "-$" + Math.Abs(dif).ToString("N0");

                int fila = dgv.Rows.Add(
                    c.FechaApertura.ToString("dd/MM/yy HH:mm"),
                    c.FechaCierre.HasValue ? c.FechaCierre.Value.ToString("dd/MM/yy HH:mm") : "—",
                    c.NombreUsuario,
                    "$" + c.MontoInicial.ToString("N0"),
                    cerrada ? "$" + c.MontoEsperado.ToString("N0") : "—",
                    cerrada ? "$" + c.MontoReal.ToString("N0") : "—",
                    difTxt,
                    cerrada ? "Cerrada" : "Abierta");

                var celdaDif = dgv.Rows[fila].Cells[6];
                if (cerrada && dif < 0)      celdaDif.Style.ForeColor = EstiloPos.Rojo;
                else if (cerrada && dif > 0) celdaDif.Style.ForeColor = EstiloPos.Amber;
                else if (cerrada)            celdaDif.Style.ForeColor = EstiloPos.Verde;

                if (!cerrada)
                    dgv.Rows[fila].Cells[7].Style.ForeColor = EstiloPos.Verde;
            }

            if (dgv.Rows.Count == 0)
                dgv.Rows.Add("—", "—", "Sin turnos en este período", "", "", "", "", "");
        }

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
    }
}

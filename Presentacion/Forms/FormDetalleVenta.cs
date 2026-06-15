using System;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    /// <summary>Diálogo de solo lectura con los ítems vendidos en una venta (incluye código).</summary>
    public class FormDetalleVenta : Form
    {
        private readonly VentaService ventaService = new VentaService();

        public FormDetalleVenta(Venta venta)
        {
            InitUI(venta);
        }

        private void InitUI(Venta v)
        {
            this.Text = "Detalle de venta N° " + v.IdVenta;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.ShowIcon = false; this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(640, 520);
            this.MinimumSize = new Size(520, 380);
            this.BackColor = EstiloPos.Fondo;

            // Encabezado
            var pnlTit = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = EstiloPos.Fondo };
            var lblTit = new Label
            {
                Text = "Venta N° " + v.IdVenta,
                Font = EstiloPos.FontTitulo, ForeColor = EstiloPos.Ink1,
                AutoSize = true, Location = new Point(24, 18)
            };
            var lblInfo = new Label
            {
                Text = v.Fecha.ToString("dd/MM/yyyy  HH:mm") + "      ·      " + v.MedioPago,
                Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink2,
                AutoSize = true, Location = new Point(24, 52)
            };
            pnlTit.Controls.Add(lblTit);
            pnlTit.Controls.Add(lblInfo);

            // Pie: totales con desglose de IVA + botón cerrar
            var pnlPie = new Panel { Dock = DockStyle.Bottom, Height = 92, BackColor = EstiloPos.Fondo, Padding = new Padding(24, 6, 24, 14) };
            string textoTotales = "";
            if (v.Descuento > 0)
                textoTotales = "Subtotal:  $" + (v.Total + v.Descuento).ToString("N0") +
                               "        Descuento:  -$" + v.Descuento.ToString("N0") + "\n";
            textoTotales += "Neto:  $" + Impuestos.Neto(v.Total).ToString("N0") +
                            "        IVA 19%:  $" + Impuestos.Iva(v.Total).ToString("N0") +
                            "\nTOTAL:  $" + v.Total.ToString("N0");

            var lblTotales = new Label
            {
                Dock = DockStyle.Fill,
                Font = EstiloPos.FontBody, ForeColor = EstiloPos.Ink1,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = textoTotales
            };
            var btnCerrar = new Button { Text = "Cerrar", Dock = DockStyle.Right, Width = 130 };
            EstiloPos.AplicarBotonPrimario(btnCerrar);
            btnCerrar.Click += (s, e) => Close();
            pnlPie.Controls.Add(lblTotales);
            pnlPie.Controls.Add(btnCerrar);

            // Grid de ítems
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = EstiloPos.Fondo, Padding = new Padding(24, 0, 24, 0) };
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = EstiloPos.Surface };
            inner.Paint += (s, e) =>
            {
                using (var pen = new Pen(EstiloPos.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, inner.Width - 1, inner.Height - 1);
            };

            var dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Código",   FillWeight = 75 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", FillWeight = 130 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cant.",    FillWeight = 38 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "P. Unit.", FillWeight = 55 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Subtotal", FillWeight = 60 });
            EstiloPos.AplicarGrid(dgv);

            foreach (var d in ventaService.ObtenerDetalleVenta(v.IdVenta))
                dgv.Rows.Add(
                    string.IsNullOrEmpty(d.CodigoBarras) ? "—" : d.CodigoBarras,
                    d.NombreProducto,
                    d.Cantidad.ToString("0.##"),
                    "$" + d.PrecioUnitario.ToString("N0"),
                    "$" + d.Subtotal.ToString("N0"));

            inner.Controls.Add(dgv);
            outer.Controls.Add(inner);
            this.Controls.Add(outer);
            this.Controls.Add(pnlPie);
            this.Controls.Add(pnlTit);
        }
    }
}

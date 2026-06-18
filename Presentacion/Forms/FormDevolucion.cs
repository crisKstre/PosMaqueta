using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    /// <summary>
    /// Devolución parcial: el admin elige cuánto devolver de cada ítem de una venta. Reintegra stock
    /// y registra la salida de efectivo en la caja abierta (vía DevolucionService).
    /// </summary>
    public class FormDevolucion : Form
    {
        private readonly DevolucionService service = new DevolucionService();
        private readonly int idVenta;
        private List<DevolucionItem> devolvibles = new List<DevolucionItem>();

        private DataGridView dgv;
        private Label lblTotal;
        private Button btnConfirmar;

        public FormDevolucion(int idVenta)
        {
            this.idVenta = idVenta;
            InitUI();
            Cargar();
        }

        private void InitUI()
        {
            this.Text = "Devolución — Venta N° " + idVenta;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.ShowIcon = false; this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(620, 472);
            this.BackColor = EstiloPos.Fondo;

            var lblTit = new Label { Text = "Indica cuánto devolver de cada producto", Font = EstiloPos.FontSubtitulo,
                ForeColor = EstiloPos.Ink1, AutoSize = false, Size = new Size(580, 30), Location = new Point(20, 16) };

            dgv = new DataGridView { Location = new Point(20, 52), Size = new Size(580, 286) };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", Visible = false, ReadOnly = true });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombre", HeaderText = "Producto",   FillWeight = 46, ReadOnly = true });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDisp",   HeaderText = "Disponible", FillWeight = 18, ReadOnly = true });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrecio", HeaderText = "Precio",     FillWeight = 18, ReadOnly = true });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDevolver", HeaderText = "Devolver *", FillWeight = 18 });
            EstiloPos.AplicarGrid(dgv);

            // SOLO la columna resaltada "Devolver" es editable. AplicarGrid deja el grid en
            // ReadOnly=true; lo reabrimos y forzamos explícitamente el ReadOnly de CADA columna,
            // sin depender del orden de alta ni de cómo el framework restaura los flags al togglear
            // el ReadOnly del grid (gotcha clásico de WinForms que dejaba celdas editables).
            dgv.ReadOnly = false;
            foreach (DataGridViewColumn col in dgv.Columns)
                col.ReadOnly = (col.Name != "colDevolver");
            dgv.EditMode = DataGridViewEditMode.EditOnEnter;
            dgv.CellEndEdit += (s, e) => Recalcular();

            // Defensa en profundidad: aunque por cualquier motivo una celda quedara editable, se
            // cancela la edición de toda columna que no sea "Devolver". Imposible editar otra cosa.
            dgv.CellBeginEdit += (s, e) =>
            {
                if (dgv.Columns[e.ColumnIndex].Name != "colDevolver") e.Cancel = true;
            };

            // Resalta la ÚNICA columna editable (fondo ámbar claro), para que no parezca que se
            // puede editar toda la tabla.
            var estiloEdit = dgv.Columns["colDevolver"].DefaultCellStyle;
            estiloEdit.BackColor          = Color.FromArgb(255, 248, 225);
            estiloEdit.SelectionBackColor = Color.FromArgb(255, 236, 179);
            estiloEdit.ForeColor          = EstiloPos.Ink1;
            estiloEdit.SelectionForeColor = EstiloPos.Ink1;
            estiloEdit.Alignment          = DataGridViewContentAlignment.MiddleRight;
            estiloEdit.Font               = new Font("Segoe UI", 12.5F, FontStyle.Bold);

            var lblHint = new Label { Text = "* Escribe en la columna resaltada «Devolver» cuánto devolver de cada producto (0 = ninguno).",
                Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink3, AutoSize = false,
                Size = new Size(580, 20), Location = new Point(20, 344) };

            lblTotal = new Label { Text = "A reembolsar: $0", Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = EstiloPos.Ink1, AutoSize = false, Size = new Size(560, 30), Location = new Point(20, 372) };

            btnConfirmar = new Button { Text = "Confirmar devolución", Size = new Size(200, 46), Location = new Point(296, 408) };
            EstiloPos.AplicarBotonPrimario(btnConfirmar);
            var btnCancelar = new Button { Text = "Cancelar", Size = new Size(92, 46), Location = new Point(508, 408) };
            EstiloPos.AplicarBotonSecundario(btnCancelar);
            btnConfirmar.Click += (s, e) => Confirmar();
            btnCancelar.Click  += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.AddRange(new Control[] { lblTit, dgv, lblHint, lblTotal, btnConfirmar, btnCancelar });
            this.CancelButton = btnCancelar;
        }

        private void Cargar()
        {
            devolvibles = service.ObtenerDevolvibles(idVenta);
            dgv.Rows.Clear();
            foreach (var it in devolvibles)
                dgv.Rows.Add(it.IdProducto, it.NombreProducto, it.Cantidad.ToString("0.##"),
                    "$" + it.PrecioUnitario.ToString("N0"), "0");

            if (devolvibles.Count == 0)
            {
                lblTotal.Text = "No quedan productos por devolver de esta venta.";
                btnConfirmar.Enabled = false;
            }
            Recalcular();
        }

        private void Recalcular()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                var it = devolvibles.FirstOrDefault(d => d.IdProducto == Convert.ToInt32(row.Cells["colId"].Value));
                if (it == null) continue;
                decimal cant = ParseCant(row.Cells["colDevolver"].Value);
                if (cant < 0) cant = 0;
                if (cant > it.Cantidad) { cant = it.Cantidad; row.Cells["colDevolver"].Value = cant.ToString("0.##"); }
                total += Dinero.Redondear(cant * it.PrecioUnitario);
            }
            if (devolvibles.Count > 0) lblTotal.Text = "A reembolsar: $" + total.ToString("N0");
            btnConfirmar.Enabled = total > 0;
        }

        private void Confirmar()
        {
            var items = new List<DevolucionItem>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                decimal cant = ParseCant(row.Cells["colDevolver"].Value);
                if (cant <= 0) continue;
                items.Add(new DevolucionItem { IdProducto = Convert.ToInt32(row.Cells["colId"].Value), Cantidad = cant });
            }
            if (items.Count == 0) { Aviso.Info(this, "Indica una cantidad a devolver.", "Devolución"); return; }

            decimal totalRef = items.Sum(i => Dinero.Redondear(
                i.Cantidad * devolvibles.First(d => d.IdProducto == i.IdProducto).PrecioUnitario));
            if (!Aviso.Confirmar(this,
                    "Se devolverán " + items.Count + " producto(s) por $" + totalRef.ToString("N0") + ".\n" +
                    "El stock vuelve al inventario y el efectivo sale de la caja del turno.",
                    "¿Registrar devolución?", "Devolver", TipoAviso.Advertencia))
                return;
            try
            {
                int id = service.Devolver(idVenta, items);
                Aviso.Exito(this, "Devolución N° " + id + " registrada. Stock reintegrado y $" +
                    totalRef.ToString("N0") + " descontado del efectivo de la caja.", "Devolución registrada");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo devolver"); }
        }

        private static decimal ParseCant(object v)
        {
            decimal.TryParse(v?.ToString(), out var c);
            return c;
        }
    }
}

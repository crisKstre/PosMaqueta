using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormVentas : Form
    {
        private readonly VentaService    ventaService    = new VentaService();
        private readonly ProductoService productoService = new ProductoService();
        private readonly CategoriaService categoriaService = new CategoriaService();
        private readonly LogService      logService      = new LogService();
        private bool logVentasVisible = false;
        private List<Producto> sugerencias = new List<Producto>();
        private string categoriaActual = "";

        public FormVentas() { InitializeComponent(); }

        private void FormVentas_Load(object sender, EventArgs e)
        {
            // Aplicar estilos en runtime (garantiza que corre después del layout)
            AplicarEstilos();

            comboMedioPago.Items.AddRange(new object[] {
                MedioPago.Efectivo, MedioPago.Tarjeta, MedioPago.Transferencia });
            comboMedioPago.SelectedIndex = 0;
            dtpDesdeV.Value = DateTime.Today;
            dtpHastaV.Value = DateTime.Today;

            CargarCategorias();
            CargarGridProductos();
            RefrescarCarrito();
            txtCodigo.Focus();
        }

        private void AplicarEstilos()
        {
            AplicarEstilosGrids();

            // Combo medio de pago
            EstiloPos.AplicarCombo(comboMedioPago);

            // Botones del panel de cobro
            EstiloPos.AplicarBotonPrimario(btnCobrar, grande: true);
            EstiloPos.AplicarBotonSecundario(btnCancelar, EstiloPos.Rojo);
            EstiloPos.AplicarBotonSecundario(btnVerLog);

            // Botón filtrar del registro de ventas
            EstiloPos.AplicarBotonSecundario(btnFiltrarLogV);
            btnFiltrarLogV.Size = new Size(90, 30);

            // btnAgregar ya viene estilizado desde el Designer (alto 44, alineado con los inputs)
        }

        private void ConfigColAccion(DataGridViewColumn col, int ancho, float fontSize, Color fg, Color sel)
        {
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            col.Width        = ancho;
            col.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment          = DataGridViewContentAlignment.MiddleCenter,
                Font               = new Font("Segoe UI", fontSize, FontStyle.Bold),
                ForeColor          = fg,
                SelectionForeColor = fg,
                Padding            = new Padding(0),
                BackColor          = EstiloPos.Surface,
                SelectionBackColor = sel
            };
        }

        private void AplicarEstilosGrids()
        {
            // dgvCarrito
            EstiloPos.AplicarGrid(dgvCarrito);
            dgvCarrito.RowTemplate.Height = 38;
            dgvCarrito.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 228, 245);
            dgvCarrito.DefaultCellStyle.SelectionForeColor = EstiloPos.Ink1;

            // Columnas de acción del carrito: ancho fijo y centradas para que no se trunquen.
            // Solo "Producto" usa el espacio flexible (Fill); el resto va con ancho fijo.
            Color colSel = Color.FromArgb(220, 228, 245);
            ConfigColAccion(colMenos,  38, 13F, EstiloPos.Ink1,  colSel);
            ConfigColAccion(colMas,    38, 13F, EstiloPos.Verde, colSel);
            ConfigColAccion(colQuitar, 38, 11F, EstiloPos.Rojo,  colSel);

            colVCantidad.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colVCantidad.Width        = 46;
            colVCantidad.DefaultCellStyle = new DataGridViewCellStyle {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = EstiloPos.FontTabla,
                ForeColor = EstiloPos.Ink1, SelectionForeColor = EstiloPos.Ink1,
                BackColor = EstiloPos.Surface, SelectionBackColor = colSel };

            colVSubtotal.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colVSubtotal.Width        = 82;
            colVSubtotal.DefaultCellStyle = new DataGridViewCellStyle {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Font = EstiloPos.FontTabla,
                ForeColor = EstiloPos.Ink1, SelectionForeColor = EstiloPos.Ink1,
                Padding = new Padding(0, 0, 8, 0),
                BackColor = EstiloPos.Surface, SelectionBackColor = colSel };

            // dgvLogVentas
            EstiloPos.AplicarGrid(dgvLogVentas);
        }

        // ── Categorías (pills) ────────────────────────────────────────

        private void CargarCategorias()
        {
            pnlCats.Controls.Clear();
            AgregarPill("Todos", "");
            // Usa la tabla Categoria — así los nuevos agregados desde Productos aparecen aquí también
            foreach (var c in categoriaService.ObtenerTodas())
                AgregarPill(c.Nombre, c.Nombre);
            ActivarPill("Todos");
        }

        private void AgregarPill(string texto, string valor)
        {
            var btn = new Button
            {
                Text      = texto,
                Tag       = valor,
                FlatStyle = FlatStyle.Flat,
                Font      = EstiloPos.FontSmall,
                BackColor = EstiloPos.Surface,
                ForeColor = EstiloPos.Ink3,
                Cursor    = Cursors.Hand,
                AutoSize  = true,
                Padding   = new Padding(14, 0, 14, 0),
                Height    = 30,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderColor = EstiloPos.Border;
            btn.FlatAppearance.BorderSize  = 1;
            btn.Click += (s, e) => {
                categoriaActual = valor;
                ActivarPill(texto);
                CargarGridProductos();
            };
            pnlCats.Controls.Add(btn);
        }

        private void ActivarPill(string texto)
        {
            foreach (Control c in pnlCats.Controls)
            {
                if (c is Button b)
                {
                    bool activo = b.Text == texto;
                    b.BackColor = activo ? EstiloPos.Ink1 : EstiloPos.Surface;
                    b.ForeColor = activo ? Color.White    : EstiloPos.Ink3;
                    b.FlatAppearance.BorderColor = activo ? EstiloPos.Ink1 : EstiloPos.Border;
                }
            }
        }

        // ── Grid de productos (tarjetas clickeables) ───────────────────

        private void CargarGridProductos(string busqueda = "")
        {
            pnlProdGrid.Controls.Clear();
            pnlProdGrid.SuspendLayout();

            List<Producto> productos;
            if (!string.IsNullOrEmpty(busqueda))
                productos = productoService.Buscar(busqueda).FindAll(p => p.Activo);
            else if (!string.IsNullOrEmpty(categoriaActual))
                productos = productoService.ObtenerPorCategoria(categoriaActual);
            else
                productos = productoService.ObtenerActivos();

            foreach (var p in productos)
                pnlProdGrid.Controls.Add(CrearTileProducto(p));

            pnlProdGrid.ResumeLayout();

            // Forzar cálculo de tamaño DESPUÉS de que WinForms termine el layout pass
            pnlProdGrid.BeginInvoke(new Action(AjustarTamañoTiles));
        }

        private void pnlProdGrid_Resize(object sender, EventArgs e)
        {
            AjustarTamañoTiles();
        }

        private void AjustarTamañoTiles()
        {
            if (pnlProdGrid.Controls.Count == 0) return;
            int cols  = 4;
            int gap   = 8;
            int ancho = (pnlProdGrid.ClientSize.Width - (gap * cols)) / cols;
            if (ancho < 100) return;
            pnlProdGrid.SuspendLayout();
            foreach (Control c in pnlProdGrid.Controls)
                c.Size = new Size(ancho, 100);
            pnlProdGrid.ResumeLayout();
        }

        private Panel CrearTileProducto(Producto p)
        {
            bool bajoStock = productoService.EstaBajoStock(p);

            var tile = new Panel
            {
                Tag       = p,
                BackColor = bajoStock ? Color.FromArgb(255, 253, 245) : EstiloPos.Surface,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 8, 8),
                Padding   = new Padding(10, 8, 10, 8),
                Size      = new Size(220, 100)  // tamaño inicial — AjustarTamañoTiles lo corrige
            };

            // Borde pintado con el color del sistema de diseño
            tile.Paint += (s, pe) =>
            {
                using (var pen = new System.Drawing.Pen(bajoStock
                    ? EstiloPos.Amber : EstiloPos.Border, 1.5f))
                {
                    var r = pe.ClipRectangle;
                    pe.Graphics.DrawRectangle(pen, 0, 0, tile.Width - 1, tile.Height - 1);
                }
            };

            var lblNombre = new Label
            {
                Text      = p.Nombre,
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = EstiloPos.Ink1,
                AutoSize  = false,
                Location  = new Point(10, 8),
                Size      = new Size(tile.Width - 20, 38),
                TextAlign = ContentAlignment.TopLeft
            };
            var lblPrecio = new Label
            {
                Text      = "$" + p.Precio.ToString("N0"),
                Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = EstiloPos.Ink1,
                AutoSize  = true,
                Location  = new Point(10, 48)
            };
            var lblStock = new Label
            {
                Text      = bajoStock ? "⚠ " + p.Stock.ToString("0.##") + " " + p.UnidadMedida
                                      : p.Stock.ToString("0.##") + " " + p.UnidadMedida,
                Font      = new Font("Segoe UI", 10F),
                ForeColor = bajoStock ? EstiloPos.Amber : EstiloPos.Ink3,
                AutoSize  = true,
                Location  = new Point(10, 74)
            };

            tile.Controls.AddRange(new System.Windows.Forms.Control[] { lblNombre, lblPrecio, lblStock });

            // Click => agregar al carrito
            EventHandler clickHandler = (s, e) => AgregarProductoAlCarrito(p);
            tile.Click       += clickHandler;
            lblNombre.Click  += clickHandler;
            lblPrecio.Click  += clickHandler;
            lblStock.Click   += clickHandler;

            return tile;
        }

        private void AgregarProductoAlCarrito(Producto p)
        {
            if (!decimal.TryParse(txtCantidad.Text, out decimal cant) || cant <= 0) cant = 1;
            try
            {
                ventaService.AgregarPorId(p.IdProducto, cant);
                RefrescarCarrito();
                txtCantidad.Text = "1";
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
        }

        // ── Scanner / Código de barras ─────────────────────────────────

        private void txtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AgregarPorCodigo();
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e) => AgregarPorCodigo();

        private void AgregarPorCodigo()
        {
            string codigo = txtCodigo.Text.Trim();
            if (string.IsNullOrEmpty(codigo)) return;
            if (!decimal.TryParse(txtCantidad.Text, out decimal cant) || cant <= 0) cant = 1;
            try
            {
                ventaService.AgregarPorCodigo(codigo, cant);
                RefrescarCarrito();
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
            txtCodigo.Clear();
            txtCantidad.Text = "1";
            txtCodigo.Focus();
        }

        // ── Buscador por nombre ────────────────────────────────────────

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            string texto = txtBuscar.Text.Trim();
            if (texto.Length == 0)
            {
                lstSugerencias.Visible = false;
                CargarGridProductos();
                return;
            }
            sugerencias = ventaService.BuscarProductos(texto);
            lstSugerencias.Items.Clear();
            foreach (var p in sugerencias)
                lstSugerencias.Items.Add(p.Nombre + "   $" + p.Precio.ToString("N0")
                    + "   stock: " + p.Stock.ToString("0.##") + " " + p.UnidadMedida);
            lstSugerencias.Visible = sugerencias.Count > 0;
            if (sugerencias.Count > 0) lstSugerencias.SelectedIndex = 0;
            CargarGridProductos(texto);
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstSugerencias.Visible)
            { lstSugerencias.Focus(); lstSugerencias.SelectedIndex = 0; e.SuppressKeyPress = true; }
        }

        private void lstSugerencias_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { PedirCantidadYAgregar(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) { lstSugerencias.Visible = false; txtBuscar.Focus(); }
        }
        private void lstSugerencias_Click(object sender, EventArgs e) { }
        private void lstSugerencias_DoubleClick(object sender, EventArgs e) => PedirCantidadYAgregar();

        private void PedirCantidadYAgregar()
        {
            int i = lstSugerencias.SelectedIndex;
            if (i < 0 || i >= sugerencias.Count) return;
            decimal cantidad = LeerCantidad();
            if (txtCantidad.Text.Trim() == "" || txtCantidad.Text.Trim() == "1")
            {
                string input = Interaction.InputBox(
                    "¿Cuántas unidades de \"" + sugerencias[i].Nombre + "\"?", "Cantidad", "1");
                if (string.IsNullOrEmpty(input)) return;
                if (!decimal.TryParse(input, out cantidad) || cantidad <= 0)
                { MostrarMensaje("Cantidad inválida."); return; }
            }
            try
            {
                ventaService.AgregarPorId(sugerencias[i].IdProducto, cantidad);
                RefrescarCarrito();
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
            txtBuscar.Clear();
            lstSugerencias.Visible = false;
            txtCantidad.Text = "1";
            txtCodigo.Focus();
        }

        // ── Carrito ────────────────────────────────────────────────────

        private void RefrescarCarrito()
        {
            dgvCarrito.Rows.Clear();
            foreach (var d in ventaService.Carrito)
                dgvCarrito.Rows.Add(
                    d.IdProducto,
                    d.NombreProducto,
                    "−",
                    d.Cantidad.ToString("0.##"),
                    "+",
                    "$" + d.Subtotal.ToString("N0"),
                    "✕");
            lblTotal.Text      = "$" + ventaService.Total.ToString("N0");
            lblMensaje.Visible = false;
        }

        private void dgvCarrito_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string col = dgvCarrito.Columns[e.ColumnIndex].Name;
            int id = Convert.ToInt32(dgvCarrito.Rows[e.RowIndex].Cells["colVId"].Value);
            try
            {
                if (col == "colQuitar")     ventaService.QuitarDelCarrito(id);
                else if (col == "colMenos") ventaService.AjustarCantidadCarrito(id, -1);
                else if (col == "colMas")   ventaService.AjustarCantidadCarrito(id, +1);
                else return;
                RefrescarCarrito();
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
        }

        // ── Cobro ──────────────────────────────────────────────────────

        private void btnCobrar_Click(object sender, EventArgs e)
        {
            if (ventaService.Carrito.Count == 0) { MostrarMensaje("El carrito está vacío."); return; }
            string  medioPago = comboMedioPago.SelectedItem.ToString();
            decimal total     = ventaService.Total;
            try
            {
                int idVenta = ventaService.CobrarVenta(Sesion.UsuarioActual.IdUsuario, medioPago);
                Aviso.Exito(this,
                    "Venta N° " + idVenta + "  ·  $" + total.ToString("N0") + "\nMedio de pago: " + medioPago,
                    "Venta registrada");
                RefrescarCarrito();
                CargarGridProductos();
                txtCodigo.Focus();
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (ventaService.Carrito.Count == 0) return;
            if (Aviso.Confirmar(this, "Se vaciará el carrito y se quitarán todos los productos agregados.",
                    "¿Cancelar la venta?", "Sí, cancelar"))
            { ventaService.VaciarCarrito(); RefrescarCarrito(); }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F12) { btnCobrar_Click(this, EventArgs.Empty); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ── Log ────────────────────────────────────────────────────────

        private void btnVerLog_Click(object sender, EventArgs e)
        {
            logVentasVisible  = !logVentasVisible;
            splitterLogV.Visible  = logVentasVisible;
            pnlLogVentas.Visible  = logVentasVisible;
            btnVerLog.Text    = logVentasVisible ? "▲ Ocultar registro" : "▼ Registro de ventas";
            if (logVentasVisible) CargarLogVentas();
        }

        private void CargarLogVentas()
        {
            var lista = logService.Obtener(dtpDesdeV.Value.Date, dtpHastaV.Value.Date, modulo: ModuloLog.Ventas);
            dgvLogVentas.Rows.Clear();
            foreach (var l in lista)
                dgvLogVentas.Rows.Add(l.Fecha.ToString("dd/MM HH:mm"), l.NombreUsuario, l.Accion, l.Detalle);
        }

        private void btnFiltrarLogV_Click(object sender, EventArgs e) => CargarLogVentas();

        // ── Utilidades ─────────────────────────────────────────────────

        private decimal LeerCantidad()
        {
            return decimal.TryParse(txtCantidad.Text, out decimal c) && c > 0 ? c : 1;
        }

        private void MostrarMensaje(string msg) { lblMensaje.Text = msg; lblMensaje.Visible = true; }
    }
}

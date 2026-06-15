using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Eventos;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormProductos : Form
    {
        private readonly ProductoService    productoService    = new ProductoService();
        private readonly CategoriaService   categoriaService   = new CategoriaService();
        private readonly LogService         logService         = new LogService();
        private int  idEnEdicion = 0;
        private bool logVisible  = false;

        public FormProductos() { InitializeComponent(); }

        private void FormProductos_Load(object sender, EventArgs e)
        {
            AplicarEstilos();

            // Re-aplicar grids en runtime para garantizar estilos correctos
            EstiloPos.AplicarGrid(dgvProductos);
            EstiloPos.AplicarGrid(dgvLog);

            comboUnidad.Items.AddRange(new object[] { UnidadMedida.Unidad, UnidadMedida.Kilogramo });
            comboUnidad.SelectedIndex = 0;
            dtpDesde.Value = DateTime.Today;
            dtpHasta.Value = DateTime.Today;
            NotificadorCambios.Cambio += OnCambioDatos;
            CargarCategorias();
            CargarProductos();
        }

        private void AplicarEstilos()
        {
            // Fondo y panels
            this.BackColor         = EstiloPos.Fondo;
            pnlFormulario.BackColor = EstiloPos.Surface;
            pnlAcciones.BackColor  = EstiloPos.Fondo;
            splitterLog.BackColor  = EstiloPos.Border;

            // Modo label
            lblModo.Font      = EstiloPos.FontSubtitulo;
            lblModo.ForeColor = EstiloPos.Ink1;

            // Labels de campo
            foreach (var l in new[] { lblCodigo, lblNombre, lblCategoria, lblPrecio,
                                       lblStock, lblStockMin, lblUnidad })
            {
                l.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                l.ForeColor = EstiloPos.Ink2;
            }
            foreach (var l in new[] { lblBuscar, lblCantidad })
            {
                l.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                l.ForeColor = EstiloPos.Ink2;
            }
            foreach (var l in new[] { lblDesde, lblHasta })
            {
                l.Font      = EstiloPos.FontSmall;
                l.ForeColor = EstiloPos.Ink2;
            }

            // Inputs fila 1 y 2
            foreach (var t in new[] { txtCodigo, txtNombre, txtPrecio, txtStock, txtStockMin })
                EstiloPos.AplicarInput(t);
            foreach (var t in new[] { txtBuscar, txtCantidad })
            {
                EstiloPos.AplicarInput(t);
                t.Height = 40;
                t.Font   = new System.Drawing.Font("Segoe UI", 12F);
            }

            // Combos
            EstiloPos.AplicarCombo(comboCategoria);
            EstiloPos.AplicarCombo(comboUnidad);
            comboCategoria.Size = new System.Drawing.Size(210, EstiloPos.AlturaInput);
            comboUnidad.Size    = new System.Drawing.Size(110, EstiloPos.AlturaInput);

            // Botón gestionar categoría
            EstiloPos.AplicarBotonSecundario(btnGestionarCat);
            btnGestionarCat.Font = EstiloPos.FontSmall;

            // Guardar / Cancelar
            EstiloPos.AplicarBotonPrimario(btnGuardar);
            EstiloPos.AplicarBotonSecundario(btnCancelar);

            // Error
            lblError.Font      = EstiloPos.FontSmall;
            lblError.ForeColor = EstiloPos.Rojo;

            // Botones de acción de inventario
            AplicarBtnAccion(btnAgregar,    EstiloPos.Verde);
            AplicarBtnAccion(btnDescontar,  EstiloPos.Rojo);
            AplicarBtnAccion(btnDesactivar, EstiloPos.Ink2);
            AplicarBtnAccion(btnEliminar,   EstiloPos.Rojo);
            AplicarBtnAccion(btnLog,        EstiloPos.Ink2);

            // Log
            pnlLog.BackColor       = EstiloPos.Surface;
            pnlLogFiltros.BackColor = EstiloPos.Surface;
            dtpDesde.Font = EstiloPos.FontBody;
            dtpHasta.Font = EstiloPos.FontBody;
            EstiloPos.AplicarBotonPrimario(btnFiltrarLog);
            btnFiltrarLog.Size = new System.Drawing.Size(90, 30);

            // Grids
            EstiloPos.AplicarGrid(dgvLog);
            EstiloPos.AplicarGrid(dgvProductos);
        }

        private void AplicarBtnAccion(System.Windows.Forms.Button b, System.Drawing.Color fg)
        {
            b.BackColor = EstiloPos.Surface;
            b.FlatAppearance.BorderColor = EstiloPos.Border;
            b.FlatAppearance.BorderSize  = 1;
            b.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            b.ForeColor = fg;
            b.Cursor    = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
        }

        private void FormProductos_FormClosed(object sender, FormClosedEventArgs e)
        {
            NotificadorCambios.Cambio -= OnCambioDatos;
        }

        private void OnCambioDatos(string entidad)
        {
            if (entidad == Entidad.Producto) CargarProductos();
        }

        // ── Categorías ────────────────────────────────────────────

        private void CargarCategorias()
        {
            string actual = comboCategoria.Text;
            comboCategoria.Items.Clear();
            comboCategoria.Items.Add("");
            foreach (var c in categoriaService.ObtenerTodas())
                comboCategoria.Items.Add(c.Nombre);
            comboCategoria.Text = actual;
        }

        private void btnGestionarCat_Click(object sender, EventArgs e)
        {
            using (var f = new FormGestionCategorias(categoriaService))
            {
                f.ShowDialog(this);
                CargarCategorias();
            }
        }

        // ── Tabla de productos ────────────────────────────────────

        private void CargarProductos()
        {
            var lista = productoService.Buscar(txtBuscar.Text);
            dgvProductos.Rows.Clear();
            foreach (var p in lista)
            {
                bool bajo = productoService.EstaBajoStock(p);
                string estado = !p.Activo ? "Inactivo" : bajo ? "Bajo" : "OK";
                int fila = dgvProductos.Rows.Add(p.IdProducto, p.CodigoBarras, p.Nombre,
                    p.Categoria, "$" + p.Precio.ToString("N0"),
                    p.Stock.ToString("0.##") + " " + p.UnidadMedida, estado);

                var row = dgvProductos.Rows[fila];
                if (!p.Activo)
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(160, 160, 160);
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(140, 140, 140);
                }
                else if (bajo)
                {
                    row.Cells["colEstado"].Style.ForeColor = Color.FromArgb(180, 30, 30);
                    row.Cells["colEstado"].Style.SelectionForeColor = Color.FromArgb(180, 30, 30);
                }
                else
                {
                    row.Cells["colEstado"].Style.ForeColor = Color.FromArgb(30, 120, 30);
                    row.Cells["colEstado"].Style.SelectionForeColor = Color.FromArgb(30, 120, 30);
                }
            }
            ActualizarBotonesSegunSeleccion();
        }

        private void dgvProductos_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarBotonesSegunSeleccion();
        }

        private void ActualizarBotonesSegunSeleccion()
        {
            if (dgvProductos.CurrentRow == null) { btnDesactivar.Text = "Desactivar"; return; }
            bool inactivo = dgvProductos.CurrentRow.Cells["colEstado"].Value?.ToString() == "Inactivo";
            btnDesactivar.Text = inactivo ? "Activar" : "Desactivar";
            btnDesactivar.ForeColor = inactivo ? Color.FromArgb(30, 120, 30) : Color.FromArgb(50, 50, 55);
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e) => CargarProductos();

        // ── Crear / Actualizar ────────────────────────────────────

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            var p = new Producto
            {
                IdProducto   = idEnEdicion,
                CodigoBarras = txtCodigo.Text.Trim(),
                Nombre       = txtNombre.Text.Trim(),
                Categoria    = comboCategoria.Text.Trim(),
                UnidadMedida = comboUnidad.SelectedItem.ToString()
            };
            if (!decimal.TryParse(txtPrecio.Text, out decimal precio))  { MostrarError("Precio inválido.");  return; }
            if (!decimal.TryParse(txtStock.Text,  out decimal stock))   { MostrarError("Stock inválido.");   return; }
            decimal.TryParse(txtStockMin.Text, out decimal stockMin);
            p.Precio = precio; p.Stock = stock; p.StockMinimo = stockMin;
            try
            {
                if (idEnEdicion == 0) productoService.Crear(p);
                else productoService.Actualizar(p);
                LimpiarFormulario();
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Editar (doble clic) ───────────────────────────────────

        private void dgvProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgvProductos.Rows[e.RowIndex].Cells["colId"].Value);
            var prod = productoService.ObtenerPorId(id);
            if (prod == null) return;

            idEnEdicion = prod.IdProducto;
            txtCodigo.Text         = prod.CodigoBarras;
            txtNombre.Text         = prod.Nombre;
            comboCategoria.Text    = prod.Categoria;
            txtPrecio.Text         = prod.Precio.ToString("0.##");
            txtStock.Text          = prod.Stock.ToString("0.##");
            txtStockMin.Text       = prod.StockMinimo.ToString("0.##");
            comboUnidad.SelectedItem = prod.UnidadMedida;
            lblModo.Text           = "Editando: " + prod.Nombre;
            btnGuardar.Text        = "Actualizar";
            btnCancelar.Visible    = true;
        }

        private void btnCancelar_Click(object sender, EventArgs e) => LimpiarFormulario();

        // ── Inventario ────────────────────────────────────────────

        private void btnAgregar_Click(object sender, EventArgs e)  => AjustarStock(true);
        private void btnDescontar_Click(object sender, EventArgs e) => AjustarStock(false);

        private void AjustarStock(bool esAgregar)
        {
            if (dgvProductos.CurrentRow == null) { MostrarError("Selecciona un producto en la tabla."); return; }
            if (!decimal.TryParse(txtCantidad.Text, out decimal cant) || cant <= 0) { MostrarError("Cantidad inválida."); return; }
            int    id     = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();
            try
            {
                decimal nuevo = productoService.AjustarStock(id, esAgregar ? cant : -cant);
                txtCantidad.Clear();
                lblError.Visible = false;
                MessageBox.Show((esAgregar ? "Stock agregado" : "Stock descontado") + " para \"" + nombre + "\".\nStock actual: " + nuevo.ToString("0.##"),
                    "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ── Activar / Desactivar ──────────────────────────────────

        private void btnDesactivar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) return;
            int    id      = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre  = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();
            bool inactivo  = dgvProductos.CurrentRow.Cells["colEstado"].Value?.ToString() == "Inactivo";
            if (inactivo)
            {
                if (MessageBox.Show("¿Activar \"" + nombre + "\"?", "Activar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                { productoService.Activar(id); LimpiarFormulario(); }
            }
            else
            {
                if (MessageBox.Show("¿Desactivar \"" + nombre + "\"?\nSeguirá visible en gris y se puede reactivar.", "Desactivar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                { productoService.Desactivar(id); LimpiarFormulario(); }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) return;
            int    id     = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();
            if (MessageBox.Show("¿Eliminar PERMANENTEMENTE \"" + nombre + "\"?", "Eliminar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try { productoService.Eliminar(id); LimpiarFormulario(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "No se puede eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        // ── Log ───────────────────────────────────────────────────

        private void btnLog_Click(object sender, EventArgs e)
        {
            logVisible = !logVisible;
            splitterLog.Visible = logVisible;
            pnlLog.Visible = logVisible;
            btnLog.Text = logVisible ? "▲ Ocultar log" : "▼ Ver log de inventario";
            if (logVisible) CargarLog();
        }

        private void CargarLog()
        {
            var lista = logService.Obtener(dtpDesde.Value.Date, dtpHasta.Value.Date, modulo: ModuloLog.Productos);
            dgvLog.Rows.Clear();
            foreach (var l in lista)
                dgvLog.Rows.Add(l.Fecha.ToString("dd/MM/yyyy HH:mm"), l.NombreUsuario, l.Accion, l.Detalle);
        }

        private void btnFiltrarLog_Click(object sender, EventArgs e) => CargarLog();

        // ── Utilidades ────────────────────────────────────────────

        private void LimpiarFormulario()
        {
            idEnEdicion = 0;
            txtCodigo.Clear(); txtNombre.Clear(); comboCategoria.Text = "";
            txtPrecio.Clear(); txtStock.Clear(); txtStockMin.Clear();
            comboUnidad.SelectedIndex = 0;
            lblModo.Text = "Nuevo producto";
            btnGuardar.Text = "Guardar";
            btnCancelar.Visible = false;
            lblError.Visible = false;
            txtCodigo.Focus();
        }

        private void MostrarError(string msg) { lblError.Text = msg; lblError.Visible = true; }
    }
}

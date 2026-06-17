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
        private ContextMenuStrip menuDescuento;

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
            // Desuscribir al disponerse: como form hijo, Close() no dispara FormClosed (evita fuga + handler huérfano).
            this.Disposed += (s, ev) => NotificadorCambios.Cambio -= OnCambioDatos;
            AplicarPermisos();
            ConfigurarMenuDescuento();
            AcomodarFilaAcciones();
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
            b.FlatAppearance.MouseOverBackColor = EstiloPos.Hover;
            b.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            b.ForeColor = fg;
            b.Cursor    = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
        }

        // Distribuye la barra de acciones: el grupo "modificar existencias" (buscar,
        // cantidad, agregar/descontar) queda a la izquierda y las acciones de producto
        // (desactivar, eliminar, log) se anclan al borde derecho — simétrico y responsive.
        private void pnlAcciones_Resize(object sender, EventArgs e) => AcomodarFilaAcciones();

        private void AcomodarFilaAcciones()
        {
            int y = btnAgregar.Top;     // conserva la Y del diseñador
            int gap = 10, margenDer = 18;

            // Ajusta el ancho de cada botón a su texto para que no se trunque con fuentes grandes
            btnAgregar.Width    = TextRenderer.MeasureText(btnAgregar.Text,   btnAgregar.Font).Width + 30;
            btnDescontar.Width  = TextRenderer.MeasureText(btnDescontar.Text, btnDescontar.Font).Width + 30;
            btnEliminar.Width   = TextRenderer.MeasureText(btnEliminar.Text,  btnEliminar.Font).Width + 30;
            btnLog.Width        = TextRenderer.MeasureText(btnLog.Text,       btnLog.Font).Width + 30;
            btnDesactivar.Width = TextRenderer.MeasureText("Desactivar",      btnDesactivar.Font).Width + 30; // el texto más largo

            // Grupo izquierdo: modificar existencias, tras el campo de cantidad
            int xi = txtCantidad.Right + 18;
            btnAgregar.Location   = new Point(xi, y);
            btnDescontar.Location = new Point(btnAgregar.Right + gap, y);

            // Grupo derecho: acciones de producto, ancladas a la derecha (solo las visibles)
            int x = pnlAcciones.ClientSize.Width - margenDer;
            foreach (var b in new[] { btnLog, btnEliminar, btnDesactivar })
            {
                if (!b.Visible) continue;
                x -= b.Width;
                b.Location = new Point(x, y);
                x -= gap;
            }
        }

        // Permisos: el inventario es de solo lectura para empleados (rol no-admin).
        // Ven la tabla, la búsqueda y el log; no pueden crear, editar, ajustar stock ni eliminar.
        private void AplicarPermisos()
        {
            if (Sesion.EsAdmin) return;

            pnlFormulario.Visible = false;   // formulario de alta / edición
            lblCantidad.Visible   = false;
            txtCantidad.Visible   = false;
            btnAgregar.Visible    = false;   // agregar stock
            btnDescontar.Visible  = false;   // descontar stock
            btnDesactivar.Visible = false;
            btnEliminar.Visible   = false;
        }

        private void FormProductos_FormClosed(object sender, FormClosedEventArgs e)
        {
            NotificadorCambios.Cambio -= OnCambioDatos;
        }

        private void OnCambioDatos(string entidad)
        {
            if (IsDisposed || Disposing) return;
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
                    p.TieneDescuento ? "-" + p.DescuentoPorcentaje.ToString("0.##") + "%" : "—",
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

        // ── Descuento por producto (clic derecho sobre la fila) ───

        private void ConfigurarMenuDescuento()
        {
            if (!Sesion.EsAdmin) return;   // solo el administrador gestiona descuentos

            menuDescuento = new ContextMenuStrip { Font = EstiloPos.FontBody };
            var miAplicar = new ToolStripMenuItem("Aplicar / editar descuento…");
            var miQuitar  = new ToolStripMenuItem("Quitar descuento");
            miAplicar.Click += (s, e) => AplicarDescuentoSeleccionado();
            miQuitar.Click  += (s, e) => QuitarDescuentoSeleccionado();
            menuDescuento.Items.Add(miAplicar);
            menuDescuento.Items.Add(miQuitar);

            // Clic derecho: selecciona la fila bajo el cursor y abre el menú sobre ella
            dgvProductos.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Right) return;
                var hit = dgvProductos.HitTest(e.X, e.Y);
                if (hit.RowIndex < 0) return;
                dgvProductos.ClearSelection();
                dgvProductos.Rows[hit.RowIndex].Selected = true;
                dgvProductos.CurrentCell = dgvProductos.Rows[hit.RowIndex].Cells["colCodigo"];
                menuDescuento.Show(dgvProductos, e.Location);
            };
        }

        private void AplicarDescuentoSeleccionado()
        {
            if (dgvProductos.CurrentRow == null) return;
            int    id     = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();
            var prod = productoService.ObtenerPorId(id);
            if (prod == null) return;

            string actual = prod.TieneDescuento ? prod.DescuentoPorcentaje.ToString("0.##") : "";
            string input = Aviso.Prompt(this, "Descuento — " + nombre,
                "Porcentaje de descuento (0 a 100) para \"" + nombre + "\":", actual);
            if (input == null) return;   // canceló

            if (!decimal.TryParse(input, out decimal pct) || pct < 0 || pct > 100)
            { Aviso.Error(this, "Ingresa un número entre 0 y 100.", "Descuento inválido"); return; }

            try
            {
                productoService.AplicarDescuento(id, pct);
                if (pct > 0)
                {
                    decimal conDesc = System.Math.Round(prod.Precio * (1 - pct / 100m), 0, System.MidpointRounding.AwayFromZero);
                    Aviso.Exito(this, "Se aplicó un " + pct.ToString("0.##") + "% a \"" + nombre + "\".\n" +
                        "Precio: $" + prod.Precio.ToString("N0") + "  →  $" + conDesc.ToString("N0"),
                        "Descuento actualizado");
                }
                else
                {
                    Aviso.Exito(this, "Se quitó el descuento de \"" + nombre + "\".", "Descuento actualizado");
                }
            }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo aplicar"); }
        }

        private void QuitarDescuentoSeleccionado()
        {
            if (dgvProductos.CurrentRow == null) return;
            int    id     = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();
            var prod = productoService.ObtenerPorId(id);
            if (prod == null) return;
            if (!prod.TieneDescuento)
            { Aviso.Info(this, "\"" + nombre + "\" no tiene descuento.", "Sin descuento"); return; }

            if (!Aviso.Confirmar(this, "Se quitará el " + prod.DescuentoPorcentaje.ToString("0.##") +
                    "% de descuento de \"" + nombre + "\".", "¿Quitar descuento?", "Quitar"))
                return;
            try
            {
                productoService.AplicarDescuento(id, 0);
                Aviso.Exito(this, "Descuento quitado de \"" + nombre + "\".", "Listo");
            }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo"); }
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

            // Confirmar antes de actualizar un producto existente (no para el alta)
            if (idEnEdicion != 0 &&
                !Aviso.Confirmar(this, "Se guardarán los cambios del producto \"" + p.Nombre + "\".",
                    "¿Actualizar producto?", "Actualizar", TipoAviso.Info))
                return;
            try
            {
                if (idEnEdicion == 0)
                {
                    productoService.Crear(p);
                    LimpiarFormulario();
                }
                else
                {
                    productoService.Actualizar(p);
                    LimpiarFormulario();
                    Aviso.Exito(this, "Los cambios de \"" + p.Nombre + "\" se guardaron correctamente.",
                        "Producto actualizado");
                }
            }
            catch (Exception ex) { MostrarError(Errores.Usuario(ex)); }
        }

        // ── Editar (doble clic) ───────────────────────────────────

        private void dgvProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!Sesion.EsAdmin) return;   // los empleados no editan productos
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
                Aviso.Exito(this,
                    (esAgregar ? "Se agregó stock a" : "Se descontó stock de") + " \"" + nombre + "\".\nStock actual: " + nuevo.ToString("0.##"),
                    "Inventario actualizado");
            }
            catch (Exception ex) { MostrarError(Errores.Usuario(ex)); }
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
                if (Aviso.Confirmar(this, "El producto volverá a estar disponible para la venta.",
                        "¿Activar \"" + nombre + "\"?", "Activar", TipoAviso.Info))
                { productoService.Activar(id); LimpiarFormulario(); }
            }
            else
            {
                if (Aviso.Confirmar(this, "Seguirá visible en gris y podrás reactivarlo cuando quieras.",
                        "¿Desactivar \"" + nombre + "\"?", "Desactivar"))
                { productoService.Desactivar(id); LimpiarFormulario(); }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) return;
            int    id     = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();
            if (!Aviso.Confirmar(this, "Esta acción no se puede deshacer.",
                    "¿Eliminar \"" + nombre + "\"?", "Eliminar", TipoAviso.Error)) return;
            try { productoService.Eliminar(id); LimpiarFormulario(); }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se puede eliminar"); }
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
            // (el descuento se gestiona aparte, con clic derecho sobre el producto)
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

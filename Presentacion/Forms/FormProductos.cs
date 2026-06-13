using System;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Eventos;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormProductos : Form
    {
        private readonly ProductoService productoService = new ProductoService();
        private int idEnEdicion = 0;

        public FormProductos()
        {
            InitializeComponent();
        }

        private void FormProductos_Load(object sender, EventArgs e)
        {
            comboUnidad.Items.AddRange(new object[] { UnidadMedida.Unidad, UnidadMedida.Kilogramo });
            comboUnidad.SelectedIndex = 0;

            NotificadorCambios.Cambio += OnCambioDatos;
            CargarProductos();
        }

        private void FormProductos_FormClosed(object sender, FormClosedEventArgs e)
        {
            NotificadorCambios.Cambio -= OnCambioDatos;
        }

        private void OnCambioDatos(string entidad)
        {
            if (entidad == Entidad.Producto)
                CargarProductos();
        }

        private void CargarProductos()
        {
            var lista = productoService.Buscar(txtBuscar.Text);
            dgvProductos.Rows.Clear();

            foreach (var p in lista)
            {
                bool bajo = productoService.EstaBajoStock(p);
                int fila = dgvProductos.Rows.Add(
                    p.IdProducto,
                    p.CodigoBarras,
                    p.Nombre,
                    p.Categoria,
                    "$" + p.Precio.ToString("N0"),
                    p.Stock.ToString("0.##") + " " + p.UnidadMedida,
                    bajo ? "Bajo" : "OK"
                );

                var celdaEstado = dgvProductos.Rows[fila].Cells["colEstado"];
                if (bajo)
                {
                    celdaEstado.Style.ForeColor = Color.FromArgb(163, 45, 45);
                    celdaEstado.Style.SelectionForeColor = Color.FromArgb(163, 45, 45);
                }
                else
                {
                    celdaEstado.Style.ForeColor = Color.FromArgb(59, 109, 17);
                    celdaEstado.Style.SelectionForeColor = Color.FromArgb(59, 109, 17);
                }
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            CargarProductos();
        }

        // ---------- Crear / Actualizar ----------

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            var p = new Producto
            {
                IdProducto = idEnEdicion,
                CodigoBarras = txtCodigo.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Categoria = txtCategoria.Text.Trim(),
                UnidadMedida = comboUnidad.SelectedItem.ToString()
            };

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio))
            {
                MostrarError("Precio inválido.");
                return;
            }
            p.Precio = precio;

            if (!decimal.TryParse(txtStock.Text, out decimal stock))
            {
                MostrarError("Stock inválido.");
                return;
            }
            p.Stock = stock;

            if (!decimal.TryParse(txtStockMin.Text, out decimal stockMin))
                stockMin = 0;
            p.StockMinimo = stockMin;

            try
            {
                if (idEnEdicion == 0)
                    productoService.Crear(p);
                else
                    productoService.Actualizar(p);

                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        // ---------- Editar ----------

        private void btnEditar_Click(object sender, EventArgs e)
        {
            CargarEnFormulario();
        }

        private void dgvProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            CargarEnFormulario();
        }

        private void CargarEnFormulario()
        {
            if (dgvProductos.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            var prod = productoService.ObtenerPorId(id);
            if (prod == null) return;

            idEnEdicion = prod.IdProducto;
            txtCodigo.Text = prod.CodigoBarras;
            txtNombre.Text = prod.Nombre;
            txtCategoria.Text = prod.Categoria;
            txtPrecio.Text = prod.Precio.ToString("0.##");
            txtStock.Text = prod.Stock.ToString("0.##");
            txtStockMin.Text = prod.StockMinimo.ToString("0.##");
            comboUnidad.SelectedItem = prod.UnidadMedida;

            lblModo.Text = "Editando: " + prod.Nombre;
            btnGuardar.Text = "Actualizar";
            btnCancelar.Visible = true;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        // ---------- Control de inventario (entrada / salida) ----------

        private void btnEntrada_Click(object sender, EventArgs e)
        {
            AjustarStockSeleccionado(true);
        }

        private void btnSalida_Click(object sender, EventArgs e)
        {
            AjustarStockSeleccionado(false);
        }

        private void AjustarStockSeleccionado(bool esEntrada)
        {
            if (dgvProductos.CurrentRow == null)
            {
                MostrarError("Selecciona un producto en la tabla.");
                return;
            }

            if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad) || cantidad <= 0)
            {
                MostrarError("Ingresa una cantidad válida en el campo Cantidad.");
                return;
            }

            int id = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();
            decimal delta = esEntrada ? cantidad : -cantidad;

            try
            {
                decimal nuevoStock = productoService.AjustarStock(id, delta);
                txtCantidad.Clear();
                lblError.Visible = false;
                string accion = esEntrada ? "Entrada" : "Salida";
                MessageBox.Show(
                    accion + " registrada para \"" + nombre + "\".\nStock actual: " + nuevoStock.ToString("0.##"),
                    "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        // ---------- Desactivar / Eliminar ----------

        private void btnDesactivar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();

            var r = MessageBox.Show(
                "¿Desactivar el producto \"" + nombre + "\"?\nNo se borra, pero deja de aparecer en ventas. Se conserva el historial.",
                "Desactivar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (r == DialogResult.Yes)
            {
                productoService.Desactivar(id);
                LimpiarFormulario();
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvProductos.CurrentRow.Cells["colId"].Value);
            string nombre = dgvProductos.CurrentRow.Cells["colNombre"].Value?.ToString();

            var r = MessageBox.Show(
                "¿Eliminar PERMANENTEMENTE el producto \"" + nombre + "\"?\nEsta acción no se puede deshacer.",
                "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (r != DialogResult.Yes) return;

            try
            {
                productoService.Eliminar(id);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No se puede eliminar",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ---------- Utilidades ----------

        private void LimpiarFormulario()
        {
            idEnEdicion = 0;
            txtCodigo.Clear();
            txtNombre.Clear();
            txtCategoria.Clear();
            txtPrecio.Clear();
            txtStock.Clear();
            txtStockMin.Clear();
            comboUnidad.SelectedIndex = 0;
            lblModo.Text = "Nuevo producto";
            btnGuardar.Text = "Guardar";
            btnCancelar.Visible = false;
            lblError.Visible = false;
            txtCodigo.Focus();
        }

        private void MostrarError(string msg)
        {
            lblError.Text = msg;
            lblError.Visible = true;
        }
    }
}

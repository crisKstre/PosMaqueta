using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormVentas : Form
    {
        private readonly VentaService ventaService = new VentaService();
        private List<Producto> sugerencias = new List<Producto>();

        public FormVentas()
        {
            InitializeComponent();
        }

        private void FormVentas_Load(object sender, EventArgs e)
        {
            comboMedioPago.Items.AddRange(new object[] {
                MedioPago.Efectivo, MedioPago.Tarjeta, MedioPago.Transferencia });
            comboMedioPago.SelectedIndex = 0;

            txtCantidad.Text = "1";
            lstSugerencias.Visible = false;
            RefrescarCarrito();
            txtCodigo.Focus();
        }

        // ---------- Escaneo / código de barras ----------

        // El lector "escribe" el código + Enter; capturamos el Enter para agregar.
        private void txtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AgregarPorCodigo();
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            AgregarPorCodigo();
        }

        private void AgregarPorCodigo()
        {
            string codigo = txtCodigo.Text.Trim();
            if (string.IsNullOrEmpty(codigo))
                return;

            try
            {
                ventaService.AgregarPorCodigo(codigo, LeerCantidad());
                RefrescarCarrito();
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message);
            }

            txtCodigo.Clear();
            txtCantidad.Text = "1";
            txtCodigo.Focus();
        }

        // ---------- Búsqueda por nombre con sugerencias ----------

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            string texto = txtBuscar.Text.Trim();
            if (texto.Length == 0)
            {
                lstSugerencias.Visible = false;
                return;
            }

            sugerencias = ventaService.BuscarProductos(texto);
            lstSugerencias.Items.Clear();

            foreach (var p in sugerencias)
            {
                lstSugerencias.Items.Add(p.Nombre + "   ·   $" + p.Precio.ToString("N0") +
                    "   ·   stock: " + p.Stock.ToString("0.##") + " " + p.UnidadMedida);
            }

            lstSugerencias.Visible = sugerencias.Count > 0;
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            // Flecha abajo pasa el foco a la lista de sugerencias
            if (e.KeyCode == Keys.Down && lstSugerencias.Visible && lstSugerencias.Items.Count > 0)
            {
                lstSugerencias.Focus();
                lstSugerencias.SelectedIndex = 0;
                e.SuppressKeyPress = true;
            }
        }

        private void lstSugerencias_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AgregarSugerenciaSeleccionada();
                e.SuppressKeyPress = true;
            }
        }

        private void lstSugerencias_DoubleClick(object sender, EventArgs e)
        {
            AgregarSugerenciaSeleccionada();
        }

        private void AgregarSugerenciaSeleccionada()
        {
            int i = lstSugerencias.SelectedIndex;
            if (i < 0 || i >= sugerencias.Count) return;

            try
            {
                ventaService.AgregarPorId(sugerencias[i].IdProducto, LeerCantidad());
                RefrescarCarrito();
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message);
            }

            txtBuscar.Clear();
            lstSugerencias.Visible = false;
            txtCantidad.Text = "1";
            txtCodigo.Focus();
        }

        private decimal LeerCantidad()
        {
            if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad) || cantidad <= 0)
                cantidad = 1;
            return cantidad;
        }

        // ---------- Carrito ----------

        private void RefrescarCarrito()
        {
            dgvCarrito.Rows.Clear();
            foreach (var d in ventaService.Carrito)
            {
                dgvCarrito.Rows.Add(
                    d.IdProducto,
                    d.NombreProducto,
                    "$" + d.PrecioUnitario.ToString("N0"),
                    d.Cantidad.ToString("0.##"),
                    "$" + d.Subtotal.ToString("N0"),
                    "Quitar"
                );
            }
            lblTotal.Text = "$" + ventaService.Total.ToString("N0");
            lblMensaje.Visible = false;
        }

        private void dgvCarrito_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCarrito.Columns[e.ColumnIndex].Name == "colQuitar")
            {
                int idProducto = Convert.ToInt32(dgvCarrito.Rows[e.RowIndex].Cells["colVId"].Value);
                ventaService.QuitarDelCarrito(idProducto);
                RefrescarCarrito();
            }
        }

        // ---------- Cobro ----------

        private void btnCobrar_Click(object sender, EventArgs e)
        {
            if (ventaService.Carrito.Count == 0)
            {
                MostrarMensaje("El carrito está vacío.");
                return;
            }

            string medioPago = comboMedioPago.SelectedItem.ToString();
            decimal total = ventaService.Total;

            try
            {
                int idVenta = ventaService.CobrarVenta(Sesion.UsuarioActual.IdUsuario, medioPago);
                MessageBox.Show(
                    "Venta registrada (N° " + idVenta + ").\nTotal: $" + total.ToString("N0") + "\nPago: " + medioPago,
                    "Venta exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefrescarCarrito();
                txtCodigo.Focus();
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (ventaService.Carrito.Count == 0) return;

            var r = MessageBox.Show("¿Cancelar la venta actual? Se vaciará el carrito.",
                "Cancelar venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                ventaService.VaciarCarrito();
                RefrescarCarrito();
                txtCodigo.Focus();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F12)
            {
                btnCobrar_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MostrarMensaje(string msg)
        {
            lblMensaje.Text = msg;
            lblMensaje.Visible = true;
        }
    }
}

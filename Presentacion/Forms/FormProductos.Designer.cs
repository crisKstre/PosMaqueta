using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormProductos
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlFormulario;
        private Label lblModo;
        private Label lblCodigo;
        private TextBox txtCodigo;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblCategoria;
        private TextBox txtCategoria;
        private Label lblPrecio;
        private TextBox txtPrecio;
        private Label lblStock;
        private TextBox txtStock;
        private Label lblStockMin;
        private TextBox txtStockMin;
        private Label lblUnidad;
        private ComboBox comboUnidad;
        private Button btnGuardar;
        private Button btnCancelar;
        private Label lblError;
        private Panel pnlBusqueda;
        private TextBox txtBuscar;
        private Label lblBuscar;
        private Label lblCantidad;
        private TextBox txtCantidad;
        private Button btnEntrada;
        private Button btnSalida;
        private Button btnEditar;
        private Button btnDesactivar;
        private Button btnEliminar;
        private DataGridView dgvProductos;
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colCodigo;
        private DataGridViewTextBoxColumn colNombre;
        private DataGridViewTextBoxColumn colCategoria;
        private DataGridViewTextBoxColumn colPrecio;
        private DataGridViewTextBoxColumn colStock;
        private DataGridViewTextBoxColumn colEstado;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlFormulario = new Panel();
            this.lblModo = new Label();
            this.lblCodigo = new Label();
            this.txtCodigo = new TextBox();
            this.lblNombre = new Label();
            this.txtNombre = new TextBox();
            this.lblCategoria = new Label();
            this.txtCategoria = new TextBox();
            this.lblPrecio = new Label();
            this.txtPrecio = new TextBox();
            this.lblStock = new Label();
            this.txtStock = new TextBox();
            this.lblStockMin = new Label();
            this.txtStockMin = new TextBox();
            this.lblUnidad = new Label();
            this.comboUnidad = new ComboBox();
            this.btnGuardar = new Button();
            this.btnCancelar = new Button();
            this.lblError = new Label();
            this.pnlBusqueda = new Panel();
            this.txtBuscar = new TextBox();
            this.lblBuscar = new Label();
            this.lblCantidad = new Label();
            this.txtCantidad = new TextBox();
            this.btnEntrada = new Button();
            this.btnSalida = new Button();
            this.btnEditar = new Button();
            this.btnDesactivar = new Button();
            this.btnEliminar = new Button();
            this.dgvProductos = new DataGridView();
            this.colId = new DataGridViewTextBoxColumn();
            this.colCodigo = new DataGridViewTextBoxColumn();
            this.colNombre = new DataGridViewTextBoxColumn();
            this.colCategoria = new DataGridViewTextBoxColumn();
            this.colPrecio = new DataGridViewTextBoxColumn();
            this.colStock = new DataGridViewTextBoxColumn();
            this.colEstado = new DataGridViewTextBoxColumn();
            this.pnlFormulario.SuspendLayout();
            this.pnlBusqueda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).BeginInit();
            this.SuspendLayout();
            //
            // pnlFormulario
            //
            this.pnlFormulario.BackColor = Color.White;
            this.pnlFormulario.BorderStyle = BorderStyle.FixedSingle;
            this.pnlFormulario.Controls.Add(this.lblModo);
            this.pnlFormulario.Controls.Add(this.lblCodigo);
            this.pnlFormulario.Controls.Add(this.txtCodigo);
            this.pnlFormulario.Controls.Add(this.lblNombre);
            this.pnlFormulario.Controls.Add(this.txtNombre);
            this.pnlFormulario.Controls.Add(this.lblCategoria);
            this.pnlFormulario.Controls.Add(this.txtCategoria);
            this.pnlFormulario.Controls.Add(this.lblPrecio);
            this.pnlFormulario.Controls.Add(this.txtPrecio);
            this.pnlFormulario.Controls.Add(this.lblStock);
            this.pnlFormulario.Controls.Add(this.txtStock);
            this.pnlFormulario.Controls.Add(this.lblStockMin);
            this.pnlFormulario.Controls.Add(this.txtStockMin);
            this.pnlFormulario.Controls.Add(this.lblUnidad);
            this.pnlFormulario.Controls.Add(this.comboUnidad);
            this.pnlFormulario.Controls.Add(this.btnGuardar);
            this.pnlFormulario.Controls.Add(this.btnCancelar);
            this.pnlFormulario.Controls.Add(this.lblError);
            this.pnlFormulario.Dock = DockStyle.Top;
            this.pnlFormulario.Height = 170;
            this.pnlFormulario.Padding = new Padding(16);
            //
            // lblModo
            //
            this.lblModo.AutoSize = true;
            this.lblModo.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            this.lblModo.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblModo.Location = new Point(16, 12);
            this.lblModo.Text = "Nuevo producto";
            //
            // lblCodigo
            //
            this.lblCodigo.AutoSize = true;
            this.lblCodigo.Font = new Font("Segoe UI", 9F);
            this.lblCodigo.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblCodigo.Location = new Point(16, 44);
            this.lblCodigo.Text = "Código de barras";
            //
            // txtCodigo
            //
            this.txtCodigo.BorderStyle = BorderStyle.FixedSingle;
            this.txtCodigo.Font = new Font("Segoe UI", 10F);
            this.txtCodigo.Location = new Point(16, 64);
            this.txtCodigo.Size = new Size(180, 27);
            //
            // lblNombre
            //
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new Font("Segoe UI", 9F);
            this.lblNombre.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblNombre.Location = new Point(210, 44);
            this.lblNombre.Text = "Nombre";
            //
            // txtNombre
            //
            this.txtNombre.BorderStyle = BorderStyle.FixedSingle;
            this.txtNombre.Font = new Font("Segoe UI", 10F);
            this.txtNombre.Location = new Point(210, 64);
            this.txtNombre.Size = new Size(220, 27);
            //
            // lblCategoria
            //
            this.lblCategoria.AutoSize = true;
            this.lblCategoria.Font = new Font("Segoe UI", 9F);
            this.lblCategoria.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblCategoria.Location = new Point(446, 44);
            this.lblCategoria.Text = "Categoría";
            //
            // txtCategoria
            //
            this.txtCategoria.BorderStyle = BorderStyle.FixedSingle;
            this.txtCategoria.Font = new Font("Segoe UI", 10F);
            this.txtCategoria.Location = new Point(446, 64);
            this.txtCategoria.Size = new Size(160, 27);
            //
            // lblPrecio
            //
            this.lblPrecio.AutoSize = true;
            this.lblPrecio.Font = new Font("Segoe UI", 9F);
            this.lblPrecio.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblPrecio.Location = new Point(16, 100);
            this.lblPrecio.Text = "Precio";
            //
            // txtPrecio
            //
            this.txtPrecio.BorderStyle = BorderStyle.FixedSingle;
            this.txtPrecio.Font = new Font("Segoe UI", 10F);
            this.txtPrecio.Location = new Point(16, 120);
            this.txtPrecio.Size = new Size(110, 27);
            //
            // lblStock
            //
            this.lblStock.AutoSize = true;
            this.lblStock.Font = new Font("Segoe UI", 9F);
            this.lblStock.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblStock.Location = new Point(138, 100);
            this.lblStock.Text = "Stock";
            //
            // txtStock
            //
            this.txtStock.BorderStyle = BorderStyle.FixedSingle;
            this.txtStock.Font = new Font("Segoe UI", 10F);
            this.txtStock.Location = new Point(138, 120);
            this.txtStock.Size = new Size(90, 27);
            //
            // lblStockMin
            //
            this.lblStockMin.AutoSize = true;
            this.lblStockMin.Font = new Font("Segoe UI", 9F);
            this.lblStockMin.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblStockMin.Location = new Point(240, 100);
            this.lblStockMin.Text = "Stock mínimo";
            //
            // txtStockMin
            //
            this.txtStockMin.BorderStyle = BorderStyle.FixedSingle;
            this.txtStockMin.Font = new Font("Segoe UI", 10F);
            this.txtStockMin.Location = new Point(240, 120);
            this.txtStockMin.Size = new Size(90, 27);
            //
            // lblUnidad
            //
            this.lblUnidad.AutoSize = true;
            this.lblUnidad.Font = new Font("Segoe UI", 9F);
            this.lblUnidad.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblUnidad.Location = new Point(342, 100);
            this.lblUnidad.Text = "Unidad";
            //
            // comboUnidad
            //
            this.comboUnidad.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboUnidad.FlatStyle = FlatStyle.Flat;
            this.comboUnidad.Font = new Font("Segoe UI", 10F);
            this.comboUnidad.Location = new Point(342, 120);
            this.comboUnidad.Size = new Size(88, 27);
            //
            // btnGuardar
            //
            this.btnGuardar.BackColor = Color.FromArgb(28, 28, 30);
            this.btnGuardar.FlatStyle = FlatStyle.Flat;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnGuardar.ForeColor = Color.White;
            this.btnGuardar.Cursor = Cursors.Hand;
            this.btnGuardar.Location = new Point(446, 118);
            this.btnGuardar.Size = new Size(110, 32);
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += this.btnGuardar_Click;
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = Color.White;
            this.btnCancelar.FlatStyle = FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 212);
            this.btnCancelar.Font = new Font("Segoe UI", 10F);
            this.btnCancelar.ForeColor = Color.FromArgb(95, 94, 90);
            this.btnCancelar.Cursor = Cursors.Hand;
            this.btnCancelar.Location = new Point(562, 118);
            this.btnCancelar.Size = new Size(90, 32);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Visible = false;
            this.btnCancelar.Click += this.btnCancelar_Click;
            //
            // lblError
            //
            this.lblError.AutoSize = true;
            this.lblError.ForeColor = Color.FromArgb(163, 45, 45);
            this.lblError.Font = new Font("Segoe UI", 9F);
            this.lblError.Location = new Point(672, 126);
            this.lblError.MaximumSize = new Size(260, 0);
            this.lblError.Visible = false;
            //
            // pnlBusqueda
            //
            this.pnlBusqueda.BackColor = Color.FromArgb(250, 250, 249);
            this.pnlBusqueda.Controls.Add(this.lblBuscar);
            this.pnlBusqueda.Controls.Add(this.txtBuscar);
            this.pnlBusqueda.Controls.Add(this.lblCantidad);
            this.pnlBusqueda.Controls.Add(this.txtCantidad);
            this.pnlBusqueda.Controls.Add(this.btnEntrada);
            this.pnlBusqueda.Controls.Add(this.btnSalida);
            this.pnlBusqueda.Controls.Add(this.btnEditar);
            this.pnlBusqueda.Controls.Add(this.btnDesactivar);
            this.pnlBusqueda.Controls.Add(this.btnEliminar);
            this.pnlBusqueda.Dock = DockStyle.Top;
            this.pnlBusqueda.Height = 60;
            this.pnlBusqueda.Padding = new Padding(16, 12, 16, 8);
            //
            // lblBuscar
            //
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Font = new Font("Segoe UI", 9.5F);
            this.lblBuscar.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblBuscar.Location = new Point(16, 20);
            this.lblBuscar.Text = "Buscar:";
            //
            // txtBuscar
            //
            this.txtBuscar.BorderStyle = BorderStyle.FixedSingle;
            this.txtBuscar.Font = new Font("Segoe UI", 10F);
            this.txtBuscar.Location = new Point(70, 16);
            this.txtBuscar.Size = new Size(190, 27);
            this.txtBuscar.TextChanged += this.txtBuscar_TextChanged;
            //
            // lblCantidad
            //
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Font = new Font("Segoe UI", 9.5F);
            this.lblCantidad.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblCantidad.Location = new Point(285, 20);
            this.lblCantidad.Text = "Cantidad:";
            //
            // txtCantidad
            //
            this.txtCantidad.BorderStyle = BorderStyle.FixedSingle;
            this.txtCantidad.Font = new Font("Segoe UI", 10F);
            this.txtCantidad.Location = new Point(353, 16);
            this.txtCantidad.Size = new Size(70, 27);
            //
            // btnEntrada
            //
            this.btnEntrada.BackColor = Color.White;
            this.btnEntrada.FlatStyle = FlatStyle.Flat;
            this.btnEntrada.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 212);
            this.btnEntrada.Font = new Font("Segoe UI", 9.5F);
            this.btnEntrada.ForeColor = Color.FromArgb(59, 109, 17);
            this.btnEntrada.Cursor = Cursors.Hand;
            this.btnEntrada.Location = new Point(431, 15);
            this.btnEntrada.Size = new Size(95, 30);
            this.btnEntrada.Text = "Entrada +";
            this.btnEntrada.UseVisualStyleBackColor = false;
            this.btnEntrada.Click += this.btnEntrada_Click;
            //
            // btnSalida
            //
            this.btnSalida.BackColor = Color.White;
            this.btnSalida.FlatStyle = FlatStyle.Flat;
            this.btnSalida.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 212);
            this.btnSalida.Font = new Font("Segoe UI", 9.5F);
            this.btnSalida.ForeColor = Color.FromArgb(163, 45, 45);
            this.btnSalida.Cursor = Cursors.Hand;
            this.btnSalida.Location = new Point(531, 15);
            this.btnSalida.Size = new Size(95, 30);
            this.btnSalida.Text = "Salida −";
            this.btnSalida.UseVisualStyleBackColor = false;
            this.btnSalida.Click += this.btnSalida_Click;
            //
            // btnEditar
            //
            this.btnEditar.BackColor = Color.White;
            this.btnEditar.FlatStyle = FlatStyle.Flat;
            this.btnEditar.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 212);
            this.btnEditar.Font = new Font("Segoe UI", 9.5F);
            this.btnEditar.ForeColor = Color.FromArgb(95, 94, 90);
            this.btnEditar.Cursor = Cursors.Hand;
            this.btnEditar.Location = new Point(645, 15);
            this.btnEditar.Size = new Size(80, 30);
            this.btnEditar.Text = "Editar";
            this.btnEditar.UseVisualStyleBackColor = false;
            this.btnEditar.Click += this.btnEditar_Click;
            //
            // btnDesactivar
            //
            this.btnDesactivar.BackColor = Color.White;
            this.btnDesactivar.FlatStyle = FlatStyle.Flat;
            this.btnDesactivar.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 212);
            this.btnDesactivar.Font = new Font("Segoe UI", 9.5F);
            this.btnDesactivar.ForeColor = Color.FromArgb(95, 94, 90);
            this.btnDesactivar.Cursor = Cursors.Hand;
            this.btnDesactivar.Location = new Point(730, 15);
            this.btnDesactivar.Size = new Size(100, 30);
            this.btnDesactivar.Text = "Desactivar";
            this.btnDesactivar.UseVisualStyleBackColor = false;
            this.btnDesactivar.Click += this.btnDesactivar_Click;
            //
            // btnEliminar
            //
            this.btnEliminar.BackColor = Color.White;
            this.btnEliminar.FlatStyle = FlatStyle.Flat;
            this.btnEliminar.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 212);
            this.btnEliminar.Font = new Font("Segoe UI", 9.5F);
            this.btnEliminar.ForeColor = Color.FromArgb(163, 45, 45);
            this.btnEliminar.Cursor = Cursors.Hand;
            this.btnEliminar.Location = new Point(835, 15);
            this.btnEliminar.Size = new Size(90, 30);
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += this.btnEliminar_Click;
            //
            // dgvProductos
            //
            this.dgvProductos.BackgroundColor = Color.White;
            this.dgvProductos.BorderStyle = BorderStyle.None;
            this.dgvProductos.Dock = DockStyle.Fill;
            this.dgvProductos.AllowUserToAddRows = false;
            this.dgvProductos.AllowUserToDeleteRows = false;
            this.dgvProductos.ReadOnly = true;
            this.dgvProductos.RowHeadersVisible = false;
            this.dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvProductos.MultiSelect = false;
            this.dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvProductos.ColumnHeadersHeight = 38;
            this.dgvProductos.RowTemplate.Height = 32;
            this.dgvProductos.Font = new Font("Segoe UI", 9.5F);
            this.dgvProductos.GridColor = Color.FromArgb(236, 236, 234);
            this.dgvProductos.EnableHeadersVisualStyles = false;
            this.dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(242, 242, 240);
            this.dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(95, 94, 90);
            this.dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            this.dgvProductos.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            this.dgvProductos.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            this.dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 238, 236);
            this.dgvProductos.DefaultCellStyle.SelectionForeColor = Color.FromArgb(28, 28, 30);
            this.dgvProductos.Columns.AddRange(new DataGridViewColumn[] {
                this.colId, this.colCodigo, this.colNombre, this.colCategoria,
                this.colPrecio, this.colStock, this.colEstado });
            this.dgvProductos.CellDoubleClick += this.dgvProductos_CellDoubleClick;
            //
            // Columnas del grid
            //
            this.colId.Name = "colId";
            this.colId.HeaderText = "Id";
            this.colId.Visible = false;
            this.colCodigo.Name = "colCodigo";
            this.colCodigo.HeaderText = "Código";
            this.colNombre.Name = "colNombre";
            this.colNombre.HeaderText = "Nombre";
            this.colNombre.FillWeight = 160;
            this.colCategoria.Name = "colCategoria";
            this.colCategoria.HeaderText = "Categoría";
            this.colPrecio.Name = "colPrecio";
            this.colPrecio.HeaderText = "Precio";
            this.colStock.Name = "colStock";
            this.colStock.HeaderText = "Stock";
            this.colEstado.Name = "colEstado";
            this.colEstado.HeaderText = "Estado";
            this.colEstado.FillWeight = 60;
            //
            // FormProductos
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.FromArgb(250, 250, 249);
            this.Controls.Add(this.dgvProductos);
            this.Controls.Add(this.pnlBusqueda);
            this.Controls.Add(this.pnlFormulario);
            this.Name = "FormProductos";
            this.Text = "Productos";
            this.Load += this.FormProductos_Load;
            this.FormClosed += this.FormProductos_FormClosed;
            this.pnlFormulario.ResumeLayout(false);
            this.pnlFormulario.PerformLayout();
            this.pnlBusqueda.ResumeLayout(false);
            this.pnlBusqueda.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

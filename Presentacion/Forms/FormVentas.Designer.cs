using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormVentas
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlEscaneo;
        private Label lblCodigo;
        private TextBox txtCodigo;
        private Label lblBuscarNombre;
        private TextBox txtBuscar;
        private ListBox lstSugerencias;
        private Label lblCantidad;
        private TextBox txtCantidad;
        private Button btnAgregar;
        private Label lblMensaje;
        private DataGridView dgvCarrito;
        private DataGridViewTextBoxColumn colVId;
        private DataGridViewTextBoxColumn colVNombre;
        private DataGridViewTextBoxColumn colVPrecio;
        private DataGridViewTextBoxColumn colVCantidad;
        private DataGridViewTextBoxColumn colVSubtotal;
        private DataGridViewButtonColumn colQuitar;
        private Panel pnlCobro;
        private Label lblTotalTitulo;
        private Label lblTotal;
        private Label lblMedioPago;
        private ComboBox comboMedioPago;
        private Button btnCobrar;
        private Button btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlEscaneo = new Panel();
            this.lblCodigo = new Label();
            this.txtCodigo = new TextBox();
            this.lblBuscarNombre = new Label();
            this.txtBuscar = new TextBox();
            this.lstSugerencias = new ListBox();
            this.lblCantidad = new Label();
            this.txtCantidad = new TextBox();
            this.btnAgregar = new Button();
            this.lblMensaje = new Label();
            this.dgvCarrito = new DataGridView();
            this.colVId = new DataGridViewTextBoxColumn();
            this.colVNombre = new DataGridViewTextBoxColumn();
            this.colVPrecio = new DataGridViewTextBoxColumn();
            this.colVCantidad = new DataGridViewTextBoxColumn();
            this.colVSubtotal = new DataGridViewTextBoxColumn();
            this.colQuitar = new DataGridViewButtonColumn();
            this.pnlCobro = new Panel();
            this.lblTotalTitulo = new Label();
            this.lblTotal = new Label();
            this.lblMedioPago = new Label();
            this.comboMedioPago = new ComboBox();
            this.btnCobrar = new Button();
            this.btnCancelar = new Button();
            this.pnlEscaneo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarrito)).BeginInit();
            this.pnlCobro.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlEscaneo
            //
            this.pnlEscaneo.BackColor = Color.White;
            this.pnlEscaneo.BorderStyle = BorderStyle.FixedSingle;
            this.pnlEscaneo.Controls.Add(this.lblCodigo);
            this.pnlEscaneo.Controls.Add(this.txtCodigo);
            this.pnlEscaneo.Controls.Add(this.lblBuscarNombre);
            this.pnlEscaneo.Controls.Add(this.txtBuscar);
            this.pnlEscaneo.Controls.Add(this.lblCantidad);
            this.pnlEscaneo.Controls.Add(this.txtCantidad);
            this.pnlEscaneo.Controls.Add(this.btnAgregar);
            this.pnlEscaneo.Controls.Add(this.lblMensaje);
            this.pnlEscaneo.Dock = DockStyle.Top;
            this.pnlEscaneo.Height = 100;
            this.pnlEscaneo.Padding = new Padding(16);
            //
            // lblCodigo
            //
            this.lblCodigo.AutoSize = true;
            this.lblCodigo.Font = new Font("Segoe UI", 9F);
            this.lblCodigo.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblCodigo.Location = new Point(16, 14);
            this.lblCodigo.Text = "Escanear o escribir código de barras";
            //
            // txtCodigo
            //
            this.txtCodigo.BorderStyle = BorderStyle.FixedSingle;
            this.txtCodigo.Font = new Font("Segoe UI", 12F);
            this.txtCodigo.Location = new Point(16, 36);
            this.txtCodigo.Size = new Size(290, 30);
            this.txtCodigo.KeyDown += this.txtCodigo_KeyDown;
            //
            // lblBuscarNombre
            //
            this.lblBuscarNombre.AutoSize = true;
            this.lblBuscarNombre.Font = new Font("Segoe UI", 9F);
            this.lblBuscarNombre.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblBuscarNombre.Location = new Point(322, 14);
            this.lblBuscarNombre.Text = "O buscar por nombre";
            //
            // txtBuscar
            //
            this.txtBuscar.BorderStyle = BorderStyle.FixedSingle;
            this.txtBuscar.Font = new Font("Segoe UI", 12F);
            this.txtBuscar.Location = new Point(322, 36);
            this.txtBuscar.Size = new Size(300, 30);
            this.txtBuscar.TextChanged += this.txtBuscar_TextChanged;
            this.txtBuscar.KeyDown += this.txtBuscar_KeyDown;
            //
            // lblCantidad
            //
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Font = new Font("Segoe UI", 9F);
            this.lblCantidad.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblCantidad.Location = new Point(638, 14);
            this.lblCantidad.Text = "Cantidad";
            //
            // txtCantidad
            //
            this.txtCantidad.BorderStyle = BorderStyle.FixedSingle;
            this.txtCantidad.Font = new Font("Segoe UI", 12F);
            this.txtCantidad.Location = new Point(638, 36);
            this.txtCantidad.Size = new Size(70, 30);
            //
            // btnAgregar
            //
            this.btnAgregar.BackColor = Color.White;
            this.btnAgregar.FlatStyle = FlatStyle.Flat;
            this.btnAgregar.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 212);
            this.btnAgregar.Font = new Font("Segoe UI", 10F);
            this.btnAgregar.ForeColor = Color.FromArgb(28, 28, 30);
            this.btnAgregar.Cursor = Cursors.Hand;
            this.btnAgregar.Location = new Point(724, 35);
            this.btnAgregar.Size = new Size(110, 32);
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += this.btnAgregar_Click;
            //
            // lblMensaje
            //
            this.lblMensaje.AutoSize = true;
            this.lblMensaje.ForeColor = Color.FromArgb(163, 45, 45);
            this.lblMensaje.Font = new Font("Segoe UI", 9F);
            this.lblMensaje.Location = new Point(16, 74);
            this.lblMensaje.MaximumSize = new Size(600, 0);
            this.lblMensaje.Visible = false;
            //
            // lstSugerencias
            //
            this.lstSugerencias.BorderStyle = BorderStyle.FixedSingle;
            this.lstSugerencias.Font = new Font("Segoe UI", 11F);
            this.lstSugerencias.ItemHeight = 24;
            this.lstSugerencias.Location = new Point(322, 102);
            this.lstSugerencias.Size = new Size(300, 196);
            this.lstSugerencias.Visible = false;
            this.lstSugerencias.KeyDown += this.lstSugerencias_KeyDown;
            this.lstSugerencias.DoubleClick += this.lstSugerencias_DoubleClick;
            //
            // dgvCarrito
            //
            this.dgvCarrito.BackgroundColor = Color.White;
            this.dgvCarrito.BorderStyle = BorderStyle.None;
            this.dgvCarrito.Dock = DockStyle.Fill;
            this.dgvCarrito.AllowUserToAddRows = false;
            this.dgvCarrito.AllowUserToDeleteRows = false;
            this.dgvCarrito.ReadOnly = true;
            this.dgvCarrito.RowHeadersVisible = false;
            this.dgvCarrito.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvCarrito.MultiSelect = false;
            this.dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCarrito.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvCarrito.ColumnHeadersHeight = 38;
            this.dgvCarrito.RowTemplate.Height = 34;
            this.dgvCarrito.Font = new Font("Segoe UI", 10F);
            this.dgvCarrito.GridColor = Color.FromArgb(236, 236, 234);
            this.dgvCarrito.EnableHeadersVisualStyles = false;
            this.dgvCarrito.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(242, 242, 240);
            this.dgvCarrito.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(95, 94, 90);
            this.dgvCarrito.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            this.dgvCarrito.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            this.dgvCarrito.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            this.dgvCarrito.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 238, 236);
            this.dgvCarrito.DefaultCellStyle.SelectionForeColor = Color.FromArgb(28, 28, 30);
            this.dgvCarrito.Columns.AddRange(new DataGridViewColumn[] {
                this.colVId, this.colVNombre, this.colVPrecio,
                this.colVCantidad, this.colVSubtotal, this.colQuitar });
            this.dgvCarrito.CellClick += this.dgvCarrito_CellClick;
            //
            // colVId
            //
            this.colVId.Name = "colVId";
            this.colVId.HeaderText = "Id";
            this.colVId.Visible = false;
            //
            // colVNombre
            //
            this.colVNombre.Name = "colVNombre";
            this.colVNombre.HeaderText = "Producto";
            this.colVNombre.FillWeight = 200;
            //
            // colVPrecio
            //
            this.colVPrecio.Name = "colVPrecio";
            this.colVPrecio.HeaderText = "Precio";
            //
            // colVCantidad
            //
            this.colVCantidad.Name = "colVCantidad";
            this.colVCantidad.HeaderText = "Cant.";
            this.colVCantidad.FillWeight = 70;
            //
            // colVSubtotal
            //
            this.colVSubtotal.Name = "colVSubtotal";
            this.colVSubtotal.HeaderText = "Subtotal";
            //
            // colQuitar
            //
            this.colQuitar.Name = "colQuitar";
            this.colQuitar.HeaderText = "";
            this.colQuitar.Text = "Quitar";
            this.colQuitar.UseColumnTextForButtonValue = true;
            this.colQuitar.FlatStyle = FlatStyle.Flat;
            this.colQuitar.FillWeight = 70;
            //
            // pnlCobro
            //
            this.pnlCobro.BackColor = Color.White;
            this.pnlCobro.BorderStyle = BorderStyle.FixedSingle;
            this.pnlCobro.Controls.Add(this.lblTotalTitulo);
            this.pnlCobro.Controls.Add(this.lblTotal);
            this.pnlCobro.Controls.Add(this.lblMedioPago);
            this.pnlCobro.Controls.Add(this.comboMedioPago);
            this.pnlCobro.Controls.Add(this.btnCobrar);
            this.pnlCobro.Controls.Add(this.btnCancelar);
            this.pnlCobro.Dock = DockStyle.Right;
            this.pnlCobro.Width = 300;
            this.pnlCobro.Padding = new Padding(20);
            //
            // lblTotalTitulo
            //
            this.lblTotalTitulo.AutoSize = true;
            this.lblTotalTitulo.Font = new Font("Segoe UI", 10F);
            this.lblTotalTitulo.ForeColor = Color.FromArgb(142, 142, 147);
            this.lblTotalTitulo.Location = new Point(24, 30);
            this.lblTotalTitulo.Text = "Total a pagar";
            //
            // lblTotal
            //
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new Font("Segoe UI Semibold", 30F, FontStyle.Bold);
            this.lblTotal.ForeColor = Color.FromArgb(28, 28, 30);
            this.lblTotal.Location = new Point(20, 52);
            this.lblTotal.Text = "$0";
            //
            // lblMedioPago
            //
            this.lblMedioPago.AutoSize = true;
            this.lblMedioPago.Font = new Font("Segoe UI", 9F);
            this.lblMedioPago.ForeColor = Color.FromArgb(99, 99, 102);
            this.lblMedioPago.Location = new Point(24, 130);
            this.lblMedioPago.Text = "Medio de pago";
            //
            // comboMedioPago
            //
            this.comboMedioPago.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboMedioPago.FlatStyle = FlatStyle.Flat;
            this.comboMedioPago.Font = new Font("Segoe UI", 11F);
            this.comboMedioPago.Location = new Point(24, 152);
            this.comboMedioPago.Size = new Size(240, 28);
            //
            // btnCobrar
            //
            this.btnCobrar.BackColor = Color.FromArgb(28, 28, 30);
            this.btnCobrar.FlatStyle = FlatStyle.Flat;
            this.btnCobrar.FlatAppearance.BorderSize = 0;
            this.btnCobrar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnCobrar.ForeColor = Color.White;
            this.btnCobrar.Cursor = Cursors.Hand;
            this.btnCobrar.Location = new Point(24, 210);
            this.btnCobrar.Size = new Size(240, 48);
            this.btnCobrar.Text = "Cobrar (F12)";
            this.btnCobrar.UseVisualStyleBackColor = false;
            this.btnCobrar.Click += this.btnCobrar_Click;
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = Color.White;
            this.btnCancelar.FlatStyle = FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 212);
            this.btnCancelar.Font = new Font("Segoe UI", 10F);
            this.btnCancelar.ForeColor = Color.FromArgb(95, 94, 90);
            this.btnCancelar.Cursor = Cursors.Hand;
            this.btnCancelar.Location = new Point(24, 268);
            this.btnCancelar.Size = new Size(240, 38);
            this.btnCancelar.Text = "Cancelar venta";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += this.btnCancelar_Click;
            //
            // FormVentas
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.FromArgb(250, 250, 249);
            this.Controls.Add(this.lstSugerencias);
            this.Controls.Add(this.dgvCarrito);
            this.Controls.Add(this.pnlCobro);
            this.Controls.Add(this.pnlEscaneo);
            this.Name = "FormVentas";
            this.Text = "Ventas";
            this.Load += this.FormVentas_Load;
            this.pnlEscaneo.ResumeLayout(false);
            this.pnlEscaneo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarrito)).EndInit();
            this.pnlCobro.ResumeLayout(false);
            this.pnlCobro.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

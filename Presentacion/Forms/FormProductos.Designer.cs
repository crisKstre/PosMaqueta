using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormProductos
    {
        private System.ComponentModel.IContainer components = null;

        private Panel    pnlFormulario;
        private Label    lblModo;
        private Label    lblCodigo;      private TextBox  txtCodigo;
        private Label    lblNombre;      private TextBox  txtNombre;
        private Label    lblCategoria;   private ComboBox comboCategoria;
        private Button   btnGestionarCat;
        private Label    lblPrecio;      private TextBox  txtPrecio;
        private Label    lblCosto;       private TextBox  txtCosto;
        private Label    lblStock;       private TextBox  txtStock;
        private Label    lblStockMin;    private TextBox  txtStockMin;
        private Label    lblUnidad;      private ComboBox comboUnidad;
        private Button   btnGuardar;
        private Button   btnCancelar;
        private Label    lblError;
        private Panel    pnlAcciones;
        private Label    lblBuscar;      private TextBox  txtBuscar;
        private Label    lblCantidad;    private TextBox  txtCantidad;
        private Button   btnAgregar;
        private Button   btnDescontar;
        private Button   btnDesactivar;
        private Button   btnEliminar;
        private Button   btnLog;
        private Panel              pnlLog;
        private Splitter           splitterLog;
        private Panel              pnlLogFiltros;
        private Label              lblDesde;  private DateTimePicker dtpDesde;
        private Label              lblHasta;  private DateTimePicker dtpHasta;
        private Button             btnFiltrarLog;
        private DataGridView       dgvLog;
        private DataGridViewTextBoxColumn colLogFecha;
        private DataGridViewTextBoxColumn colLogUsuario;
        private DataGridViewTextBoxColumn colLogAccion;
        private DataGridViewTextBoxColumn colLogDetalle;
        private DataGridView       dgvProductos;
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colCodigo;
        private DataGridViewTextBoxColumn colNombre;
        private DataGridViewTextBoxColumn colCategoria;
        private DataGridViewTextBoxColumn colPrecio;
        private DataGridViewTextBoxColumn colDescuento;
        private DataGridViewTextBoxColumn colStock;
        private DataGridViewTextBoxColumn colEstado;
        private DataGridViewTextBoxColumn colCosto;
        private DataGridViewTextBoxColumn colMargen;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlFormulario = new System.Windows.Forms.Panel();
            this.lblModo = new System.Windows.Forms.Label();
            this.lblCodigo = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblCategoria = new System.Windows.Forms.Label();
            this.comboCategoria = new System.Windows.Forms.ComboBox();
            this.btnGestionarCat = new System.Windows.Forms.Button();
            this.lblPrecio = new System.Windows.Forms.Label();
            this.txtPrecio = new System.Windows.Forms.TextBox();
            this.lblCosto = new System.Windows.Forms.Label();
            this.txtCosto = new System.Windows.Forms.TextBox();
            this.lblStock = new System.Windows.Forms.Label();
            this.txtStock = new System.Windows.Forms.TextBox();
            this.lblStockMin = new System.Windows.Forms.Label();
            this.txtStockMin = new System.Windows.Forms.TextBox();
            this.lblUnidad = new System.Windows.Forms.Label();
            this.comboUnidad = new System.Windows.Forms.ComboBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.lblError = new System.Windows.Forms.Label();
            this.pnlAcciones = new System.Windows.Forms.Panel();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.lblCantidad = new System.Windows.Forms.Label();
            this.txtCantidad = new System.Windows.Forms.TextBox();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.btnDescontar = new System.Windows.Forms.Button();
            this.btnDesactivar = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnLog = new System.Windows.Forms.Button();
            this.pnlLog = new System.Windows.Forms.Panel();
            this.pnlLogFiltros = new System.Windows.Forms.Panel();
            this.lblDesde = new System.Windows.Forms.Label();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.lblHasta = new System.Windows.Forms.Label();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.btnFiltrarLog = new System.Windows.Forms.Button();
            this.dgvLog = new System.Windows.Forms.DataGridView();
            this.colLogFecha = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogUsuario = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogAccion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogDetalle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitterLog = new System.Windows.Forms.Splitter();
            this.dgvProductos = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCodigo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategoria = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrecio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescuento = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStock = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEstado = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCosto = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMargen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlFormulario.SuspendLayout();
            this.pnlAcciones.SuspendLayout();
            this.pnlLog.SuspendLayout();
            this.pnlLogFiltros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFormulario
            // 
            this.pnlFormulario.BackColor = System.Drawing.Color.White;
            this.pnlFormulario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFormulario.Controls.Add(this.lblModo);
            this.pnlFormulario.Controls.Add(this.lblCodigo);
            this.pnlFormulario.Controls.Add(this.txtCodigo);
            this.pnlFormulario.Controls.Add(this.lblNombre);
            this.pnlFormulario.Controls.Add(this.txtNombre);
            this.pnlFormulario.Controls.Add(this.lblCategoria);
            this.pnlFormulario.Controls.Add(this.comboCategoria);
            this.pnlFormulario.Controls.Add(this.btnGestionarCat);
            this.pnlFormulario.Controls.Add(this.lblPrecio);
            this.pnlFormulario.Controls.Add(this.txtPrecio);
            this.pnlFormulario.Controls.Add(this.lblCosto);
            this.pnlFormulario.Controls.Add(this.txtCosto);
            this.pnlFormulario.Controls.Add(this.lblStock);
            this.pnlFormulario.Controls.Add(this.txtStock);
            this.pnlFormulario.Controls.Add(this.lblStockMin);
            this.pnlFormulario.Controls.Add(this.txtStockMin);
            this.pnlFormulario.Controls.Add(this.lblUnidad);
            this.pnlFormulario.Controls.Add(this.comboUnidad);
            this.pnlFormulario.Controls.Add(this.btnGuardar);
            this.pnlFormulario.Controls.Add(this.btnCancelar);
            this.pnlFormulario.Controls.Add(this.lblError);
            this.pnlFormulario.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFormulario.Location = new System.Drawing.Point(0, 0);
            this.pnlFormulario.Name = "pnlFormulario";
            this.pnlFormulario.Padding = new System.Windows.Forms.Padding(18);
            this.pnlFormulario.Size = new System.Drawing.Size(1583, 210);
            this.pnlFormulario.TabIndex = 4;
            this.pnlFormulario.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlFormulario_Paint);
            // 
            // lblModo
            // 
            this.lblModo.AutoSize = true;
            this.lblModo.Location = new System.Drawing.Point(18, 14);
            this.lblModo.Name = "lblModo";
            this.lblModo.Size = new System.Drawing.Size(103, 16);
            this.lblModo.TabIndex = 0;
            this.lblModo.Text = "Nuevo producto";
            // 
            // lblCodigo
            // 
            this.lblCodigo.AutoSize = true;
            this.lblCodigo.Location = new System.Drawing.Point(18, 52);
            this.lblCodigo.Name = "lblCodigo";
            this.lblCodigo.Size = new System.Drawing.Size(140, 16);
            this.lblCodigo.TabIndex = 1;
            this.lblCodigo.Text = "CÓDIGO DE BARRAS";
            // 
            // txtCodigo
            // 
            this.txtCodigo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCodigo.Location = new System.Drawing.Point(18, 74);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(200, 22);
            this.txtCodigo.TabIndex = 2;
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(236, 52);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(66, 16);
            this.lblNombre.TabIndex = 3;
            this.lblNombre.Text = "NOMBRE";
            // 
            // txtNombre
            // 
            this.txtNombre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNombre.Location = new System.Drawing.Point(236, 74);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(280, 22);
            this.txtNombre.TabIndex = 4;
            // 
            // lblCategoria
            // 
            this.lblCategoria.AutoSize = true;
            this.lblCategoria.Location = new System.Drawing.Point(534, 52);
            this.lblCategoria.Name = "lblCategoria";
            this.lblCategoria.Size = new System.Drawing.Size(85, 16);
            this.lblCategoria.TabIndex = 5;
            this.lblCategoria.Text = "CATEGORÍA";
            // 
            // comboCategoria
            // 
            this.comboCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCategoria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCategoria.Location = new System.Drawing.Point(534, 74);
            this.comboCategoria.Name = "comboCategoria";
            this.comboCategoria.Size = new System.Drawing.Size(210, 24);
            this.comboCategoria.TabIndex = 6;
            // 
            // btnGestionarCat
            // 
            this.btnGestionarCat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGestionarCat.Location = new System.Drawing.Point(752, 74);
            this.btnGestionarCat.Name = "btnGestionarCat";
            this.btnGestionarCat.Size = new System.Drawing.Size(120, 38);
            this.btnGestionarCat.TabIndex = 7;
            this.btnGestionarCat.Text = "Gestionar ▸";
            this.btnGestionarCat.UseVisualStyleBackColor = false;
            this.btnGestionarCat.Click += new System.EventHandler(this.btnGestionarCat_Click);
            // 
            // lblPrecio
            // 
            this.lblPrecio.AutoSize = true;
            this.lblPrecio.Location = new System.Drawing.Point(18, 130);
            this.lblPrecio.Name = "lblPrecio";
            this.lblPrecio.Size = new System.Drawing.Size(57, 16);
            this.lblPrecio.TabIndex = 8;
            this.lblPrecio.Text = "PRECIO";
            // 
            // txtPrecio
            // 
            this.txtPrecio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPrecio.Location = new System.Drawing.Point(18, 152);
            this.txtPrecio.Name = "txtPrecio";
            this.txtPrecio.Size = new System.Drawing.Size(110, 22);
            this.txtPrecio.TabIndex = 9;
            // 
            // lblCosto
            // 
            this.lblCosto.AutoSize = true;
            this.lblCosto.Location = new System.Drawing.Point(140, 130);
            this.lblCosto.Name = "lblCosto";
            this.lblCosto.Size = new System.Drawing.Size(54, 16);
            this.lblCosto.TabIndex = 10;
            this.lblCosto.Text = "COSTO";
            // 
            // txtCosto
            // 
            this.txtCosto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCosto.Location = new System.Drawing.Point(140, 152);
            this.txtCosto.Name = "txtCosto";
            this.txtCosto.Size = new System.Drawing.Size(110, 22);
            this.txtCosto.TabIndex = 11;
            // 
            // lblStock
            // 
            this.lblStock.AutoSize = true;
            this.lblStock.Location = new System.Drawing.Point(262, 130);
            this.lblStock.Name = "lblStock";
            this.lblStock.Size = new System.Drawing.Size(52, 16);
            this.lblStock.TabIndex = 12;
            this.lblStock.Text = "STOCK";
            // 
            // txtStock
            // 
            this.txtStock.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStock.Location = new System.Drawing.Point(262, 152);
            this.txtStock.Name = "txtStock";
            this.txtStock.Size = new System.Drawing.Size(100, 22);
            this.txtStock.TabIndex = 13;
            // 
            // lblStockMin
            // 
            this.lblStockMin.AutoSize = true;
            this.lblStockMin.Location = new System.Drawing.Point(374, 130);
            this.lblStockMin.Name = "lblStockMin";
            this.lblStockMin.Size = new System.Drawing.Size(82, 16);
            this.lblStockMin.TabIndex = 14;
            this.lblStockMin.Text = "STOCK MÍN.";
            // 
            // txtStockMin
            // 
            this.txtStockMin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStockMin.Location = new System.Drawing.Point(374, 152);
            this.txtStockMin.Name = "txtStockMin";
            this.txtStockMin.Size = new System.Drawing.Size(100, 22);
            this.txtStockMin.TabIndex = 15;
            // 
            // lblUnidad
            // 
            this.lblUnidad.AutoSize = true;
            this.lblUnidad.Location = new System.Drawing.Point(486, 130);
            this.lblUnidad.Name = "lblUnidad";
            this.lblUnidad.Size = new System.Drawing.Size(59, 16);
            this.lblUnidad.TabIndex = 16;
            this.lblUnidad.Text = "UNIDAD";
            // 
            // comboUnidad
            // 
            this.comboUnidad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUnidad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboUnidad.Location = new System.Drawing.Point(486, 152);
            this.comboUnidad.Name = "comboUnidad";
            this.comboUnidad.Size = new System.Drawing.Size(100, 24);
            this.comboUnidad.TabIndex = 17;
            // 
            // btnGuardar
            // 
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Location = new System.Drawing.Point(598, 152);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(120, 38);
            this.btnGuardar.TabIndex = 18;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Location = new System.Drawing.Point(730, 152);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(110, 38);
            this.btnCancelar.TabIndex = 19;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Visible = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(18, 196);
            this.lblError.MaximumSize = new System.Drawing.Size(800, 0);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(0, 16);
            this.lblError.TabIndex = 20;
            this.lblError.Visible = false;
            // 
            // pnlAcciones
            // 
            this.pnlAcciones.Controls.Add(this.lblBuscar);
            this.pnlAcciones.Controls.Add(this.txtBuscar);
            this.pnlAcciones.Controls.Add(this.lblCantidad);
            this.pnlAcciones.Controls.Add(this.txtCantidad);
            this.pnlAcciones.Controls.Add(this.btnAgregar);
            this.pnlAcciones.Controls.Add(this.btnDescontar);
            this.pnlAcciones.Controls.Add(this.btnDesactivar);
            this.pnlAcciones.Controls.Add(this.btnEliminar);
            this.pnlAcciones.Controls.Add(this.btnLog);
            this.pnlAcciones.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAcciones.Location = new System.Drawing.Point(0, 210);
            this.pnlAcciones.Name = "pnlAcciones";
            this.pnlAcciones.Size = new System.Drawing.Size(1583, 68);
            this.pnlAcciones.TabIndex = 3;
            this.pnlAcciones.Resize += new System.EventHandler(this.pnlAcciones_Resize);
            // 
            // lblBuscar
            // 
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Location = new System.Drawing.Point(7, 26);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Size = new System.Drawing.Size(63, 16);
            this.lblBuscar.TabIndex = 0;
            this.lblBuscar.Text = "BUSCAR";
            // 
            // txtBuscar
            // 
            this.txtBuscar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBuscar.Location = new System.Drawing.Point(95, 22);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(220, 22);
            this.txtBuscar.TabIndex = 1;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            // 
            // lblCantidad
            // 
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Location = new System.Drawing.Point(335, 26);
            this.lblCantidad.Name = "lblCantidad";
            this.lblCantidad.Size = new System.Drawing.Size(76, 16);
            this.lblCantidad.TabIndex = 2;
            this.lblCantidad.Text = "CANTIDAD";
            // 
            // txtCantidad
            // 
            this.txtCantidad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCantidad.Location = new System.Drawing.Point(409, 22);
            this.txtCantidad.Name = "txtCantidad";
            this.txtCantidad.Size = new System.Drawing.Size(80, 22);
            this.txtCantidad.TabIndex = 3;
            this.txtCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnAgregar
            // 
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Location = new System.Drawing.Point(505, 14);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(130, 40);
            this.btnAgregar.TabIndex = 4;
            this.btnAgregar.Text = "Agregar stock";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // btnDescontar
            // 
            this.btnDescontar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDescontar.Location = new System.Drawing.Point(643, 14);
            this.btnDescontar.Name = "btnDescontar";
            this.btnDescontar.Size = new System.Drawing.Size(140, 40);
            this.btnDescontar.TabIndex = 5;
            this.btnDescontar.Text = "Descontar stock";
            this.btnDescontar.UseVisualStyleBackColor = false;
            this.btnDescontar.Click += new System.EventHandler(this.btnDescontar_Click);
            // 
            // btnDesactivar
            // 
            this.btnDesactivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesactivar.Location = new System.Drawing.Point(793, 14);
            this.btnDesactivar.Name = "btnDesactivar";
            this.btnDesactivar.Size = new System.Drawing.Size(110, 40);
            this.btnDesactivar.TabIndex = 6;
            this.btnDesactivar.Text = "Desactivar";
            this.btnDesactivar.UseVisualStyleBackColor = false;
            this.btnDesactivar.Click += new System.EventHandler(this.btnDesactivar_Click);
            // 
            // btnEliminar
            // 
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.Location = new System.Drawing.Point(913, 14);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(100, 40);
            this.btnEliminar.TabIndex = 7;
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // btnLog
            // 
            this.btnLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLog.Location = new System.Drawing.Point(1025, 14);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(140, 40);
            this.btnLog.TabIndex = 8;
            this.btnLog.Text = "▼ Log inventario";
            this.btnLog.UseVisualStyleBackColor = false;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // pnlLog
            // 
            this.pnlLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLog.Controls.Add(this.pnlLogFiltros);
            this.pnlLog.Controls.Add(this.dgvLog);
            this.pnlLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLog.Location = new System.Drawing.Point(0, 480);
            this.pnlLog.MinimumSize = new System.Drawing.Size(0, 110);
            this.pnlLog.Name = "pnlLog";
            this.pnlLog.Size = new System.Drawing.Size(1583, 220);
            this.pnlLog.TabIndex = 1;
            this.pnlLog.Visible = false;
            // 
            // pnlLogFiltros
            // 
            this.pnlLogFiltros.Controls.Add(this.lblDesde);
            this.pnlLogFiltros.Controls.Add(this.dtpDesde);
            this.pnlLogFiltros.Controls.Add(this.lblHasta);
            this.pnlLogFiltros.Controls.Add(this.dtpHasta);
            this.pnlLogFiltros.Controls.Add(this.btnFiltrarLog);
            this.pnlLogFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogFiltros.Location = new System.Drawing.Point(0, 0);
            this.pnlLogFiltros.Name = "pnlLogFiltros";
            this.pnlLogFiltros.Size = new System.Drawing.Size(1581, 54);
            this.pnlLogFiltros.TabIndex = 0;
            // 
            // lblDesde
            // 
            this.lblDesde.AutoSize = true;
            this.lblDesde.Location = new System.Drawing.Point(18, 16);
            this.lblDesde.Name = "lblDesde";
            this.lblDesde.Size = new System.Drawing.Size(51, 16);
            this.lblDesde.TabIndex = 0;
            this.lblDesde.Text = "Desde:";
            // 
            // dtpDesde
            // 
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesde.Location = new System.Drawing.Point(78, 12);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.Size = new System.Drawing.Size(148, 22);
            this.dtpDesde.TabIndex = 1;
            // 
            // lblHasta
            // 
            this.lblHasta.AutoSize = true;
            this.lblHasta.Location = new System.Drawing.Point(250, 16);
            this.lblHasta.Name = "lblHasta";
            this.lblHasta.Size = new System.Drawing.Size(46, 16);
            this.lblHasta.TabIndex = 2;
            this.lblHasta.Text = "Hasta:";
            // 
            // dtpHasta
            // 
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHasta.Location = new System.Drawing.Point(308, 12);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.Size = new System.Drawing.Size(148, 22);
            this.dtpHasta.TabIndex = 3;
            // 
            // btnFiltrarLog
            // 
            this.btnFiltrarLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFiltrarLog.Location = new System.Drawing.Point(476, 12);
            this.btnFiltrarLog.Name = "btnFiltrarLog";
            this.btnFiltrarLog.Size = new System.Drawing.Size(90, 30);
            this.btnFiltrarLog.TabIndex = 4;
            this.btnFiltrarLog.Text = "Filtrar";
            this.btnFiltrarLog.UseVisualStyleBackColor = false;
            this.btnFiltrarLog.Click += new System.EventHandler(this.btnFiltrarLog_Click);
            // 
            // dgvLog
            // 
            this.dgvLog.AllowUserToAddRows = false;
            this.dgvLog.AllowUserToDeleteRows = false;
            this.dgvLog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvLog.ColumnHeadersHeight = 29;
            this.dgvLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLogFecha,
            this.colLogUsuario,
            this.colLogAccion,
            this.colLogDetalle});
            this.dgvLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLog.EnableHeadersVisualStyles = false;
            this.dgvLog.Location = new System.Drawing.Point(0, 0);
            this.dgvLog.MultiSelect = false;
            this.dgvLog.Name = "dgvLog";
            this.dgvLog.ReadOnly = true;
            this.dgvLog.RowHeadersVisible = false;
            this.dgvLog.RowHeadersWidth = 51;
            this.dgvLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLog.Size = new System.Drawing.Size(1581, 218);
            this.dgvLog.TabIndex = 1;
            // 
            // colLogFecha
            // 
            this.colLogFecha.FillWeight = 120F;
            this.colLogFecha.HeaderText = "Fecha/Hora";
            this.colLogFecha.MinimumWidth = 6;
            this.colLogFecha.Name = "colLogFecha";
            this.colLogFecha.ReadOnly = true;
            // 
            // colLogUsuario
            // 
            this.colLogUsuario.HeaderText = "Usuario";
            this.colLogUsuario.MinimumWidth = 6;
            this.colLogUsuario.Name = "colLogUsuario";
            this.colLogUsuario.ReadOnly = true;
            // 
            // colLogAccion
            // 
            this.colLogAccion.FillWeight = 110F;
            this.colLogAccion.HeaderText = "Acción";
            this.colLogAccion.MinimumWidth = 6;
            this.colLogAccion.Name = "colLogAccion";
            this.colLogAccion.ReadOnly = true;
            // 
            // colLogDetalle
            // 
            this.colLogDetalle.FillWeight = 300F;
            this.colLogDetalle.HeaderText = "Detalle";
            this.colLogDetalle.MinimumWidth = 6;
            this.colLogDetalle.Name = "colLogDetalle";
            this.colLogDetalle.ReadOnly = true;
            // 
            // splitterLog
            // 
            this.splitterLog.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitterLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitterLog.Location = new System.Drawing.Point(0, 700);
            this.splitterLog.Name = "splitterLog";
            this.splitterLog.Size = new System.Drawing.Size(1583, 5);
            this.splitterLog.TabIndex = 2;
            this.splitterLog.TabStop = false;
            this.splitterLog.Visible = false;
            // 
            // dgvProductos
            // 
            this.dgvProductos.AllowUserToAddRows = false;
            this.dgvProductos.AllowUserToDeleteRows = false;
            this.dgvProductos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProductos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvProductos.ColumnHeadersHeight = 29;
            this.dgvProductos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvProductos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colCodigo,
            this.colNombre,
            this.colCategoria,
            this.colPrecio,
            this.colDescuento,
            this.colStock,
            this.colEstado,
            this.colCosto,
            this.colMargen});
            this.dgvProductos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProductos.EnableHeadersVisualStyles = false;
            this.dgvProductos.Location = new System.Drawing.Point(0, 278);
            this.dgvProductos.MultiSelect = false;
            this.dgvProductos.Name = "dgvProductos";
            this.dgvProductos.ReadOnly = true;
            this.dgvProductos.RowHeadersVisible = false;
            this.dgvProductos.RowHeadersWidth = 51;
            this.dgvProductos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProductos.Size = new System.Drawing.Size(1583, 202);
            this.dgvProductos.TabIndex = 0;
            this.dgvProductos.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProductos_CellDoubleClick);
            this.dgvProductos.SelectionChanged += new System.EventHandler(this.dgvProductos_SelectionChanged);
            // 
            // colId
            // 
            this.colId.MinimumWidth = 6;
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Visible = false;
            // 
            // colCodigo
            // 
            this.colCodigo.HeaderText = "Código";
            this.colCodigo.MinimumWidth = 6;
            this.colCodigo.Name = "colCodigo";
            this.colCodigo.ReadOnly = true;
            // 
            // colNombre
            // 
            this.colNombre.FillWeight = 220F;
            this.colNombre.HeaderText = "Nombre";
            this.colNombre.MinimumWidth = 6;
            this.colNombre.Name = "colNombre";
            this.colNombre.ReadOnly = true;
            // 
            // colCategoria
            // 
            this.colCategoria.HeaderText = "Categoría";
            this.colCategoria.MinimumWidth = 6;
            this.colCategoria.Name = "colCategoria";
            this.colCategoria.ReadOnly = true;
            // 
            // colPrecio
            // 
            this.colPrecio.HeaderText = "Precio";
            this.colPrecio.MinimumWidth = 6;
            this.colPrecio.Name = "colPrecio";
            this.colPrecio.ReadOnly = true;
            // 
            // colDescuento
            // 
            this.colDescuento.FillWeight = 60F;
            this.colDescuento.HeaderText = "Desc.";
            this.colDescuento.MinimumWidth = 6;
            this.colDescuento.Name = "colDescuento";
            this.colDescuento.ReadOnly = true;
            // 
            // colStock
            // 
            this.colStock.HeaderText = "Stock";
            this.colStock.MinimumWidth = 6;
            this.colStock.Name = "colStock";
            this.colStock.ReadOnly = true;
            // 
            // colEstado
            // 
            this.colEstado.FillWeight = 80F;
            this.colEstado.HeaderText = "Estado";
            this.colEstado.MinimumWidth = 6;
            this.colEstado.Name = "colEstado";
            this.colEstado.ReadOnly = true;
            // 
            // colCosto
            // 
            this.colCosto.FillWeight = 70F;
            this.colCosto.HeaderText = "Costo";
            this.colCosto.MinimumWidth = 6;
            this.colCosto.Name = "colCosto";
            this.colCosto.ReadOnly = true;
            // 
            // colMargen
            // 
            this.colMargen.FillWeight = 95F;
            this.colMargen.HeaderText = "Margen";
            this.colMargen.MinimumWidth = 6;
            this.colMargen.Name = "colMargen";
            this.colMargen.ReadOnly = true;
            // 
            // FormProductos
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1583, 705);
            this.Controls.Add(this.dgvProductos);
            this.Controls.Add(this.pnlLog);
            this.Controls.Add(this.splitterLog);
            this.Controls.Add(this.pnlAcciones);
            this.Controls.Add(this.pnlFormulario);
            this.Name = "FormProductos";
            this.Text = "Productos";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormProductos_FormClosed);
            this.Load += new System.EventHandler(this.FormProductos_Load);
            this.pnlFormulario.ResumeLayout(false);
            this.pnlFormulario.PerformLayout();
            this.pnlAcciones.ResumeLayout(false);
            this.pnlAcciones.PerformLayout();
            this.pnlLog.ResumeLayout(false);
            this.pnlLogFiltros.ResumeLayout(false);
            this.pnlLogFiltros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

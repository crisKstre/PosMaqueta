using System.Drawing;
using System.Windows.Forms;

namespace Presentacion.Forms
{
    partial class FormVentas
    {
        private System.ComponentModel.IContainer components = null;

        // Barra superior de escaneo
        private Panel    pnlEscaneo;
        private Label    lblCodigo;       private TextBox  txtCodigo;
        private Label    lblBuscarNombre; private TextBox  txtBuscar;
        private Label    lblCantidad;     private TextBox  txtCantidad;
        private Button   btnAgregar;
        private Label    lblMensaje;

        // Área central: categorías + grid + carrito
        private Panel              pnlCentro;
        private FlowLayoutPanel    pnlCats;
        private Panel              pnlGridWrap;
        private FlowLayoutPanel    pnlProdGrid;
        private Panel              pnlCarrito;

        // Carrito
        private Label              lblCarritoTitulo;
        private DataGridView       dgvCarrito;
        private DataGridViewTextBoxColumn colVId;
        private DataGridViewTextBoxColumn colVNombre;
        private DataGridViewTextBoxColumn colMenos;
        private DataGridViewTextBoxColumn colVCantidad;
        private DataGridViewTextBoxColumn colMas;
        private DataGridViewTextBoxColumn colVSubtotal;
        private DataGridViewTextBoxColumn colQuitar;

        // Panel cobro
        private Panel    pnlCobro;
        private Label    lblTotalLabel;
        private Label    lblTotal;
        private Label    lblMedioPago;
        private ComboBox comboMedioPago;
        private Button   btnCobrar;
        private Button   btnCancelar;
        private Button   btnVerLog;

        // Log
        private Panel              pnlLogVentas;
        private Splitter           splitterLogV;
        private Panel              pnlLogFiltrosV;
        private Label              lblDesdeV;     private DateTimePicker dtpDesdeV;
        private Label              lblHastaV;     private DateTimePicker dtpHastaV;
        private Button             btnFiltrarLogV;
        private DataGridView       dgvLogVentas;
        private DataGridViewTextBoxColumn colLVFecha;
        private DataGridViewTextBoxColumn colLVUsuario;
        private DataGridViewTextBoxColumn colLVAccion;
        private DataGridViewTextBoxColumn colLVDetalle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlEscaneo = new System.Windows.Forms.Panel();
            this.lblCodigo = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.lblBuscarNombre = new System.Windows.Forms.Label();
            this.txtBuscar = new System.Windows.Forms.TextBox();
            this.lblCantidad = new System.Windows.Forms.Label();
            this.txtCantidad = new System.Windows.Forms.TextBox();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.pnlCentro = new System.Windows.Forms.Panel();
            this.pnlGridWrap = new System.Windows.Forms.Panel();
            this.pnlProdGrid = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCats = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCarrito = new System.Windows.Forms.Panel();
            this.lblCarritoTitulo = new System.Windows.Forms.Label();
            this.dgvCarrito = new System.Windows.Forms.DataGridView();
            this.colVId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMenos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVCantidad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMas = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVSubtotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colQuitar = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlCobro = new System.Windows.Forms.Panel();
            this.lblTotalLabel = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblMedioPago = new System.Windows.Forms.Label();
            this.comboMedioPago = new System.Windows.Forms.ComboBox();
            this.btnCobrar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnVerLog = new System.Windows.Forms.Button();
            this.pnlLogVentas = new System.Windows.Forms.Panel();
            this.pnlLogFiltrosV = new System.Windows.Forms.Panel();
            this.lblDesdeV = new System.Windows.Forms.Label();
            this.dtpDesdeV = new System.Windows.Forms.DateTimePicker();
            this.lblHastaV = new System.Windows.Forms.Label();
            this.dtpHastaV = new System.Windows.Forms.DateTimePicker();
            this.btnFiltrarLogV = new System.Windows.Forms.Button();
            this.dgvLogVentas = new System.Windows.Forms.DataGridView();
            this.colLVFecha = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLVUsuario = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLVAccion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLVDetalle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitterLogV = new System.Windows.Forms.Splitter();
            this.pnlEscaneo.SuspendLayout();
            this.pnlCentro.SuspendLayout();
            this.pnlGridWrap.SuspendLayout();
            this.pnlCarrito.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarrito)).BeginInit();
            this.pnlCobro.SuspendLayout();
            this.pnlLogVentas.SuspendLayout();
            this.pnlLogFiltrosV.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogVentas)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlEscaneo
            // 
            this.pnlEscaneo.BackColor = System.Drawing.Color.White;
            this.pnlEscaneo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEscaneo.Controls.Add(this.lblCodigo);
            this.pnlEscaneo.Controls.Add(this.txtCodigo);
            this.pnlEscaneo.Controls.Add(this.lblBuscarNombre);
            this.pnlEscaneo.Controls.Add(this.txtBuscar);
            this.pnlEscaneo.Controls.Add(this.lblCantidad);
            this.pnlEscaneo.Controls.Add(this.txtCantidad);
            this.pnlEscaneo.Controls.Add(this.btnAgregar);
            this.pnlEscaneo.Controls.Add(this.lblMensaje);
            this.pnlEscaneo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEscaneo.Location = new System.Drawing.Point(0, 0);
            this.pnlEscaneo.Name = "pnlEscaneo";
            this.pnlEscaneo.Size = new System.Drawing.Size(1335, 100);
            this.pnlEscaneo.TabIndex = 4;
            // 
            // lblCodigo
            // 
            this.lblCodigo.AutoSize = true;
            this.lblCodigo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCodigo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(69)))), ((int)(((byte)(79)))));
            this.lblCodigo.Location = new System.Drawing.Point(16, 10);
            this.lblCodigo.Name = "lblCodigo";
            this.lblCodigo.Size = new System.Drawing.Size(122, 15);
            this.lblCodigo.TabIndex = 0;
            this.lblCodigo.Text = "CÓDIGO DE BARRAS";
            // 
            // txtCodigo
            // 
            this.txtCodigo.BackColor = System.Drawing.Color.White;
            this.txtCodigo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCodigo.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.txtCodigo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.txtCodigo.Location = new System.Drawing.Point(16, 32);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(290, 32);
            this.txtCodigo.TabIndex = 1;
            this.txtCodigo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodigo_KeyDown);
            // 
            // lblBuscarNombre
            // 
            this.lblBuscarNombre.AutoSize = true;
            this.lblBuscarNombre.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblBuscarNombre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(69)))), ((int)(((byte)(79)))));
            this.lblBuscarNombre.Location = new System.Drawing.Point(322, 10);
            this.lblBuscarNombre.Name = "lblBuscarNombre";
            this.lblBuscarNombre.Size = new System.Drawing.Size(135, 15);
            this.lblBuscarNombre.TabIndex = 2;
            this.lblBuscarNombre.Text = "BUSCAR POR NOMBRE";
            // 
            // txtBuscar
            // 
            this.txtBuscar.BackColor = System.Drawing.Color.White;
            this.txtBuscar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBuscar.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.txtBuscar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.txtBuscar.Location = new System.Drawing.Point(322, 32);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(290, 32);
            this.txtBuscar.TabIndex = 3;
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            this.txtBuscar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBuscar_KeyDown);
            // 
            // lblCantidad
            // 
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCantidad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(69)))), ((int)(((byte)(79)))));
            this.lblCantidad.Location = new System.Drawing.Point(628, 10);
            this.lblCantidad.Name = "lblCantidad";
            this.lblCantidad.Size = new System.Drawing.Size(68, 15);
            this.lblCantidad.TabIndex = 4;
            this.lblCantidad.Text = "CANTIDAD";
            // 
            // txtCantidad
            // 
            this.txtCantidad.BackColor = System.Drawing.Color.White;
            this.txtCantidad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCantidad.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.txtCantidad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.txtCantidad.Location = new System.Drawing.Point(628, 32);
            this.txtCantidad.Name = "txtCantidad";
            this.txtCantidad.Size = new System.Drawing.Size(80, 32);
            this.txtCantidad.TabIndex = 5;
            this.txtCantidad.Text = "1";
            this.txtCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnAgregar
            // 
            this.btnAgregar.BackColor = System.Drawing.Color.White;
            this.btnAgregar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAgregar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(228)))), ((int)(((byte)(224)))));
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnAgregar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(69)))), ((int)(((byte)(79)))));
            this.btnAgregar.Location = new System.Drawing.Point(720, 32);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(130, 44);
            this.btnAgregar.TabIndex = 6;
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // lblMensaje
            // 
            this.lblMensaje.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblMensaje.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblMensaje.Location = new System.Drawing.Point(16, 80);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(2033, 16);
            this.lblMensaje.TabIndex = 7;
            this.lblMensaje.Visible = false;
            //
            // pnlCentro
            // 
            this.pnlCentro.Controls.Add(this.pnlGridWrap);
            this.pnlCentro.Controls.Add(this.pnlCarrito);
            this.pnlCentro.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCentro.Location = new System.Drawing.Point(0, 100);
            this.pnlCentro.Name = "pnlCentro";
            this.pnlCentro.Size = new System.Drawing.Size(1335, 113);
            this.pnlCentro.TabIndex = 1;
            // 
            // pnlGridWrap
            // 
            this.pnlGridWrap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(244)))), ((int)(((byte)(241)))));
            this.pnlGridWrap.Controls.Add(this.pnlProdGrid);
            this.pnlGridWrap.Controls.Add(this.pnlCats);
            this.pnlGridWrap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridWrap.Location = new System.Drawing.Point(0, 0);
            this.pnlGridWrap.Name = "pnlGridWrap";
            this.pnlGridWrap.Padding = new System.Windows.Forms.Padding(10);
            this.pnlGridWrap.Size = new System.Drawing.Size(975, 113);
            this.pnlGridWrap.TabIndex = 0;
            // 
            // pnlProdGrid
            // 
            this.pnlProdGrid.AutoScroll = true;
            this.pnlProdGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(244)))), ((int)(((byte)(241)))));
            this.pnlProdGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlProdGrid.Location = new System.Drawing.Point(10, 54);
            this.pnlProdGrid.Name = "pnlProdGrid";
            this.pnlProdGrid.Size = new System.Drawing.Size(955, 49);
            this.pnlProdGrid.TabIndex = 0;
            this.pnlProdGrid.Resize += new System.EventHandler(this.pnlProdGrid_Resize);
            // 
            // pnlCats
            // 
            this.pnlCats.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(244)))), ((int)(((byte)(241)))));
            this.pnlCats.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCats.Location = new System.Drawing.Point(10, 10);
            this.pnlCats.Name = "pnlCats";
            this.pnlCats.Padding = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.pnlCats.Size = new System.Drawing.Size(955, 44);
            this.pnlCats.TabIndex = 1;
            this.pnlCats.WrapContents = false;
            // 
            // pnlCarrito
            // 
            this.pnlCarrito.BackColor = System.Drawing.Color.White;
            this.pnlCarrito.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCarrito.Controls.Add(this.lblCarritoTitulo);
            this.pnlCarrito.Controls.Add(this.dgvCarrito);
            this.pnlCarrito.Controls.Add(this.pnlCobro);
            this.pnlCarrito.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlCarrito.Location = new System.Drawing.Point(975, 0);
            this.pnlCarrito.Name = "pnlCarrito";
            this.pnlCarrito.Size = new System.Drawing.Size(480, 113);
            this.pnlCarrito.TabIndex = 1;
            // 
            // lblCarritoTitulo
            // 
            this.lblCarritoTitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCarritoTitulo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblCarritoTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.lblCarritoTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblCarritoTitulo.Name = "lblCarritoTitulo";
            this.lblCarritoTitulo.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblCarritoTitulo.Size = new System.Drawing.Size(358, 44);
            this.lblCarritoTitulo.TabIndex = 0;
            this.lblCarritoTitulo.Text = "Carrito";
            this.lblCarritoTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvCarrito
            // 
            this.dgvCarrito.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVId,
            this.colVNombre,
            this.colMenos,
            this.colVCantidad,
            this.colMas,
            this.colVSubtotal,
            this.colQuitar});
            this.dgvCarrito.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCarrito.Location = new System.Drawing.Point(0, 0);
            this.dgvCarrito.Name = "dgvCarrito";
            this.dgvCarrito.RowTemplate.Height = 36;
            this.dgvCarrito.Size = new System.Drawing.Size(358, 0);
            this.dgvCarrito.TabIndex = 1;
            this.dgvCarrito.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCarrito_CellClick);
            // 
            // colVId
            // 
            this.colVId.Name = "colVId";
            this.colVId.Visible = false;
            // 
            // colVNombre
            // 
            this.colVNombre.FillWeight = 150F;
            this.colVNombre.HeaderText = "Producto";
            this.colVNombre.Name = "colVNombre";
            // 
            // colMenos
            // 
            this.colMenos.FillWeight = 34F;
            this.colMenos.HeaderText = "";
            this.colMenos.Name = "colMenos";
            // 
            // colVCantidad
            // 
            this.colVCantidad.FillWeight = 50F;
            this.colVCantidad.HeaderText = "Cant.";
            this.colVCantidad.Name = "colVCantidad";
            // 
            // colMas
            // 
            this.colMas.FillWeight = 34F;
            this.colMas.HeaderText = "";
            this.colMas.Name = "colMas";
            // 
            // colVSubtotal
            // 
            this.colVSubtotal.HeaderText = "Subtotal";
            this.colVSubtotal.Name = "colVSubtotal";
            // 
            // colQuitar
            // 
            this.colQuitar.FillWeight = 36F;
            this.colQuitar.HeaderText = "";
            this.colQuitar.Name = "colQuitar";
            // 
            // pnlCobro
            // 
            this.pnlCobro.BackColor = System.Drawing.Color.White;
            this.pnlCobro.Controls.Add(this.lblTotalLabel);
            this.pnlCobro.Controls.Add(this.lblTotal);
            this.pnlCobro.Controls.Add(this.lblMedioPago);
            this.pnlCobro.Controls.Add(this.comboMedioPago);
            this.pnlCobro.Controls.Add(this.btnCobrar);
            this.pnlCobro.Controls.Add(this.btnCancelar);
            this.pnlCobro.Controls.Add(this.btnVerLog);
            this.pnlCobro.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlCobro.Location = new System.Drawing.Point(0, -165);
            this.pnlCobro.Name = "pnlCobro";
            this.pnlCobro.Size = new System.Drawing.Size(358, 276);
            this.pnlCobro.TabIndex = 2;
            // 
            // lblTotalLabel
            // 
            this.lblTotalLabel.AutoSize = true;
            this.lblTotalLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTotalLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(143)))), ((int)(((byte)(154)))));
            this.lblTotalLabel.Location = new System.Drawing.Point(18, 14);
            this.lblTotalLabel.Name = "lblTotalLabel";
            this.lblTotalLabel.Size = new System.Drawing.Size(90, 15);
            this.lblTotalLabel.TabIndex = 0;
            this.lblTotalLabel.Text = "TOTAL A PAGAR";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.lblTotal.Location = new System.Drawing.Point(15, 32);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(60, 47);
            this.lblTotal.TabIndex = 1;
            this.lblTotal.Text = "$0";
            // 
            // lblMedioPago
            // 
            this.lblMedioPago.AutoSize = true;
            this.lblMedioPago.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblMedioPago.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(69)))), ((int)(((byte)(79)))));
            this.lblMedioPago.Location = new System.Drawing.Point(18, 96);
            this.lblMedioPago.Name = "lblMedioPago";
            this.lblMedioPago.Size = new System.Drawing.Size(112, 19);
            this.lblMedioPago.TabIndex = 2;
            this.lblMedioPago.Text = "Medio de pago";
            // 
            // comboMedioPago
            // 
            this.comboMedioPago.Location = new System.Drawing.Point(18, 118);
            this.comboMedioPago.Name = "comboMedioPago";
            this.comboMedioPago.Size = new System.Drawing.Size(322, 21);
            this.comboMedioPago.TabIndex = 3;
            // 
            // btnCobrar
            // 
            this.btnCobrar.Location = new System.Drawing.Point(18, 166);
            this.btnCobrar.Name = "btnCobrar";
            this.btnCobrar.Size = new System.Drawing.Size(322, 50);
            this.btnCobrar.TabIndex = 4;
            this.btnCobrar.Text = "Cobrar — F12";
            this.btnCobrar.Click += new System.EventHandler(this.btnCobrar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Location = new System.Drawing.Point(18, 226);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(152, 38);
            this.btnCancelar.TabIndex = 5;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnVerLog
            // 
            this.btnVerLog.Location = new System.Drawing.Point(188, 226);
            this.btnVerLog.Name = "btnVerLog";
            this.btnVerLog.Size = new System.Drawing.Size(152, 38);
            this.btnVerLog.TabIndex = 6;
            this.btnVerLog.Text = "▼ Registro";
            this.btnVerLog.Click += new System.EventHandler(this.btnVerLog_Click);
            // 
            // pnlLogVentas
            // 
            this.pnlLogVentas.BackColor = System.Drawing.Color.White;
            this.pnlLogVentas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLogVentas.Controls.Add(this.pnlLogFiltrosV);
            this.pnlLogVentas.Controls.Add(this.dgvLogVentas);
            this.pnlLogVentas.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLogVentas.Location = new System.Drawing.Point(0, 213);
            this.pnlLogVentas.MinimumSize = new System.Drawing.Size(0, 120);
            this.pnlLogVentas.Name = "pnlLogVentas";
            this.pnlLogVentas.Size = new System.Drawing.Size(1335, 280);
            this.pnlLogVentas.TabIndex = 2;
            this.pnlLogVentas.Visible = false;
            // 
            // pnlLogFiltrosV
            // 
            this.pnlLogFiltrosV.BackColor = System.Drawing.Color.White;
            this.pnlLogFiltrosV.Controls.Add(this.lblDesdeV);
            this.pnlLogFiltrosV.Controls.Add(this.dtpDesdeV);
            this.pnlLogFiltrosV.Controls.Add(this.lblHastaV);
            this.pnlLogFiltrosV.Controls.Add(this.dtpHastaV);
            this.pnlLogFiltrosV.Controls.Add(this.btnFiltrarLogV);
            this.pnlLogFiltrosV.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogFiltrosV.Location = new System.Drawing.Point(0, 0);
            this.pnlLogFiltrosV.Name = "pnlLogFiltrosV";
            this.pnlLogFiltrosV.Size = new System.Drawing.Size(1333, 50);
            this.pnlLogFiltrosV.TabIndex = 0;
            // 
            // lblDesdeV
            // 
            this.lblDesdeV.AutoSize = true;
            this.lblDesdeV.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDesdeV.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(69)))), ((int)(((byte)(79)))));
            this.lblDesdeV.Location = new System.Drawing.Point(16, 15);
            this.lblDesdeV.Name = "lblDesdeV";
            this.lblDesdeV.Size = new System.Drawing.Size(50, 19);
            this.lblDesdeV.TabIndex = 0;
            this.lblDesdeV.Text = "Desde:";
            // 
            // dtpDesdeV
            // 
            this.dtpDesdeV.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.dtpDesdeV.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesdeV.Location = new System.Drawing.Point(74, 11);
            this.dtpDesdeV.Name = "dtpDesdeV";
            this.dtpDesdeV.Size = new System.Drawing.Size(148, 29);
            this.dtpDesdeV.TabIndex = 1;
            // 
            // lblHastaV
            // 
            this.lblHastaV.AutoSize = true;
            this.lblHastaV.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblHastaV.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(69)))), ((int)(((byte)(79)))));
            this.lblHastaV.Location = new System.Drawing.Point(240, 15);
            this.lblHastaV.Name = "lblHastaV";
            this.lblHastaV.Size = new System.Drawing.Size(47, 19);
            this.lblHastaV.TabIndex = 2;
            this.lblHastaV.Text = "Hasta:";
            // 
            // dtpHastaV
            // 
            this.dtpHastaV.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.dtpHastaV.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHastaV.Location = new System.Drawing.Point(296, 11);
            this.dtpHastaV.Name = "dtpHastaV";
            this.dtpHastaV.Size = new System.Drawing.Size(148, 29);
            this.dtpHastaV.TabIndex = 3;
            // 
            // btnFiltrarLogV
            // 
            this.btnFiltrarLogV.Location = new System.Drawing.Point(456, 10);
            this.btnFiltrarLogV.Name = "btnFiltrarLogV";
            this.btnFiltrarLogV.Size = new System.Drawing.Size(90, 30);
            this.btnFiltrarLogV.TabIndex = 4;
            this.btnFiltrarLogV.Text = "Filtrar";
            this.btnFiltrarLogV.Click += new System.EventHandler(this.btnFiltrarLogV_Click);
            // 
            // dgvLogVentas
            // 
            this.dgvLogVentas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLVFecha,
            this.colLVUsuario,
            this.colLVAccion,
            this.colLVDetalle});
            this.dgvLogVentas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLogVentas.Location = new System.Drawing.Point(0, 0);
            this.dgvLogVentas.Name = "dgvLogVentas";
            this.dgvLogVentas.Size = new System.Drawing.Size(1333, 278);
            this.dgvLogVentas.TabIndex = 1;
            // 
            // colLVFecha
            // 
            this.colLVFecha.FillWeight = 110F;
            this.colLVFecha.HeaderText = "Fecha";
            this.colLVFecha.Name = "colLVFecha";
            // 
            // colLVUsuario
            // 
            this.colLVUsuario.HeaderText = "Usuario";
            this.colLVUsuario.Name = "colLVUsuario";
            // 
            // colLVAccion
            // 
            this.colLVAccion.FillWeight = 80F;
            this.colLVAccion.HeaderText = "Acción";
            this.colLVAccion.Name = "colLVAccion";
            // 
            // colLVDetalle
            // 
            this.colLVDetalle.FillWeight = 300F;
            this.colLVDetalle.HeaderText = "Detalle";
            this.colLVDetalle.Name = "colLVDetalle";
            // 
            // splitterLogV
            // 
            this.splitterLogV.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(228)))), ((int)(((byte)(228)))), ((int)(((byte)(224)))));
            this.splitterLogV.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitterLogV.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitterLogV.Location = new System.Drawing.Point(0, 493);
            this.splitterLogV.Name = "splitterLogV";
            this.splitterLogV.Size = new System.Drawing.Size(1335, 5);
            this.splitterLogV.TabIndex = 3;
            this.splitterLogV.TabStop = false;
            this.splitterLogV.Visible = false;
            // 
            // FormVentas
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(244)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(1335, 498);
            this.Controls.Add(this.pnlCentro);
            this.Controls.Add(this.pnlLogVentas);
            this.Controls.Add(this.splitterLogV);
            this.Controls.Add(this.pnlEscaneo);
            this.Name = "FormVentas";
            this.Text = "Ventas";
            this.Load += new System.EventHandler(this.FormVentas_Load);
            this.pnlEscaneo.ResumeLayout(false);
            this.pnlEscaneo.PerformLayout();
            this.pnlCentro.ResumeLayout(false);
            this.pnlGridWrap.ResumeLayout(false);
            this.pnlCarrito.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarrito)).EndInit();
            this.pnlCobro.ResumeLayout(false);
            this.pnlCobro.PerformLayout();
            this.pnlLogVentas.ResumeLayout(false);
            this.pnlLogFiltrosV.ResumeLayout(false);
            this.pnlLogFiltrosV.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogVentas)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

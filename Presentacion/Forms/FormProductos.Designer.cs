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

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlFormulario  = new System.Windows.Forms.Panel();
            this.lblModo        = new System.Windows.Forms.Label();
            this.lblCodigo      = new System.Windows.Forms.Label();
            this.txtCodigo      = new System.Windows.Forms.TextBox();
            this.lblNombre      = new System.Windows.Forms.Label();
            this.txtNombre      = new System.Windows.Forms.TextBox();
            this.lblCategoria   = new System.Windows.Forms.Label();
            this.comboCategoria = new System.Windows.Forms.ComboBox();
            this.btnGestionarCat = new System.Windows.Forms.Button();
            this.lblPrecio      = new System.Windows.Forms.Label();
            this.txtPrecio      = new System.Windows.Forms.TextBox();
            this.lblStock       = new System.Windows.Forms.Label();
            this.txtStock       = new System.Windows.Forms.TextBox();
            this.lblStockMin    = new System.Windows.Forms.Label();
            this.txtStockMin    = new System.Windows.Forms.TextBox();
            this.lblUnidad      = new System.Windows.Forms.Label();
            this.comboUnidad    = new System.Windows.Forms.ComboBox();
            this.btnGuardar     = new System.Windows.Forms.Button();
            this.btnCancelar    = new System.Windows.Forms.Button();
            this.lblError       = new System.Windows.Forms.Label();
            this.pnlAcciones    = new System.Windows.Forms.Panel();
            this.lblBuscar      = new System.Windows.Forms.Label();
            this.txtBuscar      = new System.Windows.Forms.TextBox();
            this.lblCantidad    = new System.Windows.Forms.Label();
            this.txtCantidad    = new System.Windows.Forms.TextBox();
            this.btnAgregar     = new System.Windows.Forms.Button();
            this.btnDescontar   = new System.Windows.Forms.Button();
            this.btnDesactivar  = new System.Windows.Forms.Button();
            this.btnEliminar    = new System.Windows.Forms.Button();
            this.btnLog         = new System.Windows.Forms.Button();
            this.pnlLog         = new System.Windows.Forms.Panel();
            this.splitterLog    = new System.Windows.Forms.Splitter();
            this.pnlLogFiltros  = new System.Windows.Forms.Panel();
            this.lblDesde       = new System.Windows.Forms.Label();
            this.dtpDesde       = new System.Windows.Forms.DateTimePicker();
            this.lblHasta       = new System.Windows.Forms.Label();
            this.dtpHasta       = new System.Windows.Forms.DateTimePicker();
            this.btnFiltrarLog  = new System.Windows.Forms.Button();
            this.dgvLog         = new System.Windows.Forms.DataGridView();
            this.colLogFecha    = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogUsuario  = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogAccion   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogDetalle  = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvProductos   = new System.Windows.Forms.DataGridView();
            this.colId          = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCodigo      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNombre      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategoria   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrecio      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescuento   = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStock       = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEstado      = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlFormulario.SuspendLayout();
            this.pnlAcciones.SuspendLayout();
            this.pnlLog.SuspendLayout();
            this.pnlLogFiltros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductos)).BeginInit();
            this.SuspendLayout();
            // pnlFormulario
            this.pnlFormulario.BackColor = System.Drawing.Color.White;
            this.pnlFormulario.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFormulario.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblModo, this.lblCodigo, this.txtCodigo, this.lblNombre, this.txtNombre,
                this.lblCategoria, this.comboCategoria, this.btnGestionarCat,
                this.lblPrecio, this.txtPrecio, this.lblStock, this.txtStock,
                this.lblStockMin, this.txtStockMin, this.lblUnidad, this.comboUnidad,
                this.btnGuardar, this.btnCancelar, this.lblError });
            this.pnlFormulario.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFormulario.Height = 210;
            this.pnlFormulario.Name = "pnlFormulario";
            this.pnlFormulario.Padding = new System.Windows.Forms.Padding(18);
            // lblModo
            this.lblModo.AutoSize = true;
            this.lblModo.Location = new System.Drawing.Point(18, 14);
            this.lblModo.Name = "lblModo";
            this.lblModo.Text = "Nuevo producto";
            // lblCodigo
            this.lblCodigo.AutoSize = true;
            this.lblCodigo.Location = new System.Drawing.Point(18, 52);
            this.lblCodigo.Name = "lblCodigo";
            this.lblCodigo.Text = "CÓDIGO DE BARRAS";
            // txtCodigo
            this.txtCodigo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCodigo.Location = new System.Drawing.Point(18, 74);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(200, 38);
            // lblNombre
            this.lblNombre.AutoSize = true;
            this.lblNombre.Location = new System.Drawing.Point(236, 52);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Text = "NOMBRE";
            // txtNombre
            this.txtNombre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNombre.Location = new System.Drawing.Point(236, 74);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(280, 38);
            // lblCategoria
            this.lblCategoria.AutoSize = true;
            this.lblCategoria.Location = new System.Drawing.Point(534, 52);
            this.lblCategoria.Name = "lblCategoria";
            this.lblCategoria.Text = "CATEGORÍA";
            // comboCategoria
            this.comboCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCategoria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboCategoria.Location = new System.Drawing.Point(534, 74);
            this.comboCategoria.Name = "comboCategoria";
            this.comboCategoria.Size = new System.Drawing.Size(210, 38);
            // btnGestionarCat
            this.btnGestionarCat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGestionarCat.Location = new System.Drawing.Point(752, 74);
            this.btnGestionarCat.Name = "btnGestionarCat";
            this.btnGestionarCat.Size = new System.Drawing.Size(120, 38);
            this.btnGestionarCat.Text = "Gestionar ▸";
            this.btnGestionarCat.UseVisualStyleBackColor = false;
            this.btnGestionarCat.Click += new System.EventHandler(this.btnGestionarCat_Click);
            // lblPrecio
            this.lblPrecio.AutoSize = true;
            this.lblPrecio.Location = new System.Drawing.Point(18, 130);
            this.lblPrecio.Name = "lblPrecio";
            this.lblPrecio.Text = "PRECIO";
            // txtPrecio
            this.txtPrecio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPrecio.Location = new System.Drawing.Point(18, 152);
            this.txtPrecio.Name = "txtPrecio";
            this.txtPrecio.Size = new System.Drawing.Size(140, 38);
            // lblStock
            this.lblStock.AutoSize = true;
            this.lblStock.Location = new System.Drawing.Point(176, 130);
            this.lblStock.Name = "lblStock";
            this.lblStock.Text = "STOCK";
            // txtStock
            this.txtStock.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStock.Location = new System.Drawing.Point(176, 152);
            this.txtStock.Name = "txtStock";
            this.txtStock.Size = new System.Drawing.Size(120, 38);
            // lblStockMin
            this.lblStockMin.AutoSize = true;
            this.lblStockMin.Location = new System.Drawing.Point(314, 130);
            this.lblStockMin.Name = "lblStockMin";
            this.lblStockMin.Text = "STOCK MÍNIMO";
            // txtStockMin
            this.txtStockMin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStockMin.Location = new System.Drawing.Point(314, 152);
            this.txtStockMin.Name = "txtStockMin";
            this.txtStockMin.Size = new System.Drawing.Size(120, 38);
            // lblUnidad
            this.lblUnidad.AutoSize = true;
            this.lblUnidad.Location = new System.Drawing.Point(452, 130);
            this.lblUnidad.Name = "lblUnidad";
            this.lblUnidad.Text = "UNIDAD";
            // comboUnidad
            this.comboUnidad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUnidad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboUnidad.Location = new System.Drawing.Point(452, 152);
            this.comboUnidad.Name = "comboUnidad";
            this.comboUnidad.Size = new System.Drawing.Size(110, 38);
            // btnGuardar
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Location = new System.Drawing.Point(580, 152);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(140, 38);
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // btnCancelar
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Location = new System.Drawing.Point(728, 152);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(110, 38);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Visible = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // lblError
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(18, 196);
            this.lblError.MaximumSize = new System.Drawing.Size(800, 0);
            this.lblError.Name = "lblError";
            this.lblError.Visible = false;
            // pnlAcciones
            this.pnlAcciones.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblBuscar, this.txtBuscar, this.lblCantidad, this.txtCantidad,
                this.btnAgregar, this.btnDescontar, this.btnDesactivar, this.btnEliminar, this.btnLog });
            this.pnlAcciones.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAcciones.Height = 68;
            this.pnlAcciones.Name = "pnlAcciones";
            this.pnlAcciones.Resize += new System.EventHandler(this.pnlAcciones_Resize);
            // lblBuscar
            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Location = new System.Drawing.Point(18, 18);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Text = "BUSCAR";
            // txtBuscar
            this.txtBuscar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBuscar.Location = new System.Drawing.Point(80, 14);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(220, 40);
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            // lblCantidad
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Location = new System.Drawing.Point(320, 18);
            this.lblCantidad.Name = "lblCantidad";
            this.lblCantidad.Text = "CANTIDAD";
            // txtCantidad
            this.txtCantidad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCantidad.Location = new System.Drawing.Point(394, 14);
            this.txtCantidad.Name = "txtCantidad";
            this.txtCantidad.Size = new System.Drawing.Size(80, 40);
            this.txtCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // btnAgregar
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Location = new System.Drawing.Point(490, 14);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(130, 40);
            this.btnAgregar.Text = "Agregar stock";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // btnDescontar
            this.btnDescontar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDescontar.Location = new System.Drawing.Point(628, 14);
            this.btnDescontar.Name = "btnDescontar";
            this.btnDescontar.Size = new System.Drawing.Size(140, 40);
            this.btnDescontar.Text = "Descontar stock";
            this.btnDescontar.UseVisualStyleBackColor = false;
            this.btnDescontar.Click += new System.EventHandler(this.btnDescontar_Click);
            // btnDesactivar
            this.btnDesactivar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesactivar.Location = new System.Drawing.Point(778, 14);
            this.btnDesactivar.Name = "btnDesactivar";
            this.btnDesactivar.Size = new System.Drawing.Size(110, 40);
            this.btnDesactivar.Text = "Desactivar";
            this.btnDesactivar.UseVisualStyleBackColor = false;
            this.btnDesactivar.Click += new System.EventHandler(this.btnDesactivar_Click);
            // btnEliminar
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.Location = new System.Drawing.Point(898, 14);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(100, 40);
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // btnLog
            this.btnLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLog.Location = new System.Drawing.Point(1010, 14);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(140, 40);
            this.btnLog.Text = "▼ Log inventario";
            this.btnLog.UseVisualStyleBackColor = false;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // pnlLog
            this.pnlLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLog.Controls.Add(this.pnlLogFiltros);
            this.pnlLog.Controls.Add(this.dgvLog);
            this.pnlLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLog.Height = 300;
            this.pnlLog.MinimumSize = new System.Drawing.Size(0, 120);
            this.pnlLog.Name = "pnlLog";
            this.pnlLog.Visible = false;
            // splitterLog
            this.splitterLog.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitterLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitterLog.Height = 5;
            this.splitterLog.Name = "splitterLog";
            this.splitterLog.Visible = false;
            // pnlLogFiltros
            this.pnlLogFiltros.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblDesde, this.dtpDesde, this.lblHasta, this.dtpHasta, this.btnFiltrarLog });
            this.pnlLogFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogFiltros.Height = 54;
            this.pnlLogFiltros.Name = "pnlLogFiltros";
            // lblDesde
            this.lblDesde.AutoSize = true;
            this.lblDesde.Location = new System.Drawing.Point(18, 16);
            this.lblDesde.Name = "lblDesde";
            this.lblDesde.Text = "Desde:";
            // dtpDesde
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesde.Location = new System.Drawing.Point(78, 12);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.Size = new System.Drawing.Size(148, 30);
            // lblHasta
            this.lblHasta.AutoSize = true;
            this.lblHasta.Location = new System.Drawing.Point(250, 16);
            this.lblHasta.Name = "lblHasta";
            this.lblHasta.Text = "Hasta:";
            // dtpHasta
            this.dtpHasta.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHasta.Location = new System.Drawing.Point(308, 12);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.Size = new System.Drawing.Size(148, 30);
            // btnFiltrarLog
            this.btnFiltrarLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFiltrarLog.Location = new System.Drawing.Point(476, 12);
            this.btnFiltrarLog.Name = "btnFiltrarLog";
            this.btnFiltrarLog.Size = new System.Drawing.Size(90, 30);
            this.btnFiltrarLog.Text = "Filtrar";
            this.btnFiltrarLog.UseVisualStyleBackColor = false;
            this.btnFiltrarLog.Click += new System.EventHandler(this.btnFiltrarLog_Click);
            // dgvLog
            this.dgvLog.AllowUserToAddRows = false;
            this.dgvLog.AllowUserToDeleteRows = false;
            this.dgvLog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colLogFecha, this.colLogUsuario, this.colLogAccion, this.colLogDetalle });
            this.dgvLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLog.EnableHeadersVisualStyles = false;
            this.dgvLog.MultiSelect = false;
            this.dgvLog.Name = "dgvLog";
            this.dgvLog.ReadOnly = true;
            this.dgvLog.RowHeadersVisible = false;
            this.dgvLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.colLogFecha.Name = "colLogFecha";     this.colLogFecha.HeaderText = "Fecha/Hora";  this.colLogFecha.FillWeight = 120;
            this.colLogUsuario.Name = "colLogUsuario";  this.colLogUsuario.HeaderText = "Usuario";     this.colLogUsuario.FillWeight = 100;
            this.colLogAccion.Name = "colLogAccion";    this.colLogAccion.HeaderText = "Acción";       this.colLogAccion.FillWeight = 110;
            this.colLogDetalle.Name = "colLogDetalle";  this.colLogDetalle.HeaderText = "Detalle";     this.colLogDetalle.FillWeight = 300;
            // dgvProductos
            this.dgvProductos.AllowUserToAddRows = false;
            this.dgvProductos.AllowUserToDeleteRows = false;
            this.dgvProductos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProductos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvProductos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvProductos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colId, this.colCodigo, this.colNombre, this.colCategoria,
                this.colPrecio, this.colDescuento, this.colStock, this.colEstado });
            this.dgvProductos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProductos.EnableHeadersVisualStyles = false;
            this.dgvProductos.MultiSelect = false;
            this.dgvProductos.Name = "dgvProductos";
            this.dgvProductos.ReadOnly = true;
            this.dgvProductos.RowHeadersVisible = false;
            this.dgvProductos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.colId.Name = "colId";             this.colId.Visible = false;
            this.colCodigo.Name = "colCodigo";     this.colCodigo.HeaderText = "Código";
            this.colNombre.Name = "colNombre";     this.colNombre.HeaderText = "Nombre";     this.colNombre.FillWeight = 220;
            this.colCategoria.Name = "colCategoria"; this.colCategoria.HeaderText = "Categoría";
            this.colPrecio.Name = "colPrecio";     this.colPrecio.HeaderText = "Precio";
            this.colDescuento.Name = "colDescuento"; this.colDescuento.HeaderText = "Desc.";   this.colDescuento.FillWeight = 60;
            this.colStock.Name = "colStock";       this.colStock.HeaderText = "Stock";
            this.colEstado.Name = "colEstado";     this.colEstado.HeaderText = "Estado";     this.colEstado.FillWeight = 80;
            this.dgvProductos.CellDoubleClick  += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProductos_CellDoubleClick);
            this.dgvProductos.SelectionChanged += new System.EventHandler(this.dgvProductos_SelectionChanged);
            // FormProductos
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.dgvProductos);
            this.Controls.Add(this.pnlLog);
            this.Controls.Add(this.splitterLog);
            this.Controls.Add(this.pnlAcciones);
            this.Controls.Add(this.pnlFormulario);
            this.Name = "FormProductos";
            this.Text = "Productos";
            this.Load     += new System.EventHandler(this.FormProductos_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormProductos_FormClosed);
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

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
        private ListBox  lstSugerencias;
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
        private DataGridViewTextBoxColumn colVCantidad;
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
            this.pnlEscaneo       = new Panel();
            this.lblCodigo        = new Label();  this.txtCodigo    = new TextBox();
            this.lblBuscarNombre  = new Label();  this.txtBuscar    = new TextBox();
            this.lstSugerencias   = new ListBox();
            this.lblCantidad      = new Label();  this.txtCantidad  = new TextBox();
            this.btnAgregar       = new Button();
            this.lblMensaje       = new Label();
            this.pnlCentro        = new Panel();
            this.pnlCats          = new FlowLayoutPanel();
            this.pnlGridWrap      = new Panel();
            this.pnlProdGrid      = new FlowLayoutPanel();
            this.pnlCarrito       = new Panel();
            this.lblCarritoTitulo = new Label();
            this.dgvCarrito       = new DataGridView();
            this.colVId           = new DataGridViewTextBoxColumn();
            this.colVNombre       = new DataGridViewTextBoxColumn();
            this.colVCantidad     = new DataGridViewTextBoxColumn();
            this.colVSubtotal     = new DataGridViewTextBoxColumn();
            this.colQuitar        = new DataGridViewTextBoxColumn();
            this.pnlCobro         = new Panel();
            this.lblTotalLabel    = new Label();
            this.lblTotal         = new Label();
            this.lblMedioPago     = new Label();
            this.comboMedioPago   = new ComboBox();
            this.btnCobrar        = new Button();
            this.btnCancelar      = new Button();
            this.btnVerLog        = new Button();
            this.pnlLogVentas     = new Panel();
            this.splitterLogV     = new Splitter();
            this.pnlLogFiltrosV   = new Panel();
            this.lblDesdeV        = new Label();  this.dtpDesdeV = new DateTimePicker();
            this.lblHastaV        = new Label();  this.dtpHastaV = new DateTimePicker();
            this.btnFiltrarLogV   = new Button();
            this.dgvLogVentas     = new DataGridView();
            this.colLVFecha       = new DataGridViewTextBoxColumn();
            this.colLVUsuario     = new DataGridViewTextBoxColumn();
            this.colLVAccion      = new DataGridViewTextBoxColumn();
            this.colLVDetalle     = new DataGridViewTextBoxColumn();

            this.pnlEscaneo.SuspendLayout();
            this.pnlCentro.SuspendLayout();
            this.pnlCarrito.SuspendLayout();
            this.pnlCobro.SuspendLayout();
            this.pnlLogVentas.SuspendLayout();
            this.pnlLogFiltrosV.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarrito)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogVentas)).BeginInit();
            this.SuspendLayout();

            // ── Panel escaneo (Dock Top) ──────────────────────────
            this.pnlEscaneo.BackColor = System.Drawing.Color.White;
            this.pnlEscaneo.BorderStyle = BorderStyle.FixedSingle;
            this.pnlEscaneo.Dock = DockStyle.Top;
            this.pnlEscaneo.Height = 100;
            this.pnlEscaneo.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblCodigo, txtCodigo, lblBuscarNombre, txtBuscar,
                lblCantidad, txtCantidad, btnAgregar, lblMensaje });

            // lblCodigo
            this.lblCodigo.AutoSize = true;
            this.lblCodigo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCodigo.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.lblCodigo.Location = new System.Drawing.Point(16, 10);
            this.lblCodigo.Name = "lblCodigo";
            this.lblCodigo.Text = "CÓDIGO DE BARRAS";
            // txtCodigo
            this.txtCodigo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCodigo.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.txtCodigo.BackColor = System.Drawing.Color.White;
            this.txtCodigo.ForeColor = System.Drawing.Color.FromArgb(14, 14, 18);
            this.txtCodigo.Location = new System.Drawing.Point(16, 32);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(290, 44);
            this.txtCodigo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodigo_KeyDown);
            // lblBuscarNombre
            this.lblBuscarNombre.AutoSize = true;
            this.lblBuscarNombre.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblBuscarNombre.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.lblBuscarNombre.Location = new System.Drawing.Point(322, 10);
            this.lblBuscarNombre.Name = "lblBuscarNombre";
            this.lblBuscarNombre.Text = "BUSCAR POR NOMBRE";
            // txtBuscar
            this.txtBuscar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBuscar.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.txtBuscar.BackColor = System.Drawing.Color.White;
            this.txtBuscar.ForeColor = System.Drawing.Color.FromArgb(14, 14, 18);
            this.txtBuscar.Location = new System.Drawing.Point(322, 32);
            this.txtBuscar.Name = "txtBuscar";
            this.txtBuscar.Size = new System.Drawing.Size(290, 44);
            this.txtBuscar.TextChanged += new System.EventHandler(this.txtBuscar_TextChanged);
            this.txtBuscar.KeyDown     += new System.Windows.Forms.KeyEventHandler(this.txtBuscar_KeyDown);
            // lblCantidad
            this.lblCantidad.AutoSize = true;
            this.lblCantidad.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCantidad.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.lblCantidad.Location = new System.Drawing.Point(628, 10);
            this.lblCantidad.Name = "lblCantidad";
            this.lblCantidad.Text = "CANTIDAD";
            // txtCantidad
            this.txtCantidad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCantidad.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.txtCantidad.BackColor = System.Drawing.Color.White;
            this.txtCantidad.ForeColor = System.Drawing.Color.FromArgb(14, 14, 18);
            this.txtCantidad.Location = new System.Drawing.Point(628, 32);
            this.txtCantidad.Name = "txtCantidad";
            this.txtCantidad.Size = new System.Drawing.Size(80, 44);
            this.txtCantidad.Text = "1";
            this.txtCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // btnAgregar
            this.btnAgregar.BackColor = System.Drawing.Color.White;
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(228, 228, 224);
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnAgregar.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.btnAgregar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAgregar.Location = new System.Drawing.Point(720, 32);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(130, 44);
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);

            this.lblMensaje.AutoSize   = false;
            this.lblMensaje.Font       = new System.Drawing.Font("Segoe UI", 10F);
            this.lblMensaje.ForeColor  = System.Drawing.Color.FromArgb(220, 38, 38);
            this.lblMensaje.Location   = new Point(16, 82);
            this.lblMensaje.Size       = new Size(900, 16);
            this.lblMensaje.Anchor     = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.lblMensaje.Visible    = false;

            // ── lstSugerencias (flotante, encima del grid) ────────
            this.lstSugerencias.BorderStyle = BorderStyle.FixedSingle;
            this.lstSugerencias.Font        = new System.Drawing.Font("Segoe UI", 12F);
            this.lstSugerencias.ItemHeight  = 28;
            this.lstSugerencias.Location    = new Point(322, 88);
            this.lstSugerencias.Size        = new Size(290, 224);
            this.lstSugerencias.Visible     = false;
            this.lstSugerencias.ForeColor   = System.Drawing.Color.FromArgb(14, 14, 18);
            this.lstSugerencias.Click       += this.lstSugerencias_Click;
            this.lstSugerencias.KeyDown     += this.lstSugerencias_KeyDown;
            this.lstSugerencias.DoubleClick += this.lstSugerencias_DoubleClick;

            // ── Panel central (Fill) ──────────────────────────────
            this.pnlCentro.Dock = DockStyle.Fill;
            this.pnlCentro.Controls.Add(this.pnlGridWrap);
            this.pnlCentro.Controls.Add(this.pnlCarrito);

            // ── Panel carrito (Dock Right) ────────────────────────
            this.pnlCarrito.BackColor   = System.Drawing.Color.White;
            this.pnlCarrito.BorderStyle = BorderStyle.FixedSingle;
            this.pnlCarrito.Dock        = DockStyle.Right;
            this.pnlCarrito.Width       = 360;
            this.pnlCarrito.Controls.Add(this.lblCarritoTitulo);
            this.pnlCarrito.Controls.Add(this.dgvCarrito);
            this.pnlCarrito.Controls.Add(this.pnlCobro);

            this.lblCarritoTitulo.AutoSize  = false;
            this.lblCarritoTitulo.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblCarritoTitulo.ForeColor = System.Drawing.Color.FromArgb(14, 14, 18);
            this.lblCarritoTitulo.Dock      = DockStyle.Top;
            this.lblCarritoTitulo.Height    = 44;
            this.lblCarritoTitulo.Text      = "Carrito";
            this.lblCarritoTitulo.TextAlign = ContentAlignment.MiddleLeft;
            this.lblCarritoTitulo.Padding   = new Padding(14, 0, 0, 0);

            // Grid carrito
            this.dgvCarrito.Dock = DockStyle.Fill;
            this.dgvCarrito.RowTemplate.Height = 36;
            this.dgvCarrito.Columns.AddRange(new DataGridViewColumn[] {
                colVId, colVNombre, colVCantidad, colVSubtotal, colQuitar });
            this.colVId.Name       = "colVId";       this.colVId.Visible        = false;
            this.colVNombre.Name   = "colVNombre";   this.colVNombre.HeaderText   = "Producto";  this.colVNombre.FillWeight  = 200;
            this.colVCantidad.Name = "colVCantidad"; this.colVCantidad.HeaderText = "Cant.";     this.colVCantidad.FillWeight = 60;
            this.colVSubtotal.Name = "colVSubtotal"; this.colVSubtotal.HeaderText = "Subtotal";  this.colVSubtotal.FillWeight = 100;
            this.colQuitar.Name    = "colQuitar";    this.colQuitar.HeaderText    = "";           this.colQuitar.FillWeight    = 36;
            this.dgvCarrito.CellClick += this.dgvCarrito_CellClick;

            // ── Panel cobro (Dock Bottom del carrito) ─────────────
            this.pnlCobro.BackColor = System.Drawing.Color.White;
            this.pnlCobro.Dock      = DockStyle.Bottom;
            this.pnlCobro.Height    = 220;
            this.pnlCobro.Padding   = new Padding(14, 10, 14, 10);
            this.pnlCobro.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblTotalLabel, lblTotal, lblMedioPago, comboMedioPago, btnCobrar, btnCancelar, btnVerLog });

            this.lblTotalLabel.AutoSize  = true;
            this.lblTotalLabel.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTotalLabel.ForeColor = System.Drawing.Color.FromArgb(143, 143, 154);
            this.lblTotalLabel.Location  = new Point(14, 10);
            this.lblTotalLabel.Text      = "TOTAL A PAGAR";

            this.lblTotal.AutoSize  = true;
            this.lblTotal.Font      = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(14, 14, 18);
            this.lblTotal.Location  = new Point(10, 26);
            this.lblTotal.Text      = "$0";

            this.lblMedioPago.AutoSize  = true;
            this.lblMedioPago.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblMedioPago.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.lblMedioPago.Location  = new Point(14, 98);
            this.lblMedioPago.Text      = "Medio de pago";
            this.comboMedioPago.Location = new Point(14, 118);
            this.comboMedioPago.Size     = new Size(266, 38);
            this.btnCobrar.Location = new Point(14, 160);
            this.btnCobrar.Size     = new Size(266, 50);
            this.btnCobrar.Text     = "Cobrar — F12";
            this.btnCobrar.Click   += this.btnCobrar_Click;
            this.btnCancelar.Location = new Point(14, 216);
            this.btnCancelar.Size     = new Size(126, 38);
            this.btnCancelar.Text     = "Cancelar";
            this.btnCancelar.Click   += this.btnCancelar_Click;
            this.btnVerLog.Location = new Point(146, 216);
            this.btnVerLog.Size     = new Size(134, 38);
            this.btnVerLog.Text     = "▼ Registro";
            this.btnVerLog.Click   += this.btnVerLog_Click;

            // ── Grid wrap: categorías + grid de productos ─────────
            this.pnlGridWrap.Dock        = DockStyle.Fill;
            this.pnlGridWrap.BackColor   = System.Drawing.Color.FromArgb(245, 244, 241);
            this.pnlGridWrap.Padding     = new Padding(10);
            this.pnlGridWrap.Controls.Add(this.pnlProdGrid);
            this.pnlGridWrap.Controls.Add(this.pnlCats);

            // Pills categorías
            this.pnlCats.BackColor   = System.Drawing.Color.FromArgb(245, 244, 241);
            this.pnlCats.Dock        = DockStyle.Top;
            this.pnlCats.Height      = 44;
            this.pnlCats.FlowDirection = FlowDirection.LeftToRight;
            this.pnlCats.WrapContents  = false;
            this.pnlCats.Padding       = new Padding(0, 6, 0, 6);

            // Grid de productos (FlowLayout, auto-wrap)
            this.pnlProdGrid.Dock          = DockStyle.Fill;
            this.pnlProdGrid.BackColor     = System.Drawing.Color.FromArgb(245, 244, 241);
            this.pnlProdGrid.FlowDirection = FlowDirection.LeftToRight;
            this.pnlProdGrid.WrapContents  = true;
            this.pnlProdGrid.AutoScroll    = true;
            this.pnlProdGrid.Padding       = new System.Windows.Forms.Padding(0);
            this.pnlProdGrid.Resize       += new System.EventHandler(this.pnlProdGrid_Resize);

            // ── Log (Dock Bottom) ─────────────────────────────────
            this.pnlLogVentas.BackColor   = System.Drawing.Color.White;
            this.pnlLogVentas.BorderStyle = BorderStyle.FixedSingle;
            this.pnlLogVentas.Dock        = DockStyle.Bottom;
            this.pnlLogVentas.Height      = 280;
            this.pnlLogVentas.MinimumSize = new System.Drawing.Size(0, 120);
            this.pnlLogVentas.Visible     = false;
            this.pnlLogVentas.Controls.Add(this.pnlLogFiltrosV);
            this.pnlLogVentas.Controls.Add(this.dgvLogVentas);

            this.splitterLogV.Dock      = DockStyle.Bottom;
            this.splitterLogV.Height    = 5;
            this.splitterLogV.BackColor = System.Drawing.Color.FromArgb(228, 228, 224);
            this.splitterLogV.Cursor    = Cursors.HSplit;
            this.splitterLogV.Visible   = false;

            this.pnlLogFiltrosV.BackColor = System.Drawing.Color.White;
            this.pnlLogFiltrosV.Dock      = DockStyle.Top;
            this.pnlLogFiltrosV.Height    = 50;
            this.pnlLogFiltrosV.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblDesdeV, dtpDesdeV, lblHastaV, dtpHastaV, btnFiltrarLogV });

            // lblDesdeV
            this.lblDesdeV.AutoSize = true;
            this.lblDesdeV.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDesdeV.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.lblDesdeV.Location = new System.Drawing.Point(16, 15);
            this.lblDesdeV.Name = "lblDesdeV";
            this.lblDesdeV.Text = "Desde:";
            // dtpDesdeV
            this.dtpDesdeV.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDesdeV.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.dtpDesdeV.Location = new System.Drawing.Point(74, 11);
            this.dtpDesdeV.Name = "dtpDesdeV";
            this.dtpDesdeV.Size = new System.Drawing.Size(148, 30);
            // lblHastaV
            this.lblHastaV.AutoSize = true;
            this.lblHastaV.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblHastaV.ForeColor = System.Drawing.Color.FromArgb(69, 69, 79);
            this.lblHastaV.Location = new System.Drawing.Point(240, 15);
            this.lblHastaV.Name = "lblHastaV";
            this.lblHastaV.Text = "Hasta:";
            // dtpHastaV
            this.dtpHastaV.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpHastaV.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.dtpHastaV.Location = new System.Drawing.Point(296, 11);
            this.dtpHastaV.Name = "dtpHastaV";
            this.dtpHastaV.Size = new System.Drawing.Size(148, 30);
            this.btnFiltrarLogV.Location = new Point(456, 10);
            this.btnFiltrarLogV.Size     = new Size(90, 30);
            this.btnFiltrarLogV.Text     = "Filtrar";
            this.btnFiltrarLogV.Click   += this.btnFiltrarLogV_Click;
            this.dgvLogVentas.Dock = DockStyle.Fill;
            this.dgvLogVentas.Columns.AddRange(new DataGridViewColumn[] {
                colLVFecha, colLVUsuario, colLVAccion, colLVDetalle });
            this.colLVFecha.Name   = "colLVFecha";   this.colLVFecha.HeaderText   = "Fecha";    this.colLVFecha.FillWeight   = 110;
            this.colLVUsuario.Name = "colLVUsuario"; this.colLVUsuario.HeaderText = "Usuario";  this.colLVUsuario.FillWeight = 100;
            this.colLVAccion.Name  = "colLVAccion";  this.colLVAccion.HeaderText  = "Acción";   this.colLVAccion.FillWeight  = 80;
            this.colLVDetalle.Name = "colLVDetalle"; this.colLVDetalle.HeaderText = "Detalle";  this.colLVDetalle.FillWeight = 300;

            // ── FormVentas ────────────────────────────────────────
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor     = System.Drawing.Color.FromArgb(245, 244, 241);
            this.Controls.Add(this.lstSugerencias);
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
            this.pnlCarrito.ResumeLayout(false);
            this.pnlCobro.ResumeLayout(false);
            this.pnlCobro.PerformLayout();
            this.pnlLogVentas.ResumeLayout(false);
            this.pnlLogFiltrosV.ResumeLayout(false);
            this.pnlLogFiltrosV.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCarrito)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLogVentas)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

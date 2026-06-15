using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormVentas : Form
    {
        private readonly VentaService    ventaService    = new VentaService();
        private readonly ProductoService productoService = new ProductoService();
        private readonly CategoriaService categoriaService = new CategoriaService();
        private readonly LogService      logService      = new LogService();
        private bool logVentasVisible = false;
        private List<Producto> sugerencias = new List<Producto>();
        private string categoriaActual = "";
        private FlowLayoutPanel pnlTabs;
        private Timer timerVentas;
        private Label lblDesgloseIva;
        private Button btnDescuento;
        private List<Venta> ventasRegistro = new List<Venta>();
        private static readonly TimeSpan InactividadMax = TimeSpan.FromMinutes(10);

        public FormVentas() { InitializeComponent(); }

        private void FormVentas_Load(object sender, EventArgs e)
        {
            // Aplicar estilos en runtime (garantiza que corre después del layout)
            AplicarEstilos();
            AcomodarCobro();

            comboMedioPago.Items.AddRange(new object[] {
                MedioPago.Efectivo, MedioPago.Tarjeta, MedioPago.Transferencia });
            comboMedioPago.SelectedIndex = 0;
            dtpDesdeV.Value = DateTime.Today;
            dtpHastaV.Value = DateTime.Today;

            CargarCategorias();
            CargarGridProductos();
            InicializarPestanas();
            ventaService.CerrarPausadasInactivas(InactividadMax);   // limpia las que vencieron en otro módulo
            RefrescarCarrito();
            txtCodigo.Focus();
        }

        private void AplicarEstilos()
        {
            AplicarEstilosGrids();

            // Combo medio de pago
            EstiloPos.AplicarCombo(comboMedioPago);

            // Botones del panel de cobro
            EstiloPos.AplicarBotonPrimario(btnCobrar, grande: true);
            EstiloPos.AplicarBotonSecundario(btnCancelar, EstiloPos.Rojo);
            EstiloPos.AplicarBotonSecundario(btnVerLog);

            // Botón filtrar del registro de ventas
            EstiloPos.AplicarBotonSecundario(btnFiltrarLogV);
            btnFiltrarLogV.Size = new Size(90, 30);

            // btnAgregar viene estilizado desde el Designer; le añadimos el feedback hover
            btnAgregar.Cursor = Cursors.Hand;
            btnAgregar.FlatAppearance.MouseOverBackColor = EstiloPos.Hover;

            // Desglose de IVA bajo el total (los precios ya incluyen IVA)
            lblDesgloseIva = new Label
            {
                AutoSize  = false,
                Location  = new Point(18, 74),
                Size      = new Size(322, 20),
                Font      = EstiloPos.FontSmall,
                ForeColor = EstiloPos.Ink3
            };
            pnlCobro.Controls.Add(lblDesgloseIva);
            lblDesgloseIva.BringToFront();

            // Botón para aplicar un descuento al total de la venta
            btnDescuento = new Button
            {
                Text = "Descuento", AutoSize = false, Size = new Size(112, 28),
                Font = EstiloPos.FontSmall, FlatStyle = FlatStyle.Flat,
                BackColor = EstiloPos.Surface, ForeColor = EstiloPos.Ink2,
                Cursor = Cursors.Hand, UseVisualStyleBackColor = false
            };
            btnDescuento.FlatAppearance.BorderColor = EstiloPos.Border;
            btnDescuento.FlatAppearance.MouseOverBackColor = EstiloPos.Hover;
            btnDescuento.Click += BtnDescuento_Click;
            pnlCobro.Controls.Add(btnDescuento);
            btnDescuento.BringToFront();

            pnlCobro.Resize += (s, e) => AcomodarCobro();   // reajusta el ancho de los controles del cobro
        }

        private void BtnDescuento_Click(object sender, EventArgs e)
        {
            if (ventaService.Carrito.Count == 0)
            { MostrarMensaje("Agrega productos antes de aplicar un descuento."); return; }

            string actual = ventaService.Descuento > 0 ? ventaService.Descuento.ToString("0") : "";
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Descuento en $ sobre el subtotal ($" + ventaService.Subtotal.ToString("N0") + "):",
                "Aplicar descuento", actual);
            if (string.IsNullOrEmpty(input)) return;
            if (!decimal.TryParse(input, out decimal monto) || monto < 0)
            { MostrarMensaje("Descuento inválido."); return; }

            ventaService.AplicarDescuento(monto);
            RefrescarCarrito();
        }

        private void ConfigColAccion(DataGridViewColumn col, int ancho, float fontSize, Color fg, Color sel)
        {
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            col.Width        = ancho;
            col.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment          = DataGridViewContentAlignment.MiddleCenter,
                Font               = new Font("Segoe UI", fontSize, FontStyle.Bold),
                ForeColor          = fg,
                SelectionForeColor = fg,
                Padding            = new Padding(0),
                BackColor          = EstiloPos.Surface,
                SelectionBackColor = sel
            };
        }

        // Reensancha los controles del panel de cobro al ancho real del carrito
        private void AcomodarCobro()
        {
            const int margen = 18;
            int util = pnlCobro.ClientSize.Width - margen * 2;
            if (util < 120) return;

            if (btnDescuento != null)
                btnDescuento.Location = new Point(pnlCobro.ClientSize.Width - btnDescuento.Width - margen, 12);
            if (lblDesgloseIva != null) lblDesgloseIva.Width = util;
            comboMedioPago.Width = util;
            btnCobrar.Width      = util;

            int gap = 14;
            int mitad = (util - gap) / 2;
            btnCancelar.Location = new Point(margen, btnCancelar.Top);
            btnCancelar.Width    = mitad;
            btnVerLog.Location   = new Point(margen + mitad + gap, btnVerLog.Top);
            btnVerLog.Width      = mitad;
        }

        private void AplicarEstilosGrids()
        {
            // dgvCarrito
            EstiloPos.AplicarGrid(dgvCarrito);
            dgvCarrito.RowTemplate.Height = 38;
            dgvCarrito.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 228, 245);
            dgvCarrito.DefaultCellStyle.SelectionForeColor = EstiloPos.Ink1;

            // Columnas de acción del carrito: los símbolos − + ✕ se DIBUJAN (CellPainting),
            // así nunca se truncan. Solo "Producto" usa el espacio flexible (Fill).
            Color colSel = EstiloPos.Seleccion;
            ConfigColAccion(colMenos,  36, 14F, EstiloPos.Ink1,  colSel);
            ConfigColAccion(colMas,    36, 16F, EstiloPos.Verde, colSel);
            ConfigColAccion(colQuitar, 36, 13F, EstiloPos.Rojo,  colSel);

            colVCantidad.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colVCantidad.Width        = 54;
            colVCantidad.DefaultCellStyle = new DataGridViewCellStyle {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = EstiloPos.FontTabla,
                ForeColor = EstiloPos.Ink1, SelectionForeColor = EstiloPos.Ink1,
                BackColor = EstiloPos.Surface, SelectionBackColor = colSel };

            colVSubtotal.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colVSubtotal.Width        = 82;
            colVSubtotal.DefaultCellStyle = new DataGridViewCellStyle {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Font = EstiloPos.FontTabla,
                ForeColor = EstiloPos.Ink1, SelectionForeColor = EstiloPos.Ink1,
                Padding = new Padding(0, 0, 8, 0),
                BackColor = EstiloPos.Surface, SelectionBackColor = colSel };

            // Encabezados del carrito: solo Producto / Cant. / Subtotal llevan título,
            // alineados con su contenido. Las columnas de acción (− + ✕) quedan sin texto.
            colVNombre.HeaderCell.Style.Padding     = new Padding(12, 0, 0, 0);
            colVCantidad.HeaderText                 = "Cant.";
            colVCantidad.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colVCantidad.HeaderCell.Style.Padding   = new Padding(0);
            colVSubtotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colVSubtotal.HeaderCell.Style.Padding   = new Padding(0, 0, 8, 0);

            // Da un poco más de aire a la izquierda en el nombre del producto
            colVNombre.DefaultCellStyle = new DataGridViewCellStyle {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Font = EstiloPos.FontTabla,
                ForeColor = EstiloPos.Ink1, SelectionForeColor = EstiloPos.Ink1,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = EstiloPos.Surface, SelectionBackColor = colSel };

            // Cursor de mano sobre las columnas de acción para indicar que son clickeables
            dgvCarrito.CellMouseEnter += DgvCarrito_CellMouseEnter;
            dgvCarrito.CellPainting   += DgvCarrito_CellPainting;

            // dgvLogVentas: registro de ventas del período (doble clic para ver detalle por código)
            EstiloPos.AplicarGrid(dgvLogVentas);
            dgvLogVentas.Columns.Clear();
            dgvLogVentas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRegId", Visible = false });
            dgvLogVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "N°",            FillWeight = 30 });
            dgvLogVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha",         FillWeight = 70 });
            dgvLogVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Medio de pago", FillWeight = 65 });
            dgvLogVentas.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total",         FillWeight = 55 });
            dgvLogVentas.CellDoubleClick += DgvLogVentas_CellDoubleClick;
        }

        // ── Categorías (pills) ────────────────────────────────────────

        private void CargarCategorias()
        {
            pnlCats.Controls.Clear();
            AgregarPill("Todos", "");
            // Usa la tabla Categoria — así los nuevos agregados desde Productos aparecen aquí también
            foreach (var c in categoriaService.ObtenerTodas())
                AgregarPill(c.Nombre, c.Nombre);
            ActivarPill("Todos");
        }

        private void AgregarPill(string texto, string valor)
        {
            var btn = new Button
            {
                Text      = texto,
                Tag       = valor,
                FlatStyle = FlatStyle.Flat,
                Font      = EstiloPos.FontSmall,
                BackColor = EstiloPos.Surface,
                ForeColor = EstiloPos.Ink3,
                Cursor    = Cursors.Hand,
                AutoSize  = true,
                Padding   = new Padding(14, 0, 14, 0),
                Height    = 30,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderColor = EstiloPos.Border;
            btn.FlatAppearance.BorderSize  = 1;
            btn.FlatAppearance.MouseOverBackColor = EstiloPos.Hover;
            btn.Click += (s, e) => {
                categoriaActual = valor;
                ActivarPill(texto);
                CargarGridProductos();
            };
            pnlCats.Controls.Add(btn);
        }

        private void ActivarPill(string texto)
        {
            foreach (Control c in pnlCats.Controls)
            {
                if (c is Button b)
                {
                    bool activo = b.Text == texto;
                    b.BackColor = activo ? EstiloPos.Ink1 : EstiloPos.Surface;
                    b.ForeColor = activo ? Color.White    : EstiloPos.Ink3;
                    b.FlatAppearance.BorderColor = activo ? EstiloPos.Ink1 : EstiloPos.Border;
                }
            }
        }

        // ── Grid de productos (tarjetas clickeables) ───────────────────

        private void CargarGridProductos(string busqueda = "")
        {
            pnlProdGrid.Controls.Clear();
            pnlProdGrid.SuspendLayout();

            List<Producto> productos;
            if (!string.IsNullOrEmpty(busqueda))
                productos = productoService.Buscar(busqueda).FindAll(p => p.Activo);
            else if (!string.IsNullOrEmpty(categoriaActual))
                productos = productoService.ObtenerPorCategoria(categoriaActual);
            else
                productos = productoService.ObtenerActivos();

            foreach (var p in productos)
                pnlProdGrid.Controls.Add(CrearTileProducto(p));

            pnlProdGrid.ResumeLayout();

            // Forzar cálculo de tamaño DESPUÉS de que WinForms termine el layout pass
            pnlProdGrid.BeginInvoke(new Action(AjustarTamañoTiles));
        }

        private void pnlProdGrid_Resize(object sender, EventArgs e)
        {
            AjustarTamañoTiles();
        }

        private void AjustarTamañoTiles()
        {
            if (pnlProdGrid.Controls.Count == 0) return;
            int cols  = 4;
            int gap   = 8;
            int ancho = (pnlProdGrid.ClientSize.Width - (gap * cols)) / cols;
            if (ancho < 100) return;
            pnlProdGrid.SuspendLayout();
            foreach (Control c in pnlProdGrid.Controls)
                c.Size = new Size(ancho, 100);
            pnlProdGrid.ResumeLayout();
        }

        private Panel CrearTileProducto(Producto p)
        {
            bool bajoStock = productoService.EstaBajoStock(p);

            var tile = new Panel
            {
                Tag       = p,
                BackColor = bajoStock ? Color.FromArgb(255, 253, 245) : EstiloPos.Surface,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 8, 8),
                Padding   = new Padding(10, 8, 10, 8),
                Size      = new Size(220, 100)  // tamaño inicial — AjustarTamañoTiles lo corrige
            };

            // Borde pintado; cambia a azul resaltado cuando el mouse está encima
            bool hover = false;
            tile.Paint += (s, pe) =>
            {
                Color colorBorde = hover ? EstiloPos.Azul : (bajoStock ? EstiloPos.Amber : EstiloPos.Border);
                using (var pen = new System.Drawing.Pen(colorBorde, hover ? 2.2f : 1.5f))
                    pe.Graphics.DrawRectangle(pen, 0, 0, tile.Width - 1, tile.Height - 1);
            };

            var lblNombre = new Label
            {
                Text      = p.Nombre,
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = EstiloPos.Ink1,
                AutoSize  = false,
                Location  = new Point(10, 8),
                Size      = new Size(tile.Width - 20, 38),
                TextAlign = ContentAlignment.TopLeft
            };
            var lblPrecio = new Label
            {
                Text      = "$" + p.Precio.ToString("N0"),
                Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = EstiloPos.Ink1,
                AutoSize  = true,
                Location  = new Point(10, 48)
            };
            var lblStock = new Label
            {
                Text      = bajoStock ? "⚠ " + p.Stock.ToString("0.##") + " " + p.UnidadMedida
                                      : p.Stock.ToString("0.##") + " " + p.UnidadMedida,
                Font      = new Font("Segoe UI", 10F),
                ForeColor = bajoStock ? EstiloPos.Amber : EstiloPos.Ink3,
                AutoSize  = true,
                Location  = new Point(10, 74)
            };

            tile.Controls.AddRange(new System.Windows.Forms.Control[] { lblNombre, lblPrecio, lblStock });

            // Click => agregar al carrito
            EventHandler clickHandler = (s, e) => AgregarProductoAlCarrito(p);
            tile.Click       += clickHandler;
            lblNombre.Click  += clickHandler;
            lblPrecio.Click  += clickHandler;
            lblStock.Click   += clickHandler;

            // Feedback hover: fondo azul claro + borde azul (los hijos también disparan el evento)
            Color fondoNormal = tile.BackColor;
            Color fondoHover  = bajoStock ? Color.FromArgb(255, 246, 228) : EstiloPos.HoverFila;
            EventHandler entrar = (s, e) => { hover = true;  tile.BackColor = fondoHover;  tile.Invalidate(); };
            EventHandler salir  = (s, e) =>
            {
                if (!tile.ClientRectangle.Contains(tile.PointToClient(Cursor.Position)))
                { hover = false; tile.BackColor = fondoNormal; tile.Invalidate(); }
            };
            foreach (Control c in new Control[] { tile, lblNombre, lblPrecio, lblStock })
            {
                c.MouseEnter += entrar;
                c.MouseLeave += salir;
            }

            return tile;
        }

        private void AgregarProductoAlCarrito(Producto p)
        {
            if (!decimal.TryParse(txtCantidad.Text, out decimal cant) || cant <= 0) cant = 1;
            try
            {
                ventaService.AgregarPorId(p.IdProducto, cant);
                RefrescarCarrito();
                txtCantidad.Text = "1";
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
        }

        // ── Scanner / Código de barras ─────────────────────────────────

        private void txtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AgregarPorCodigo();
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e) => AgregarPorCodigo();

        private void AgregarPorCodigo()
        {
            string codigo = txtCodigo.Text.Trim();
            if (string.IsNullOrEmpty(codigo)) return;
            if (!decimal.TryParse(txtCantidad.Text, out decimal cant) || cant <= 0) cant = 1;
            try
            {
                ventaService.AgregarPorCodigo(codigo, cant);
                RefrescarCarrito();
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
            txtCodigo.Clear();
            txtCantidad.Text = "1";
            txtCodigo.Focus();
        }

        // ── Buscador por nombre ────────────────────────────────────────

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            string texto = txtBuscar.Text.Trim();
            if (texto.Length == 0)
            {
                lstSugerencias.Visible = false;
                CargarGridProductos();
                return;
            }
            sugerencias = ventaService.BuscarProductos(texto);
            lstSugerencias.Items.Clear();
            foreach (var p in sugerencias)
                lstSugerencias.Items.Add(p.Nombre + "   $" + p.Precio.ToString("N0")
                    + "   stock: " + p.Stock.ToString("0.##") + " " + p.UnidadMedida);
            lstSugerencias.Visible = sugerencias.Count > 0;
            if (sugerencias.Count > 0) lstSugerencias.SelectedIndex = 0;
            CargarGridProductos(texto);
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstSugerencias.Visible)
            { lstSugerencias.Focus(); lstSugerencias.SelectedIndex = 0; e.SuppressKeyPress = true; }
        }

        private void lstSugerencias_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { PedirCantidadYAgregar(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) { lstSugerencias.Visible = false; txtBuscar.Focus(); }
        }
        private void lstSugerencias_Click(object sender, EventArgs e) { }
        private void lstSugerencias_DoubleClick(object sender, EventArgs e) => PedirCantidadYAgregar();

        private void PedirCantidadYAgregar()
        {
            int i = lstSugerencias.SelectedIndex;
            if (i < 0 || i >= sugerencias.Count) return;
            decimal cantidad = LeerCantidad();
            if (txtCantidad.Text.Trim() == "" || txtCantidad.Text.Trim() == "1")
            {
                string input = Interaction.InputBox(
                    "¿Cuántas unidades de \"" + sugerencias[i].Nombre + "\"?", "Cantidad", "1");
                if (string.IsNullOrEmpty(input)) return;
                if (!decimal.TryParse(input, out cantidad) || cantidad <= 0)
                { MostrarMensaje("Cantidad inválida."); return; }
            }
            try
            {
                ventaService.AgregarPorId(sugerencias[i].IdProducto, cantidad);
                RefrescarCarrito();
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
            txtBuscar.Clear();
            lstSugerencias.Visible = false;
            txtCantidad.Text = "1";
            txtCodigo.Focus();
        }

        // ── Pestañas de ventas (varias ventas en paralelo) ─────────────

        private void InicializarPestanas()
        {
            lblCarritoTitulo.Visible = false;   // la barra de pestañas reemplaza el título

            // Pestañas de ventas dentro de una barra navegable con flechas
            pnlTabs = new FlowLayoutPanel
            {
                BackColor     = EstiloPos.Surface,
                Padding       = new Padding(6, 11, 6, 0),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };
            var hostTabs = CrearBarraNavegable(pnlTabs, 52);
            hostTabs.Dock = DockStyle.Top;
            pnlCarrito.Controls.Add(hostTabs);
            dgvCarrito.BringToFront();   // el carrito (Fill) reclama el espacio entre pestañas y cobro

            // Categorías dentro de una barra navegable con flechas
            pnlGridWrap.Controls.Remove(pnlCats);
            pnlCats.WrapContents = false;
            pnlCats.Padding      = new Padding(0, 8, 0, 0);
            var hostCats = CrearBarraNavegable(pnlCats, 46);
            hostCats.Dock = DockStyle.Top;
            pnlGridWrap.Controls.Add(hostCats);
            pnlProdGrid.BringToFront();

            timerVentas = new Timer { Interval = 30000 };   // revisa inactividad cada 30 s
            timerVentas.Tick += TimerVentas_Tick;
            timerVentas.Start();
        }

        // Envuelve un FlowLayoutPanel en una barra con flechas ◄ ► que desplazan el contenido.
        // Las flechas solo aparecen cuando los elementos no caben en el ancho visible.
        private Panel CrearBarraNavegable(FlowLayoutPanel flow, int altura)
        {
            var host = new Panel { Height = altura, BackColor = flow.BackColor };

            // El panel "clip" ocupa todo el host y recorta el contenido; las flechas flotan encima.
            var clip = new Panel { Dock = DockStyle.Fill, BackColor = flow.BackColor };
            flow.Dock     = DockStyle.None;
            flow.Location = new Point(0, 0);
            flow.Height   = altura;
            flow.Width    = 6000;   // ancho amplio: los items van en una sola fila; el clip los recorta
            clip.Controls.Add(flow);
            host.Controls.Add(clip);

            var btnIzq = FlechaNav(false);
            var btnDer = FlechaNav(true);
            btnIzq.Height = altura; btnIzq.Location = new Point(0, 0);
            btnIzq.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            btnDer.Height = altura;
            btnDer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            host.Controls.Add(btnIzq);
            host.Controls.Add(btnDer);

            void Actualizar()
            {
                int ancho   = AnchoContenido(flow);
                int visible = host.ClientSize.Width;
                int min     = Math.Min(0, visible - ancho);
                if (flow.Left < min) flow.Left = min;
                if (flow.Left > 0)   flow.Left = 0;

                btnDer.Left = visible - btnDer.Width;
                // Cada flecha aparece solo si hay contenido en esa dirección
                btnIzq.Visible = flow.Left < 0;
                btnDer.Visible = flow.Left > min;
                if (btnIzq.Visible) btnIzq.BringToFront();
                if (btnDer.Visible) btnDer.BringToFront();
            }

            btnIzq.Click += (s, e) => { flow.Left = Math.Min(0, flow.Left + 150); Actualizar(); };
            btnDer.Click += (s, e) =>
            {
                int min = Math.Min(0, host.ClientSize.Width - AnchoContenido(flow));
                flow.Left = Math.Max(min, flow.Left - 150);
                Actualizar();
            };
            flow.ControlAdded   += (s, e) => Actualizar();
            flow.ControlRemoved += (s, e) => BeginInvoke(new Action(Actualizar));
            host.SizeChanged    += (s, e) => Actualizar();

            return host;
        }

        private static int AnchoContenido(FlowLayoutPanel flow)
        {
            int ancho = 0;
            foreach (Control c in flow.Controls)
                ancho = Math.Max(ancho, c.Right + c.Margin.Right);
            return ancho + flow.Padding.Right;
        }

        private Button FlechaNav(bool derecha)
        {
            var b = new Button
            {
                Text = "", Width = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = EstiloPos.Surface,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Visible = false, TabStop = false
            };
            b.FlatAppearance.BorderColor = EstiloPos.Border;
            b.FlatAppearance.MouseOverBackColor = EstiloPos.Hover;
            b.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int cx = b.Width / 2, cy = b.Height / 2, r = 5;
                using (var br = new SolidBrush(EstiloPos.Ink2))
                {
                    Point[] tri = derecha
                        ? new[] { new Point(cx - r, cy - r - 1), new Point(cx + r, cy), new Point(cx - r, cy + r + 1) }
                        : new[] { new Point(cx + r, cy - r - 1), new Point(cx - r, cy), new Point(cx + r, cy + r + 1) };
                    e.Graphics.FillPolygon(br, tri);
                }
            };
            return b;
        }

        private void TimerVentas_Tick(object sender, EventArgs e)
        {
            if (ventaService.CerrarPausadasInactivas(InactividadMax) > 0)
                RefrescarCarrito();   // refresca carrito + pestañas
        }

        private void RefrescarTabs()
        {
            if (pnlTabs == null) return;
            pnlTabs.SuspendLayout();
            pnlTabs.Controls.Clear();

            var ventas = ventaService.VentasEnCurso;
            int activaId = ventaService.Activa.Id;
            for (int i = 0; i < ventas.Count; i++)
                pnlTabs.Controls.Add(CrearTab(ventas[i], i + 1, ventas[i].Id == activaId));

            var btnMas = new Button
            {
                Text      = "",
                Width     = 38,
                Height    = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = EstiloPos.Surface,
                Cursor    = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin    = new Padding(0)
            };
            btnMas.FlatAppearance.BorderColor = EstiloPos.Border;
            btnMas.FlatAppearance.MouseOverBackColor = EstiloPos.VerdeBg;
            // El "+" se dibuja (dos líneas) para que quede perfectamente centrado
            btnMas.Paint += (s, e) =>
            {
                int cx = btnMas.Width / 2, cy = btnMas.Height / 2, r = 6;
                using (var pen = new Pen(EstiloPos.Verde, 2.4f))
                {
                    e.Graphics.DrawLine(pen, cx - r, cy, cx + r, cy);
                    e.Graphics.DrawLine(pen, cx, cy - r, cx, cy + r);
                }
            };
            btnMas.Click += (s, e) =>
            {
                ventaService.NuevaVenta();
                RefrescarCarrito();
                txtCodigo.Focus();
            };
            pnlTabs.Controls.Add(btnMas);

            pnlTabs.ResumeLayout();
        }

        private Button CrearTab(VentaEnCurso v, int numero, bool activa)
        {
            var b = new Button
            {
                Text      = "Venta " + numero,
                AutoSize  = false,
                Width     = 70,
                Height    = 30,
                FlatStyle = FlatStyle.Flat,
                Font      = EstiloPos.FontSmall,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = activa ? EstiloPos.Ink1 : EstiloPos.Surface,
                ForeColor = activa ? Color.White    : EstiloPos.Ink2,
                UseVisualStyleBackColor = false,
                Margin    = new Padding(0, 0, 6, 0)
            };
            b.FlatAppearance.BorderColor = activa ? EstiloPos.Ink1 : EstiloPos.Border;
            b.FlatAppearance.BorderSize  = 1;
            b.FlatAppearance.MouseOverBackColor = activa ? Color.FromArgb(45, 45, 55) : EstiloPos.Hover;
            int id = v.Id;
            b.Click += (s, e) =>
            {
                ventaService.ActivarVenta(id);
                RefrescarCarrito();
                txtCodigo.Focus();
            };
            return b;
        }

        // ── Carrito ────────────────────────────────────────────────────

        private void RefrescarCarrito()
        {
            dgvCarrito.Rows.Clear();
            foreach (var d in ventaService.Carrito)
                dgvCarrito.Rows.Add(
                    d.IdProducto,
                    d.NombreProducto,
                    "−",
                    d.Cantidad.ToString("0.##"),
                    "+",
                    "$" + d.Subtotal.ToString("N0"),
                    "✕");
            decimal total = ventaService.Total;
            lblTotal.Text = "$" + total.ToString("N0");
            if (lblDesgloseIva != null)
            {
                string txt = ventaService.Descuento > 0
                    ? "Desc. -$" + ventaService.Descuento.ToString("N0") + "     "
                    : "";
                txt += "Neto $" + Impuestos.Neto(total).ToString("N0") +
                       "     IVA 19% $" + Impuestos.Iva(total).ToString("N0");
                lblDesgloseIva.Text = txt;
            }
            lblMensaje.Visible = false;
            RefrescarTabs();
        }

        private void dgvCarrito_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string col = dgvCarrito.Columns[e.ColumnIndex].Name;
            int id = Convert.ToInt32(dgvCarrito.Rows[e.RowIndex].Cells["colVId"].Value);
            try
            {
                if (col == "colQuitar")     ventaService.QuitarDelCarrito(id);
                else if (col == "colMenos") ventaService.AjustarCantidadCarrito(id, -1);
                else if (col == "colMas")   ventaService.AjustarCantidadCarrito(id, +1);
                else return;
                RefrescarCarrito();
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
        }

        private void DgvCarrito_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) { dgvCarrito.Cursor = Cursors.Default; return; }
            string col = dgvCarrito.Columns[e.ColumnIndex].Name;
            dgvCarrito.Cursor = (col == "colMenos" || col == "colMas" || col == "colQuitar")
                ? Cursors.Hand : Cursors.Default;
        }

        // Dibuja los símbolos − + ✕ de las columnas de acción (en vez de texto, así no se truncan)
        private void DgvCarrito_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string col = dgvCarrito.Columns[e.ColumnIndex].Name;
            if (col != "colMenos" && col != "colMas" && col != "colQuitar") return;

            e.PaintBackground(e.CellBounds, true);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int cx = e.CellBounds.Left + e.CellBounds.Width / 2;
            int cy = e.CellBounds.Top + e.CellBounds.Height / 2;
            int r = 7;
            Color color = col == "colMenos" ? EstiloPos.Ink1
                        : col == "colMas"   ? EstiloPos.Verde
                        :                     EstiloPos.Rojo;
            using (var pen = new Pen(color, 2.6f))
            {
                if (col == "colMenos")
                {
                    e.Graphics.DrawLine(pen, cx - r, cy, cx + r, cy);
                }
                else if (col == "colMas")
                {
                    e.Graphics.DrawLine(pen, cx - r, cy, cx + r, cy);
                    e.Graphics.DrawLine(pen, cx, cy - r, cx, cy + r);
                }
                else // ✕
                {
                    e.Graphics.DrawLine(pen, cx - r, cy - r, cx + r, cy + r);
                    e.Graphics.DrawLine(pen, cx - r, cy + r, cx + r, cy - r);
                }
            }
            e.Handled = true;
        }

        // ── Cobro ──────────────────────────────────────────────────────

        private void btnCobrar_Click(object sender, EventArgs e)
        {
            if (ventaService.Carrito.Count == 0) { MostrarMensaje("El carrito está vacío."); return; }
            string  medioPago = comboMedioPago.SelectedItem.ToString();
            decimal total     = ventaService.Total;

            // En efectivo: pedir con cuánto paga y calcular el vuelto antes de registrar
            decimal vuelto = 0;
            if (medioPago == MedioPago.Efectivo)
            {
                using (var dlg = new FormCobroEfectivo(total))
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;
                    vuelto = dlg.Vuelto;
                }
            }

            try
            {
                int idVenta = ventaService.CobrarVenta(Sesion.UsuarioActual.IdUsuario, medioPago);
                string msg = "Venta N° " + idVenta + "  ·  $" + total.ToString("N0") + "\n" +
                    "Neto $" + Impuestos.Neto(total).ToString("N0") + "   ·   IVA $" + Impuestos.Iva(total).ToString("N0") + "\n" +
                    "Medio de pago: " + medioPago;
                if (medioPago == MedioPago.Efectivo && vuelto > 0)
                    msg += "\nVuelto: $" + vuelto.ToString("N0");
                Aviso.Exito(this, msg, "Venta registrada");
                RefrescarCarrito();
                CargarGridProductos();
                txtCodigo.Focus();
            }
            catch (Exception ex) { MostrarMensaje(ex.Message); }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            int items = ventaService.Carrito.Count;
            // Si es la única venta abierta y está vacía, no hay nada que cancelar
            if (items == 0 && ventaService.VentasEnCurso.Count <= 1) return;

            string detalle = items > 0
                ? "Se descartará esta venta y sus " + items + " producto(s) del carrito."
                : "Se cerrará esta venta.";
            if (!Aviso.Confirmar(this, detalle, "¿Cancelar la venta?", "Sí, cancelar"))
                return;

            ventaService.CerrarVenta(ventaService.Activa.Id);
            RefrescarCarrito();
            txtCodigo.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F12) { btnCobrar_Click(this, EventArgs.Empty); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerVentas?.Stop();
            base.OnFormClosed(e);
        }

        // ── Log ────────────────────────────────────────────────────────

        private void btnVerLog_Click(object sender, EventArgs e)
        {
            logVentasVisible  = !logVentasVisible;
            splitterLogV.Visible  = logVentasVisible;
            pnlLogVentas.Visible  = logVentasVisible;
            btnVerLog.Text    = logVentasVisible ? "▲ Ocultar registro" : "▼ Registro de ventas";
            if (logVentasVisible) CargarLogVentas();
        }

        private void CargarLogVentas()
        {
            ventasRegistro = ventaService.ObtenerVentas(dtpDesdeV.Value.Date, dtpHastaV.Value.Date);
            dgvLogVentas.Rows.Clear();
            foreach (var v in ventasRegistro)
                dgvLogVentas.Rows.Add(v.IdVenta, "#" + v.IdVenta, v.Fecha.ToString("dd/MM HH:mm"),
                    v.MedioPago, "$" + v.Total.ToString("N0"));
        }

        private void DgvLogVentas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgvLogVentas.Rows[e.RowIndex].Cells["colRegId"].Value);
            var v = ventasRegistro.Find(x => x.IdVenta == id);
            if (v != null)
                using (var f = new FormDetalleVenta(v)) f.ShowDialog(this);
        }

        private void btnFiltrarLogV_Click(object sender, EventArgs e) => CargarLogVentas();

        // ── Utilidades ─────────────────────────────────────────────────

        private decimal LeerCantidad()
        {
            return decimal.TryParse(txtCantidad.Text, out decimal c) && c > 0 ? c : 1;
        }

        private void MostrarMensaje(string msg) { lblMensaje.Text = msg; lblMensaje.Visible = true; }
    }
}

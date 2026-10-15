using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public sealed class FormMovimientosCaja : Form
    {
        private readonly MovimientoCajaService servicio = new MovimientoCajaService();
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly DateTimePicker desde = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 135 };
        private readonly DateTimePicker hasta = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 135 };
        private readonly Label totales = new Label { Dock = DockStyle.Top, Height = 54, Padding = new Padding(0, 8, 0, 8) };
        private readonly Timer filtroPendiente = new Timer { Interval = 300 };
        private bool cambiandoRango, iniciado;
        private readonly Button anular;
        private int? cajaAbierta;

        public FormMovimientosCaja()
        {
            Text = "Gastos y movimientos de caja"; StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false; ShowIcon = false; Size = new Size(1100, 650);
            MinimumSize = new Size(850, 520); BackColor = EstiloPos.Fondo; Font = EstiloPos.FontBody;
            AutoScaleMode = AutoScaleMode.Font; Padding = new Padding(20);
            var titulo = new Label { Text = Text, Font = EstiloPos.FontTitulo, AutoSize = true, Dock = DockStyle.Top };
            var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 8, 0, 0), WrapContents = false };
            filtros.Controls.Add(new Label { Text = "Desde", AutoSize = true, Padding = new Padding(0, 5, 0, 0) }); filtros.Controls.Add(desde);
            filtros.Controls.Add(new Label { Text = "Hasta", AutoSize = true, Padding = new Padding(8, 5, 0, 0) }); filtros.Controls.Add(hasta);
            filtros.Controls.Add(new Label { Text = "Se aplica automáticamente", AutoSize = true, ForeColor = EstiloPos.Ink2, Padding = new Padding(12, 5, 0, 0) });
            desde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            hasta.Value = DateTime.Today;
            var rangos = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Padding = new Padding(0, 0, 0, 4) };
            AgregarRango(rangos, "Hoy", () => SetRango(DateTime.Today, DateTime.Today));
            AgregarRango(rangos, "Ayer", () => SetRango(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1)));
            AgregarRango(rangos, "7 días", () => SetRango(DateTime.Today.AddDays(-6), DateTime.Today));
            AgregarRango(rangos, "30 días", () => SetRango(DateTime.Today.AddDays(-29), DateTime.Today));
            AgregarRango(rangos, "Este mes", () => SetRango(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today));
            AgregarRango(rangos, "Mes anterior", () =>
            {
                var primero = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                SetRango(primero.AddMonths(-1), primero.AddDays(-1));
            });
            AgregarRango(rangos, "Todo", () => SetRango(desde.MinDate.Date, DateTime.Today));
            desde.ValueChanged += (s, e) => ProgramarFiltro();
            hasta.ValueChanged += (s, e) => ProgramarFiltro();
            filtroPendiente.Tick += (s, e) => Cargar();
            Disposed += (s, e) => filtroPendiente.Dispose();

            var acciones = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(0, 52), Padding = new Padding(0, 8, 0, 0) };
            var nuevo = Boton("Registrar", Registrar, true); nuevo.Enabled = Sesion.EsAdmin;
            anular = Boton("Anular seleccionado", Anular, false); anular.Width = 205;
            acciones.Controls.Add(nuevo); acciones.Controls.Add(anular);
            acciones.Controls.Add(new Label { Text = Sesion.EsAdmin ? "Los turnos cerrados no se modifican." : "Consulta · Solo el administrador registra y anula.", AutoSize = true, Padding = new Padding(12, 8, 0, 0) });

            foreach (string nombre in new[] { "Fecha", "Tipo", "Descripción", "Monto", "Pago / origen", "Turno", "Usuario", "Estado" })
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = nombre, MinimumWidth = 65,
                    FillWeight = nombre == "Descripción" ? 180 : nombre == "Pago / origen" ? 150 : 95 });
            EstiloPos.AplicarGrid(grid);
            // Conservar columnas legibles: en ventanas pequeñas se desplazan horizontalmente.
            int[] anchos = { 145, 100, 180, 100, 180, 65, 130, 90 };
            for (int i = 0; i < grid.Columns.Count; i++) grid.Columns[i].MinimumWidth = anchos[i];
            // CurrentCellChanged se dispara cuando CurrentRow ya corresponde a la nueva fila.
            grid.CurrentCellChanged += (s, e) => Seleccionar();
            DetallesGrid.Conectar(this, grid, fila => DetallesGrid.Movimiento(fila.Tag as MovimientoCaja));
            var ayudaDetalle = new Label { Text = "Doble clic sobre un movimiento para ver su detalle.",
                AutoSize = true, Dock = DockStyle.Top, Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink2,
                Padding = new Padding(0, 0, 0, 8) };
            Controls.Add(grid); Controls.Add(acciones);
            Controls.Add(ayudaDetalle); Controls.Add(totales); Controls.Add(rangos); Controls.Add(filtros); Controls.Add(titulo);
            Shown += (s, e) => { iniciado = true; Cargar(); };
        }

        private void AgregarRango(FlowLayoutPanel contenedor, string texto, Action accion)
        {
            var boton = Boton(texto, accion, false);
            boton.Width = TextRenderer.MeasureText(texto, boton.Font).Width + 28;
            contenedor.Controls.Add(boton);
        }

        private void ProgramarFiltro()
        {
            if (!iniciado || cambiandoRango) return;
            filtroPendiente.Stop();
            filtroPendiente.Start();
        }

        private void SetRango(DateTime inicio, DateTime fin)
        {
            filtroPendiente.Stop();
            cambiandoRango = true;
            try { desde.Value = inicio; hasta.Value = fin; }
            finally { cambiandoRango = false; }
            Cargar();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5) { Cargar(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private Button Boton(string texto, Action accion, bool primario)
        {
            var b = new Button { Text = texto, Width = 125, Height = 34 };
            if (primario) EstiloPos.AplicarBotonPrimario(b); else EstiloPos.AplicarBotonSecundario(b);
            b.Click += (s, e) => accion(); return b;
        }

        private MovimientoCaja Seleccion => grid.CurrentRow?.Tag as MovimientoCaja;

        private void Cargar()
        {
            filtroPendiente.Stop();
            if (hasta.Value.Date < desde.Value.Date)
            {
                grid.Rows.Clear();
                totales.Text = "Revisa las fechas: Hasta debe ser igual o posterior a Desde.";
                totales.ForeColor = EstiloPos.Rojo;
                Seleccionar();
                return;
            }
            try
            {
                int? seleccionado = Seleccion?.IdMovimiento;
                var movimientos = servicio.Obtener(desde.Value, hasta.Value);
                cajaAbierta = new CajaService().ObtenerCajaAbierta()?.IdCaja;
                grid.Rows.Clear();
                foreach (var m in movimientos)
                {
                    int i = grid.Rows.Add(m.Fecha.ToString("dd/MM/yy HH:mm"), m.Tipo, m.Descripcion,
                        "$" + m.Monto.ToString("N0"), m.MedioPago + " / " + m.Origen, m.IdCaja?.ToString() ?? "—",
                        m.NombreUsuario, m.Anulado ? "Anulado" : "Vigente");
                    grid.Rows[i].Tag = m;
                    if (m.Anulado) grid.Rows[i].DefaultCellStyle.ForeColor = EstiloPos.Ink3;
                }
                var filaSeleccionada = grid.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(f => (f.Tag as MovimientoCaja)?.IdMovimiento == seleccionado);
                if (filaSeleccionada != null) grid.CurrentCell = filaSeleccionada.Cells[0];
                decimal gastos = movimientos.Where(m => !m.Anulado && m.Tipo == TipoMovimientoCaja.Gasto).Sum(m => m.Monto);
                decimal entradas = movimientos.Sum(m => Math.Max(0, m.EfectoCaja));
                decimal salidas = -movimientos.Sum(m => Math.Min(0, m.EfectoCaja));
                totales.ForeColor = EstiloPos.Ink1;
                totales.Text = "Período: gastos $" + gastos.ToString("N0") + "   ·   Entradas al cajón $" + entradas.ToString("N0") +
                    "   ·   Salidas del cajón $" + salidas.ToString("N0") + "   ·   " + movimientos.Count + " registros";
                Seleccionar();
            }
            catch (Exception ex)
            {
                grid.Rows.Clear(); totales.Text = "No se pudo cargar el período.";
                Seleccionar(); Errores.Mostrar(this, ex);
            }
        }

        private void Seleccionar()
        {
            var m = Seleccion;
            anular.Enabled = Sesion.EsAdmin && m != null && !m.Anulado && (!m.IdCaja.HasValue || m.IdCaja == cajaAbierta);
        }

        private void Registrar()
        {
            using (var f = new FormRegistrarMovimiento())
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    // Mostrar lo recién registrado aunque antes se consultara otro período.
                    SetRango(DateTime.Today, DateTime.Today);
                }
        }

        private void Anular()
        {
            var m = Seleccion; if (m == null) return;
            using (var f = new Form { Text = "Anular movimiento N°" + m.IdMovimiento, Size = new Size(540, 300),
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent, ShowInTaskbar = false, Font = EstiloPos.FontBody, Padding = new Padding(20) })
            {
                var texto = new TextBox { Multiline = true, Dock = DockStyle.Fill, MaxLength = 300 };
                var encabezado = new Label { AutoSize = true, Dock = DockStyle.Top, Text = "Motivo de anulación (obligatorio).\nEl registro original se conservará.", Padding = new Padding(0, 0, 0, 12) };
                var botones = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48 };
                var confirmar = Boton("Anular", () =>
                {
                    try { servicio.Anular(m.IdMovimiento, texto.Text); f.DialogResult = DialogResult.OK; }
                    catch (Exception ex) { Errores.Mostrar(f, ex); }
                }, true);
                var cancelar = Boton("Cancelar", () => f.DialogResult = DialogResult.Cancel, false);
                botones.Controls.Add(confirmar); botones.Controls.Add(cancelar); f.CancelButton = cancelar;
                f.Controls.Add(texto); f.Controls.Add(encabezado); f.Controls.Add(botones);
                if (f.ShowDialog(this) == DialogResult.OK) Cargar();
            }
        }
    }

    public sealed class FormRegistrarMovimiento : Form
    {
        private readonly ComboBox tipo = Combo(TipoMovimientoCaja.Gasto, TipoMovimientoCaja.Retiro, TipoMovimientoCaja.Reposicion);
        private readonly ComboBox origen = Combo(OrigenMovimientoCaja.Cajon, OrigenMovimientoCaja.Externo);
        private readonly ComboBox medio = Combo(MedioPago.Efectivo, MedioPago.Transferencia, MedioPago.Tarjeta);
        private readonly ComboBox categoria = new ComboBox { Dock = DockStyle.Fill, MaxLength = 80 };
        private readonly TextBox descripcion = new TextBox { Dock = DockStyle.Fill, MaxLength = 300 };
        private readonly TextBox referencia = new TextBox { Dock = DockStyle.Fill, MaxLength = 120 };
        private readonly NumericUpDown monto = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 999999999999m, ThousandsSeparator = true, DecimalPlaces = 0 };
        private readonly Label ayuda = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = EstiloPos.Ink2 };
        private readonly Label error = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = EstiloPos.Rojo };

        public FormRegistrarMovimiento()
        {
            Text = "Registrar gasto o movimiento"; StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false; ShowIcon = false; FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false; ClientSize = new Size(620, 570);
            Font = EstiloPos.FontBody; BackColor = EstiloPos.Fondo; AutoScaleMode = AutoScaleMode.Font;
            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 11 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165)); tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 8; i++) tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            Fila(tabla, 0, "Tipo", tipo); Fila(tabla, 1, "Categoría", categoria);
            Fila(tabla, 2, "Descripción", descripcion); Fila(tabla, 3, "Monto (CLP)", monto);
            Fila(tabla, 4, "Medio de pago", medio); Fila(tabla, 5, "Origen del dinero", origen);
            Fila(tabla, 6, "Referencia", referencia);
            var caja = new CajaService().ObtenerCajaAbierta();
            Fila(tabla, 7, "Fecha / turno", new Label { AutoSize = true, Text = DateTime.Now.ToString("dd/MM/yyyy") + " · " + (caja == null ? "Sin caja abierta" : "Caja N°" + caja.IdCaja) });
            tabla.Controls.Add(ayuda, 0, 8); tabla.SetColumnSpan(ayuda, 2);
            tabla.Controls.Add(error, 0, 9); tabla.SetColumnSpan(error, 2);
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var guardar = new Button { Text = "Guardar", Width = 130, Height = 36, Enabled = Sesion.EsAdmin };
            var cancelar = new Button { Text = "Cancelar", Width = 130, Height = 36, DialogResult = DialogResult.Cancel };
            EstiloPos.AplicarBotonPrimario(guardar); EstiloPos.AplicarBotonSecundario(cancelar);
            guardar.Click += (s, e) => Guardar(); acciones.Controls.Add(guardar); acciones.Controls.Add(cancelar);
            tabla.Controls.Add(acciones, 0, 10); tabla.SetColumnSpan(acciones, 2); Controls.Add(tabla);
            AcceptButton = guardar; CancelButton = cancelar;
            categoria.Items.AddRange(new object[] { "Insumos y bolsas", "Transporte", "Servicios", "Limpieza", "Mantención", "Otros" });
            categoria.SelectedIndex = 0;
            tipo.SelectedIndexChanged += (s, e) => Configurar();
            medio.SelectedIndexChanged += (s, e) => { if ((string)medio.SelectedItem != MedioPago.Efectivo) origen.SelectedItem = OrigenMovimientoCaja.Externo; Configurar(); };
            origen.SelectedIndexChanged += (s, e) => Configurar();
            if (caja == null) origen.SelectedItem = OrigenMovimientoCaja.Externo;
            Configurar();
        }

        private static ComboBox Combo(params string[] valores)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            c.Items.AddRange(valores); c.SelectedIndex = 0; return c;
        }
        private static void Fila(TableLayoutPanel t, int fila, string nombre, Control control)
        {
            t.Controls.Add(new Label { Text = nombre, AutoSize = true, Padding = new Padding(0, 5, 0, 0) }, 0, fila);
            t.Controls.Add(control, 1, fila);
        }
        private bool configurando;
        private void Configurar()
        {
            if (configurando) return;
            configurando = true;
            bool gasto = (string)tipo.SelectedItem == TipoMovimientoCaja.Gasto;
            categoria.Enabled = gasto; medio.Enabled = gasto;
            if (!gasto) { medio.SelectedItem = MedioPago.Efectivo; origen.SelectedItem = OrigenMovimientoCaja.Cajon; }
            origen.Enabled = gasto && (string)medio.SelectedItem == MedioPago.Efectivo;
            ayuda.Text = (string)origen.SelectedItem == OrigenMovimientoCaja.Cajon
                ? ((string)tipo.SelectedItem == TipoMovimientoCaja.Reposicion ? "Suma efectivo al cajón. No es una venta." : "Resta efectivo del cajón. Requiere una caja abierta.")
                : "Registra el gasto sin modificar el efectivo del cajón.";
            ayuda.Text += " La referencia es opcional.";
            configurando = false;
        }
        private void Guardar()
        {
            try
            {
                new MovimientoCajaService().Registrar(new MovimientoCaja
                {
                    Tipo = (string)tipo.SelectedItem, Categoria = categoria.Enabled ? categoria.Text : "",
                    Descripcion = descripcion.Text, Monto = monto.Value, MedioPago = (string)medio.SelectedItem,
                    Origen = (string)origen.SelectedItem, Referencia = referencia.Text
                });
                DialogResult = DialogResult.OK;
            }
            catch (Dominio.NegocioException ex) { error.Text = ex.Message; }
            catch (Exception ex) { Errores.Mostrar(this, ex); }
        }
    }
}

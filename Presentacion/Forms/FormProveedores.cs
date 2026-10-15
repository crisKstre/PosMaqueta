using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public sealed class FormProveedores : Form
    {
        private readonly ProveedorService servicio = new ProveedorService();
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly TextBox buscar = new TextBox { Width = 320, MaxLength = 160 };
        private readonly ComboBox estado = new ComboBox { Width = 135, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label resumen = new Label { Dock = DockStyle.Top, Height = 38, ForeColor = EstiloPos.Ink2 };
        private readonly Timer filtro = new Timer { Interval = 250 };
        private readonly Button editar, cambiarEstado;

        public FormProveedores()
        {
            Text = "Proveedores"; Size = new Size(1100, 650); MinimumSize = new Size(780, 480);
            BackColor = EstiloPos.Fondo; Font = EstiloPos.FontBody; Padding = new Padding(24);
            var titulo = new Label { Text = "Proveedores", Dock = DockStyle.Top, Height = 48, Font = EstiloPos.FontTitulo };
            var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 0, 0, 10) };
            filtros.Controls.Add(new Label { Text = "Buscar", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
            filtros.Controls.Add(buscar); filtros.Controls.Add(estado);
            estado.Items.AddRange(new object[] { "Activos", "Inactivos", "Todos" }); estado.SelectedIndex = 0;
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0, 12, 0, 0) };
            var nuevo = Boton("Nuevo proveedor", () => Editar(null)); nuevo.Enabled = Sesion.EsAdmin;
            editar = Boton("Editar", () => Editar(Seleccion));
            cambiarEstado = Boton("Desactivar", CambiarEstado);
            acciones.Controls.AddRange(new Control[] { nuevo, editar, cambiarEstado });
            foreach (string nombre in new[] { "Proveedor", "RUT", "Contacto", "Teléfono", "Correo", "Estado" })
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = nombre, MinimumWidth = nombre == "Proveedor" ? 190 : 110, FillWeight = nombre == "Proveedor" ? 180 : 100 });
            EstiloPos.AplicarGrid(grid);
            grid.CurrentCellChanged += (s, e) => ActualizarAcciones();
            DetallesGrid.Conectar(this, grid, fila => CrearDetalle(fila.Tag as Proveedor));
            Controls.Add(grid); Controls.Add(acciones); Controls.Add(resumen); Controls.Add(filtros); Controls.Add(titulo);
            buscar.TextChanged += (s, e) => { filtro.Stop(); filtro.Start(); };
            estado.SelectedIndexChanged += (s, e) => Cargar();
            filtro.Tick += (s, e) => Cargar();
            Disposed += (s, e) => filtro.Dispose();
            Load += (s, e) => Cargar();
            ActualizarAcciones();
        }

        private Button Boton(string texto, Action accion)
        {
            var b = new Button { Text = texto, Width = 170, Height = 38 };
            EstiloPos.AplicarBotonSecundario(b); b.Click += (s, e) => accion(); return b;
        }
        private Proveedor Seleccion => grid.CurrentRow?.Tag as Proveedor;
        private void ActualizarAcciones()
        {
            editar.Enabled = cambiarEstado.Enabled = Sesion.EsAdmin && Seleccion != null;
            cambiarEstado.Text = Seleccion?.Activo == false ? "Reactivar" : "Desactivar";
        }
        private void Cargar(int? seleccionar = null)
        {
            filtro.Stop();
            try
            {
                seleccionar = seleccionar ?? Seleccion?.IdProveedor;
                var lista = servicio.Obtener(buscar.Text, estado.SelectedIndex == 2 ? (bool?)null : estado.SelectedIndex == 0);
                grid.Rows.Clear();
                foreach (var p in lista)
                {
                    var fila = grid.Rows[grid.Rows.Add(p.Nombre, p.Rut, p.Contacto, p.Telefono, p.Correo, p.Activo ? "Activo" : "Inactivo")];
                    fila.Tag = p;
                    if (!p.Activo) fila.DefaultCellStyle.ForeColor = EstiloPos.Ink3;
                }
                var elegida = grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(f => ((Proveedor)f.Tag).IdProveedor == seleccionar);
                if (elegida != null) grid.CurrentCell = elegida.Cells[0];
                resumen.Text = lista.Count + " proveedores · Doble clic para ver el detalle." + (Sesion.EsAdmin ? "" : " · Solo consulta");
                ActualizarAcciones();
            }
            catch (Exception ex) { grid.Rows.Clear(); ActualizarAcciones(); Errores.Mostrar(this, ex); }
        }
        private void Editar(Proveedor p)
        {
            if (!Sesion.EsAdmin) return;
            using (var f = new FormEditarProveedor(p))
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    buscar.Clear(); estado.SelectedIndex = 2;
                    Cargar(f.IdGuardado);
                }
        }
        private void CambiarEstado()
        {
            var p = Seleccion; if (p == null || !Sesion.EsAdmin) return;
            string accion = p.Activo ? "Desactivar" : "Reactivar";
            if (!Aviso.Confirmar(this, accion + " a «" + p.Nombre + "»?\nSus datos se conservarán.", accion + " proveedor", accion)) return;
            try { servicio.CambiarEstado(p.IdProveedor, !p.Activo); Cargar(); }
            catch (Exception ex) { Errores.Mostrar(this, ex); }
        }
        private static Form CrearDetalle(Proveedor p)
        {
            if (p == null) return null;
            var f = new FormDetalleRegistro("Detalle del proveedor", p.Nombre);
            f.AgregarSeccion("Identificación", "Nombre / razón social", p.Nombre, "RUT", p.Rut, "Estado", p.Activo ? "Activo" : "Inactivo");
            f.AgregarSeccion("Contacto", "Persona de contacto", p.Contacto, "Teléfono", p.Telefono, "Correo", p.Correo, "Dirección", p.Direccion);
            f.AgregarSeccion("Notas", "Observaciones", p.Notas);
            f.AgregarSeccion("Registro", "Creado", p.FechaCreacion.ToString("dd/MM/yyyy HH:mm"), "Última actualización", p.FechaActualizacion.ToString("dd/MM/yyyy HH:mm"));
            return f;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        { if (keyData == Keys.F5) { Cargar(); return true; } return base.ProcessCmdKey(ref msg, keyData); }
    }

    public sealed class FormEditarProveedor : Form
    {
        private readonly TextBox nombre = Campo(120), rut = Campo(15), contacto = Campo(120), telefono = Campo(40), correo = Campo(160), direccion = Campo(240), notas = Campo(1000);
        private readonly Label error = new Label { Dock = DockStyle.Fill, ForeColor = EstiloPos.Rojo };
        private readonly int id;
        public int IdGuardado { get; private set; }

        public FormEditarProveedor(Proveedor p)
        {
            id = p?.IdProveedor ?? 0;
            Text = id == 0 ? "Nuevo proveedor" : "Editar proveedor";
            StartPosition = FormStartPosition.CenterParent; ShowIcon = false; ShowInTaskbar = false;
            Size = new Size(700, 680); MinimumSize = new Size(600, 600); BackColor = EstiloPos.Fondo; Font = EstiloPos.FontBody;
            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 11 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 165)); tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++) tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); tabla.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            var campos = new[] { nombre, rut, contacto, telefono, correo, direccion, notas };
            var etiquetas = new[] { "Nombre / razón social *", "RUT", "Contacto", "Teléfono", "Correo", "Dirección", "Notas" };
            // Notas ocupa el espacio flexible; las otras seis filas mantienen altura estable.
            tabla.RowStyles[6].SizeType = SizeType.Percent; tabla.RowStyles[6].Height = 100;
            tabla.RowStyles[7].SizeType = SizeType.Absolute; tabla.RowStyles[7].Height = 0;
            for (int i = 0; i < campos.Length; i++)
            {
                tabla.Controls.Add(new Label { Text = etiquetas[i], Dock = DockStyle.Fill, Font = EstiloPos.FontSmall }, 0, i);
                tabla.Controls.Add(campos[i], 1, i);
            }
            notas.Multiline = true; notas.ScrollBars = ScrollBars.Vertical;
            var ayuda = new Label { Text = "* Obligatorio. Los demás datos son opcionales.", Dock = DockStyle.Fill, Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink2 };
            tabla.Controls.Add(ayuda, 0, 8); tabla.SetColumnSpan(ayuda, 2);
            tabla.Controls.Add(error, 0, 9); tabla.SetColumnSpan(error, 2);
            var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var guardar = new Button { Text = "Guardar", Width = 130, Height = 36, Enabled = Sesion.EsAdmin };
            var cancelar = new Button { Text = "Cancelar", Width = 130, Height = 36, DialogResult = DialogResult.Cancel };
            EstiloPos.AplicarBotonPrimario(guardar); EstiloPos.AplicarBotonSecundario(cancelar);
            guardar.Click += (s, e) => Guardar(); botones.Controls.Add(guardar); botones.Controls.Add(cancelar);
            tabla.Controls.Add(botones, 0, 10); tabla.SetColumnSpan(botones, 2); Controls.Add(tabla);
            AcceptButton = guardar; CancelButton = cancelar;
            if (p != null) { nombre.Text = p.Nombre; rut.Text = p.Rut; contacto.Text = p.Contacto; telefono.Text = p.Telefono; correo.Text = p.Correo; direccion.Text = p.Direccion; notas.Text = p.Notas; }
        }
        private static TextBox Campo(int max) => new TextBox { Dock = DockStyle.Fill, MaxLength = max };
        private void Guardar()
        {
            try
            {
                var p = new Proveedor { IdProveedor = id, Nombre = nombre.Text, Rut = rut.Text, Contacto = contacto.Text, Telefono = telefono.Text, Correo = correo.Text, Direccion = direccion.Text, Notas = notas.Text };
                var servicio = new ProveedorService();
                if (id == 0) IdGuardado = servicio.Crear(p); else { servicio.Actualizar(p); IdGuardado = id; }
                DialogResult = DialogResult.OK;
            }
            catch (Dominio.NegocioException ex) { error.Text = ex.Message; }
            catch (Exception ex) { Errores.Mostrar(this, ex); }
        }
    }
}

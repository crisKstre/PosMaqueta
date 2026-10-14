using System;
using System.Drawing;
using System.Windows.Forms;
using Entidades;

namespace Presentacion.Forms
{
    public sealed class FormCantidadVenta : Form
    {
        private readonly bool porPeso;
        private readonly decimal disponible;
        private readonly decimal precio;
        private readonly TextBox entrada;
        private readonly ComboBox unidad;
        private readonly Label resultado;
        private readonly Button confirmar;
        public decimal Cantidad { get; private set; }

        public FormCantidadVenta(Producto producto, decimal? cantidadInicial, decimal disponible,
            decimal precioUnitario, bool editando = false)
        {
            SuspendLayout();
            porPeso = producto.UnidadMedida == UnidadMedida.Kilogramo;
            this.disponible = disponible;
            precio = precioUnitario;
            Text = editando ? "Editar cantidad" : "Ingresar peso";
            ClientSize = new Size(480, 340);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = ShowInTaskbar = ShowIcon = false;
            BackColor = EstiloPos.Surface;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = EstiloPos.FontLabel;

            var nombre = new Label { Text = producto.Nombre, AutoEllipsis = true,
                Location = new Point(24, 20), Size = new Size(432, 32), Font = EstiloPos.FontTabla };
            var precioLabel = new Label { Text = "$" + precio.ToString("N0") + (porPeso ? " / kg" : " / unidad"),
                Location = new Point(24, 56), Size = new Size(432, 24) };
            var instruccion = new Label { Text = editando ? "Cantidad total que quedará en el carrito:" : "Cantidad que deseas agregar:",
                Location = new Point(24, 92), Size = new Size(432, 24) };
            entrada = new TextBox { Text = cantidadInicial?.ToString("0.###") ?? "", MaxLength = 32,
                Location = new Point(24, 124), Size = new Size(248, 32), Font = EstiloPos.FontTabla,
                TabIndex = 0, TextAlign = HorizontalAlignment.Right };
            unidad = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(284, 124), Size = new Size(172, 32), TabIndex = 1 };
            unidad.Items.Add(porPeso ? "Kilogramos (kg)" : "Unidades");
            if (porPeso) unidad.Items.Add("Gramos (g)");
            unidad.SelectedIndex = 0;
            unidad.Enabled = porPeso;
            var ayuda = new Label { Text = porPeso ? "Ejemplo: 0,350 kg o 350 gramos. Coma o punto decimal." : "Ingresa unidades enteras.",
                Location = new Point(24, 164), Size = new Size(432, 42), Font = EstiloPos.FontSmall };
            resultado = new Label { Location = new Point(24, 208), Size = new Size(432, 56), Font = EstiloPos.FontLabel };
            confirmar = new Button { Text = editando ? "Guardar cantidad" : "Agregar al carrito",
                Location = new Point(24, 278), Size = new Size(260, 40), TabIndex = 2 };
            EstiloPos.AplicarBotonPrimario(confirmar);
            var cancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel,
                Location = new Point(296, 278), Size = new Size(160, 40), TabIndex = 3 };
            EstiloPos.AplicarBotonSecundario(cancelar);
            Controls.AddRange(new Control[] { nombre, precioLabel, instruccion, entrada, unidad, ayuda, resultado, confirmar, cancelar });
            AcceptButton = confirmar;
            CancelButton = cancelar;
            entrada.TextChanged += (s, e) => Actualizar();
            // Evitar reinterpretar una entrada de 350 gramos como 350 kilos al cambiar de unidad.
            unidad.SelectedIndexChanged += (s, e) => { entrada.Clear(); entrada.Focus(); };
            confirmar.Click += (s, e) => { if (Actualizar()) { DialogResult = DialogResult.OK; Close(); } };
            Shown += (s, e) => { entrada.Focus(); entrada.SelectAll(); };
            Actualizar();
            ResumeLayout(true);
        }

        private bool Actualizar()
        {
            bool valida = EntradaCantidad.TryCalcular(entrada.Text, porPeso, unidad.SelectedIndex == 1,
                disponible, precio, out decimal cantidad, out decimal subtotal, out string error);
            confirmar.Enabled = valida;
            Cantidad = valida ? cantidad : 0m;
            resultado.ForeColor = valida ? EstiloPos.Verde : EstiloPos.Rojo;
            resultado.Text = valida
                ? cantidad.ToString("0.###") + (porPeso ? " kg" : " unidades") + " · Subtotal $" + subtotal.ToString("N0")
                : error;
            return valida;
        }
    }
}

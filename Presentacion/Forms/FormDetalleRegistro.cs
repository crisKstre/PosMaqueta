using System;
using System.Drawing;
using System.Windows.Forms;
using Entidades;

namespace Presentacion.Forms
{
    /// <summary>Consulta de solo lectura, con secciones y texto completo.</summary>
    public class FormDetalleRegistro : Form
    {
        private readonly Panel contenido;
        private bool ajustando;

        public FormDetalleRegistro(string titulo, string contexto)
        {
            Text = titulo;
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;
            ShowInTaskbar = false;
            MinimizeBox = false;
            Size = new Size(720, 640);
            MinimumSize = new Size(520, 400);
            BackColor = EstiloPos.Fondo;

            var cuerpo = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(24) };
            contenido = new Panel { Dock = DockStyle.Top };
            contenido.SizeChanged += (s, e) => AjustarContenido();
            AgregarBloque(new Label { Text = titulo, Font = EstiloPos.FontSubtitulo,
                ForeColor = EstiloPos.Ink1, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 8) });
            AgregarBloque(new Label { Text = contexto, Font = EstiloPos.FontSmall,
                ForeColor = EstiloPos.Ink2, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 18) });
            cuerpo.Controls.Add(contenido);

            var pie = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(24, 12, 24, 12) };
            var cerrar = new Button { Text = "Cerrar", Dock = DockStyle.Right, Width = 120, DialogResult = DialogResult.Cancel };
            EstiloPos.AplicarBotonPrimario(cerrar);
            pie.Controls.Add(cerrar);
            CancelButton = cerrar;
            Controls.Add(cuerpo);
            Controls.Add(pie);
        }

        private void AgregarBloque(Control control)
        {
            control.Dock = DockStyle.None;
            control.AutoSize = false;
            contenido.Controls.Add(control);
            AjustarContenido();
        }

        private static int UbicarTexto(Control texto, int x, int y, int ancho)
        {
            ancho = Math.Max(1, ancho);
            texto.AutoSize = false;
            texto.Dock = DockStyle.None;
            int alto = TextRenderer.MeasureText(texto.Text, texto.Font,
                new Size(ancho, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
            texto.SetBounds(x, y, ancho, alto);
            return alto;
        }

        private void AjustarContenido()
        {
            if (ajustando || contenido.Width <= 0) return;
            ajustando = true;
            try
            {
                // Medir al ancho real y sin límite de alto evita recortes en descripciones extensas.
                // Repetir si la aparición de la barra vertical cambia el ancho disponible.
                for (int intento = 0; intento < 3; intento++)
                {
                    int ancho = contenido.ClientSize.Width, y = 0;
                    foreach (Control bloque in contenido.Controls)
                    {
                        if (bloque is Label)
                            y += UbicarTexto(bloque, 0, y, ancho) + bloque.Margin.Bottom;
                        else
                        {
                            int interior = Math.Max(1, ancho - 32);
                            int izquierda = (int)(interior * 0.36);
                            int filaY = 16 + UbicarTexto(bloque.Controls[0], 16, 16, interior) + 14;
                            for (int i = 1; i < bloque.Controls.Count; i += 2)
                            {
                                int h1 = UbicarTexto(bloque.Controls[i], 16, filaY, izquierda - 12);
                                int h2 = UbicarTexto(bloque.Controls[i + 1], 16 + izquierda, filaY, interior - izquierda);
                                filaY += Math.Max(h1, h2) + 12;
                            }
                            bloque.SetBounds(0, y, ancho, filaY + 4);
                            y += bloque.Height + 16;
                        }
                    }
                    contenido.Height = y + 16;
                    if (contenido.ClientSize.Width == ancho) break;
                }
            }
            finally { ajustando = false; }
        }

        public void AgregarSeccion(string titulo, params string[] campos)
        {
            var seccion = new Panel { BackColor = EstiloPos.Surface };
            var encabezado = new Label { Text = titulo, AutoSize = true, Dock = DockStyle.Top,
                Font = EstiloPos.FontLabel, ForeColor = EstiloPos.Ink1, Margin = new Padding(0, 0, 0, 14) };
            seccion.Controls.Add(encabezado);
            for (int i = 0; i < campos.Length; i += 2)
            {
                seccion.Controls.Add(new Label { Text = campos[i], AutoSize = true, Dock = DockStyle.Top,
                    Font = EstiloPos.FontSmall, ForeColor = EstiloPos.Ink2 });
                seccion.Controls.Add(new Label { Text = string.IsNullOrWhiteSpace(campos[i + 1]) ? "—" : campos[i + 1],
                    AutoSize = true, Dock = DockStyle.Top, Font = EstiloPos.FontBody,
                    ForeColor = EstiloPos.Ink1 });
            }
            AgregarBloque(seccion);
        }
    }

    internal static class DetallesGrid
    {
        // La fila del evento conserva su objeto incluso después de ordenar el grid.
        public static void Conectar(Form propietario, DataGridView grid, Func<DataGridViewRow, Form> crear)
        {
            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count || grid.Rows[e.RowIndex].IsNewRow) return;
                try
                {
                    using (var detalle = crear(grid.Rows[e.RowIndex]))
                        if (detalle != null) detalle.ShowDialog(propietario);
                }
                catch (Exception ex) { Aviso.Error(propietario, Errores.Usuario(ex), "No se pudo abrir el detalle"); }
            };
        }

        private static string Pesos(decimal valor) => "$" + valor.ToString("N0");

        public static Form Producto(Producto p)
        {
            if (p == null) return null;
            var f = new FormDetalleRegistro("Detalle del producto", p.Nombre);
            f.AgregarSeccion("Identificación", "Código de barras", p.CodigoBarras, "Categoría", p.Categoria,
                "Unidad de venta", p.UnidadMedida, "Estado", p.Activo ? "Activo" : "Inactivo");
            f.AgregarSeccion("Inventario", "Stock actual", p.Stock.ToString("0.###") + " " + p.UnidadMedida,
                "Stock mínimo", p.StockMinimo.ToString("0.###") + " " + p.UnidadMedida);
            f.AgregarSeccion("Precio de venta", "Precio de lista", Pesos(p.Precio),
                "Descuento", p.DescuentoPorcentaje.ToString("0.##") + "%", "Precio final", Pesos(p.PrecioConDescuento));
            return f;
        }

        public static Form Rendimiento(ProductoVendido p, string periodo)
        {
            if (p == null) return null;
            var f = new FormDetalleRegistro("Detalle de ventas del producto", p.Nombre + "\n" + periodo);
            f.AgregarSeccion("Resultados del período", "Cantidad neta", p.Cantidad.ToString("0.###"),
                "Venta neta", Pesos(p.Total), "Costo neto", Pesos(p.Costo),
                "Costo reintegrado por devoluciones", Pesos(p.CostoDevuelto), "Ganancia después de devoluciones", Pesos(p.Utilidad));
            f.AgregarSeccion("Cómo leer los valores", "Cantidad y venta netas", "Descuentan las devoluciones del período.",
                "Ganancia", "Venta neta menos costo neto. No descuenta gastos del negocio.");
            return f;
        }

        public static Form Caja(Caja c, ResumenCaja r)
        {
            if (c == null) return null;
            bool cerrada = c.Estado == EstadoCaja.Cerrada;
            var f = new FormDetalleRegistro("Detalle de caja N° " + c.IdCaja, "Consulta del turno y su arqueo");
            f.AgregarSeccion("Turno", "Usuario", c.NombreUsuario, "Estado", c.Estado,
                "Apertura", c.FechaApertura.ToString("dd/MM/yyyy HH:mm"),
                "Cierre", c.FechaCierre?.ToString("dd/MM/yyyy HH:mm") ?? "Pendiente");
            f.AgregarSeccion("Ventas y movimientos", "Cantidad de ventas", r.CantidadVentas.ToString(),
                "Ventas en efectivo", Pesos(r.TotalEfectivo), "Ventas con tarjeta", Pesos(r.TotalTarjeta),
                "Ventas por transferencia", Pesos(r.TotalTransferencia), "Reembolsos en efectivo", Pesos(r.TotalDevoluciones),
                "Otras entradas al cajón", Pesos(r.OtrasEntradas), "Salidas del cajón (incluye gastos)", Pesos(r.OtrasSalidas),
                "Gastos del turno, todos los medios", Pesos(r.TotalGastos));
            decimal esperado = cerrada ? c.MontoEsperado : c.MontoInicial + r.TotalEfectivo - r.TotalDevoluciones + r.OtrasEntradas - r.OtrasSalidas;
            f.AgregarSeccion("Arqueo", "Fondo inicial", Pesos(c.MontoInicial),
                cerrada ? "Efectivo esperado al cierre" : "Efectivo esperado hasta ahora", Pesos(esperado),
                "Efectivo contado", cerrada ? Pesos(c.MontoReal) : "Pendiente de cierre",
                "Diferencia (contado − esperado)", cerrada ? Pesos(c.MontoReal - esperado) : "Pendiente de cierre");
            return f;
        }

        public static Form Movimiento(MovimientoCaja m)
        {
            if (m == null) return null;
            var f = new FormDetalleRegistro(m.Tipo + " N° " + m.IdMovimiento, "Detalle del movimiento · solo lectura");
            f.AgregarSeccion("Resumen", "Tipo", m.Tipo, "Monto registrado", Pesos(m.Monto),
                "Estado", m.Anulado ? "Anulado" : "Vigente", "Fecha", m.Fecha.ToString("dd/MM/yyyy HH:mm"), "Registrado por", m.NombreUsuario);
            f.AgregarSeccion("Concepto", "Categoría", m.Categoria, "Descripción", m.Descripcion,
                "Referencia", string.IsNullOrWhiteSpace(m.Referencia) ? "Sin referencia" : m.Referencia);
            f.AgregarSeccion("Pago y caja", "Medio de pago", m.MedioPago, "Origen del dinero", m.Origen,
                "Turno asociado", m.IdCaja.HasValue ? "Caja N° " + m.IdCaja.Value : "Sin turno asociado",
                "Efecto vigente en el cajón", Pesos(m.EfectoCaja), "Cómo se contabiliza",
                m.Anulado ? "El registro original se conserva, pero ya no afecta gastos ni efectivo."
                : m.Tipo == TipoMovimientoCaja.Gasto
                    ? (m.Origen == OrigenMovimientoCaja.Cajon ? "Gasto pagado desde el cajón: resta efectivo una sola vez." : "Gasto pagado fuera del cajón: no modifica su efectivo.")
                    : (m.Tipo == TipoMovimientoCaja.Reposicion ? "Suma efectivo al cajón; no es una venta ni un gasto." : "Resta efectivo del cajón; no es un gasto ni una venta."));
            if (m.Anulado)
                f.AgregarSeccion("Anulación", "Fecha", m.FechaAnulacion?.ToString("dd/MM/yyyy HH:mm"),
                    "Anulado por", m.NombreUsuarioAnulacion, "Motivo", m.MotivoAnulacion);
            return f;
        }

        public static Form Registro(DataGridViewRow fila)
        {
            var f = new FormDetalleRegistro("Detalle del registro", "Cambios de productos · solo lectura");
            f.AgregarSeccion("Registro", "Fecha", Convert.ToString(fila.Cells[0].Value),
                "Usuario", Convert.ToString(fila.Cells[1].Value), "Acción", Convert.ToString(fila.Cells[2].Value));
            f.AgregarSeccion("Detalle completo", "Descripción", Convert.ToString(fila.Cells[3].Value));
            return f;
        }
    }
}

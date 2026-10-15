using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entidades;

namespace Presentacion.Forms
{
    internal enum IndicadorReporte { Ventas, VentaNeta, Inventario, Margen, Iva, Ticket, InventarioCosto, Cobros, Utilidad, Productos }

    internal static class DetallesTarjetas
    {
        public static void Conectar(Form propietario, Control tarjeta, Func<Form> crear)
        {
            var ayuda = new ToolTip();
            tarjeta.Disposed += (s, e) => ayuda.Dispose();
            EventHandler abrir = (s, e) =>
            {
                try { using (var f = crear()) if (f != null) f.ShowDialog(propietario); }
                catch (Exception ex) { Aviso.Error(propietario, Errores.Usuario(ex), "No se pudo abrir el detalle"); }
            };
            ConectarHijos(tarjeta, abrir, ayuda);
        }

        private static void ConectarHijos(Control control, EventHandler abrir, ToolTip ayuda)
        {
            control.Cursor = Cursors.Hand;
            control.DoubleClick += abrir;
            ayuda.SetToolTip(control, "Doble clic para ver el detalle");
            foreach (Control hijo in control.Controls) ConectarHijos(hijo, abrir, ayuda);
        }

        private static string Pesos(decimal valor) => "$" + valor.ToString("N0");

        private static Form CrearLista(string titulo, string contexto, out DataGridView grid)
        {
            var f = new Form { Text = titulo, Size = new Size(960, 620), MinimumSize = new Size(640, 420),
                StartPosition = FormStartPosition.CenterParent, ShowIcon = false, ShowInTaskbar = false,
                MinimizeBox = false, BackColor = EstiloPos.Fondo, Padding = new Padding(20) };
            var encabezado = new Label { Text = contexto + "\nDoble clic en una fila para ver su detalle.",
                Dock = DockStyle.Top, Height = 78, Font = EstiloPos.FontBody, ForeColor = EstiloPos.Ink1 };
            var pie = new Panel { Dock = DockStyle.Bottom, Height = 54, Padding = new Padding(0, 12, 0, 0) };
            var cerrar = new Button { Text = "Cerrar", Dock = DockStyle.Right, Width = 120, DialogResult = DialogResult.Cancel };
            EstiloPos.AplicarBotonPrimario(cerrar);
            pie.Controls.Add(cerrar);
            f.CancelButton = cerrar;
            grid = new DataGridView { Dock = DockStyle.Fill };
            EstiloPos.AplicarGrid(grid);
            f.Controls.Add(grid);
            f.Controls.Add(pie);
            f.Controls.Add(encabezado);
            return f;
        }

        public static Form Ventas(string titulo, string contexto, List<Venta> ventas)
        {
            var f = CrearLista(titulo, contexto + "\n" + ventas.Count + " ventas · Total cobrado: " + Pesos(ventas.Sum(v => v.Total)), out var grid);
            foreach (string nombre in new[] { "N°", "Fecha", "Vendedor", "Medio de pago", "Total" })
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = nombre, ValueType = nombre == "Total" ? typeof(decimal) : typeof(string) });
            grid.Columns[4].DefaultCellStyle.Format = "$ #,##0";
            foreach (var v in ventas)
                grid.Rows[grid.Rows.Add("#" + v.IdVenta, v.Fecha.ToString("dd/MM/yyyy HH:mm"), v.NombreUsuario, v.MedioPago, v.Total)].Tag = v;
            DetallesGrid.Conectar(f, grid, fila => fila.Tag is Venta v ? new FormDetalleVenta(v) : null);
            return f;
        }

        public static Form Inventario(string titulo, List<Producto> productos, bool aCosto, bool bajoStock, bool incluirInactivos)
        {
            var lista = productos.Where(p => incluirInactivos || p.Activo).ToList();
            string contexto = bajoStock ? "Productos activos cuyo stock está en el mínimo o por debajo."
                : incluirInactivos ? "Catálogo: incluye productos activos e inactivos."
                : aCosto ? "Stock activo valorizado al costo de compra." : "Stock activo valorizado al precio de lista, sin descuentos.";
            contexto += "\n" + lista.Count + " productos";
            if (!bajoStock && !incluirInactivos) contexto += " · Total: " + Pesos(lista.Sum(p => p.Stock * (aCosto ? p.Costo : p.Precio)));
            var f = CrearLista(titulo, contexto, out var grid);
            foreach (string nombre in new[] { "Producto", "Stock", "Unidad", "Mínimo", "Estado" })
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = nombre });
            grid.Columns[0].FillWeight = 180;
            grid.Columns[1].DefaultCellStyle.Format = grid.Columns[3].DefaultCellStyle.Format = "0.###";
            if (!bajoStock && !incluirInactivos)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = aCosto ? "Costo unitario" : "Precio de lista" });
                grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Valor del stock" });
                grid.Columns[5].DefaultCellStyle.Format = grid.Columns[6].DefaultCellStyle.Format = "$ #,##0";
            }
            foreach (var p in lista)
            {
                var valores = new List<object> { p.Nombre, p.Stock, p.UnidadMedida, p.StockMinimo, p.Activo ? "Activo" : "Inactivo" };
                if (!bajoStock && !incluirInactivos) { decimal precio = aCosto ? p.Costo : p.Precio; valores.Add(precio); valores.Add(p.Stock * precio); }
                grid.Rows[grid.Rows.Add(valores.ToArray())].Tag = p;
            }
            DetallesGrid.Conectar(f, grid, fila => DetallesGrid.Producto(fila.Tag as Producto));
            return f;
        }

        public static Form Reporte(IndicadorReporte indicador, ResumenVentas r, string periodo, List<Venta> ventas, List<Producto> productos)
        {
            if (r == null) return null;
            if (indicador == IndicadorReporte.Ventas) return Ventas("Ventas del período", periodo, ventas);
            if (indicador == IndicadorReporte.Inventario || indicador == IndicadorReporte.InventarioCosto || indicador == IndicadorReporte.Productos)
                return Inventario(indicador == IndicadorReporte.InventarioCosto ? "Inventario a costo" : indicador == IndicadorReporte.Productos ? "Total productos" : "Inventario total",
                    productos, indicador == IndicadorReporte.InventarioCosto, false, indicador == IndicadorReporte.Productos);
            string titulo;
            string[] campos;
            switch (indicador)
            {
                case IndicadorReporte.VentaNeta:
                    titulo = "Venta neta";
                    campos = new[] { "Ventas cobradas", Pesos(r.TotalVendido), "Reembolsos del período", Pesos(r.TotalDevoluciones), "Venta neta", Pesos(r.TotalNeto), "Cálculo", "Ventas cobradas menos reembolsos. Puede incluir reembolsos de ventas anteriores." }; break;
                case IndicadorReporte.Utilidad:
                    titulo = "Ganancia después de devoluciones";
                    campos = new[] { "Venta neta", Pesos(r.TotalNeto), "Costo de lo vendido", Pesos(r.TotalCosto), "Costo reintegrado", Pesos(r.TotalCostoDevuelto), "Costo neto", Pesos(r.TotalCostoNeto), "Ganancia", Pesos(r.Utilidad), "Cálculo", "Venta neta menos costo neto. No descuenta gastos del negocio. Los productos sin costo registrado aportan costo cero." }; break;
                case IndicadorReporte.Margen:
                    titulo = "Margen sobre venta neta";
                    campos = new[] { "Ganancia", Pesos(r.Utilidad), "Venta neta", Pesos(r.TotalNeto), "Margen", r.TieneMargen ? r.MargenPorcentaje.ToString("0") + "%" : "No calculable", "Cálculo", "Ganancia ÷ venta neta × 100. No se calcula cuando la venta neta es cero o negativa; no descuenta gastos del negocio." }; break;
                case IndicadorReporte.Iva:
                    titulo = "IVA de ventas (bruto)";
                    campos = new[] { "Ventas con IVA", Pesos(r.TotalVendido), "Neto sin IVA", Pesos(Impuestos.Neto(r.TotalVendido)), "IVA incluido", Pesos(Impuestos.Iva(r.TotalVendido)), "Alcance", "Desglose del importe vendido antes de devoluciones. La venta neta de la tarjeta vecina sí descuenta reembolsos." }; break;
                case IndicadorReporte.Ticket:
                    titulo = "Ticket promedio";
                    campos = new[] { "Ventas cobradas", Pesos(r.TotalVendido), "Cantidad de ventas", r.CantidadVentas.ToString(), "Promedio por venta", Pesos(r.TicketPromedio), "Cálculo", "Total cobrado ÷ cantidad de ventas, antes de devoluciones. Sin ventas, el promedio es $0." }; break;
                default:
                    titulo = "Cobros / reembolsos";
                    campos = new[] { "Cobros en efectivo", Pesos(r.TotalEfectivo), "Cobros con tarjeta", Pesos(r.TotalTarjeta), "Cobros por transferencia", Pesos(r.TotalTransferencia), "Reembolsos", Pesos(r.TotalDevoluciones), "Efectivo después de reembolsos", Pesos(r.TotalEfectivo - r.TotalDevoluciones), "Alcance", "Los reembolsos salen del efectivo. Este valor no incluye fondo inicial, gastos, retiros ni reposiciones del cajón." }; break;
            }
            var detalle = new FormDetalleRegistro(titulo, periodo);
            detalle.AgregarSeccion("Desglose", campos);
            return detalle;
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Entidades;

namespace Presentacion
{
    // Una única superficie evita el repintado independiente de Labels/PictureBox transparentes.
    internal sealed class TarjetaProducto : Control
    {
        private static readonly Font FuenteNombre = new Font("Segoe UI", 11F, FontStyle.Bold);
        private static readonly Font FuentePrecio = new Font("Segoe UI", 14F, FontStyle.Bold);
        private static readonly Font FuenteStock = new Font("Segoe UI", 10F);
        private static readonly Font FuenteOriginal = new Font("Segoe UI", 9.5F, FontStyle.Strikeout);
        private static readonly Font FuenteOferta = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        private readonly Producto producto;
        private readonly Image foto; // Prestada por CacheFotosProducto; no se dispone aquí.
        private readonly bool bajoStock;
        private bool resaltada;
        private bool accionEnCurso;

        public int AltoPreferido => Escalar(214);

        public TarjetaProducto(Producto producto, Image foto, bool bajoStock)
        {
            this.producto = producto;
            this.foto = foto;
            this.bajoStock = bajoStock;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Tag = producto;
            Cursor = Cursors.Hand;
            TabStop = false;
            Margin = new Padding(0, 0, 8, 8);
            Size = new Size(220, 214);
            BackColor = FondoNormal;
            AccessibleRole = AccessibleRole.PushButton;
            AccessibleName = producto.Nombre + ", " + Precio;
        }

        private Color FondoNormal => bajoStock ? Color.FromArgb(255, 253, 245) : EstiloPos.Surface;
        private string Precio => "$" + producto.PrecioConDescuento.ToString("N0") +
            (producto.UnidadMedida == UnidadMedida.Kilogramo ? " / kg" : "");
        private int Escalar(int valor) => (int)Math.Round(valor * DeviceDpi / 96f);

        private void Resaltar(bool valor)
        {
            valor = valor && Enabled && !accionEnCurso;
            if (resaltada == valor) return;
            resaltada = valor;
            BackColor = valor ? (bajoStock ? Color.FromArgb(255, 246, 228) : EstiloPos.HoverFila) : FondoNormal;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); Resaltar(true); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); Resaltar(false); }
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled) Resaltar(false);
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible) Resaltar(false);
        }

        protected override void OnClick(EventArgs e)
        {
            if (accionEnCurso || !Enabled) return;
            accionEnCurso = true;
            Resaltar(false);
            Update(); // Quita el hover antes de abrir el diálogo modal.
            try { base.OnClick(e); }
            finally
            {
                accionEnCurso = false;
                // MouseLeave no está garantizado mientras un diálogo captura la interacción.
                if (!IsDisposed && !Disposing) Resaltar(false);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            int margen = Escalar(10), ancho = Math.Max(1, ClientSize.Width - margen * 2);
            var areaFoto = new Rectangle((Width - Escalar(80)) / 2, Escalar(8), Escalar(80), Escalar(80));
            if (foto == null)
                TextRenderer.DrawText(g, "Sin foto", FuenteStock, areaFoto, EstiloPos.Ink3,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            else
            {
                float factor = Math.Min((float)areaFoto.Width / foto.Width, (float)areaFoto.Height / foto.Height);
                int w = Math.Max(1, (int)(foto.Width * factor)), h = Math.Max(1, (int)(foto.Height * factor));
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(foto, areaFoto.Left + (areaFoto.Width - w) / 2, areaFoto.Top + (areaFoto.Height - h) / 2, w, h);
            }
            const TextFormatFlags texto = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(g, producto.Nombre, FuenteNombre,
                new Rectangle(margen, Escalar(94), ancho, Escalar(36)), EstiloPos.Ink1, texto | TextFormatFlags.WordBreak);
            TextRenderer.DrawText(g, Precio, FuentePrecio,
                new Rectangle(margen, Escalar(134), ancho, Escalar(26)),
                producto.TieneDescuento ? EstiloPos.Verde : EstiloPos.Ink1, texto | TextFormatFlags.SingleLine);
            TextRenderer.DrawText(g, (bajoStock ? "⚠ " : "") + producto.Stock.ToString("0.###") + " " + producto.UnidadMedida,
                FuenteStock, new Rectangle(margen, Escalar(186), ancho, Escalar(22)),
                bajoStock ? EstiloPos.Amber : EstiloPos.Ink3, texto | TextFormatFlags.SingleLine);
            if (producto.TieneDescuento)
            {
                TextRenderer.DrawText(g, "$" + producto.Precio.ToString("N0"), FuenteOriginal,
                    new Rectangle(margen, Escalar(162), ancho, Escalar(22)), EstiloPos.Ink3, texto | TextFormatFlags.SingleLine);
                var badge = new Rectangle(Width - Escalar(56), Escalar(8), Escalar(48), Escalar(20));
                using (var pincel = new SolidBrush(EstiloPos.Verde)) g.FillRectangle(pincel, badge);
                TextRenderer.DrawText(g, "-" + producto.DescuentoPorcentaje.ToString("0.##") + "%", FuenteOferta,
                    badge, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            using (var pen = new Pen(resaltada ? EstiloPos.Azul : (bajoStock ? EstiloPos.Amber : EstiloPos.Border),
                (resaltada ? 2f : 1.5f) * DeviceDpi / 96f))
                g.DrawRectangle(pen, 1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3));
        }
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace Presentacion
{
    /// <summary>
    /// Fuente única de verdad para colores, fuentes y tamaños del sistema POS.
    /// Cualquier cambio visual se hace AQUÍ — no en los Designers individuales.
    /// </summary>
    public static class EstiloPos
    {
        // ── Paleta de colores ─────────────────────────────────────────────

        // Fondos
        public static readonly Color Fondo      = Color.FromArgb(245, 244, 241); // página
        public static readonly Color Surface    = Color.FromArgb(255, 255, 255); // tarjetas/panels
        public static readonly Color Border     = Color.FromArgb(228, 228, 224); // bordes

        // Sidebar
        public static readonly Color Sidebar       = Color.FromArgb(20, 20, 25);
        public static readonly Color SidebarActivo = Color.FromArgb(35, 35, 46);
        public static readonly Color SidebarAcento = Color.FromArgb(37, 99, 235); // línea azul

        // Tipografía
        public static readonly Color Ink1 = Color.FromArgb(14,  14,  18);  // primario
        public static readonly Color Ink2 = Color.FromArgb(69,  69,  79);  // secundario
        public static readonly Color Ink3 = Color.FromArgb(143, 143, 154); // deshabilitado/hint

        // Semánticos (siempre los mismos en todos los módulos)
        public static readonly Color Verde       = Color.FromArgb(22,  163, 74);
        public static readonly Color VerdeBg     = Color.FromArgb(220, 252, 231);
        public static readonly Color Rojo        = Color.FromArgb(220, 38,  38);
        public static readonly Color RojoBg      = Color.FromArgb(254, 226, 226);
        public static readonly Color Amber       = Color.FromArgb(217, 119, 6);
        public static readonly Color AmberBg     = Color.FromArgb(254, 243, 199);
        public static readonly Color Azul        = Color.FromArgb(37,  99,  235);
        public static readonly Color AzulBg      = Color.FromArgb(239, 244, 254);

        // Resaltado al pasar el mouse (hover) sobre elementos claros — azulado, con buen contraste
        public static readonly Color Hover       = Color.FromArgb(196, 215, 245);
        public static readonly Color HoverFila   = Color.FromArgb(214, 229, 250);
        public static readonly Color Seleccion   = Color.FromArgb(184, 208, 246);

        // ── Tipografía ────────────────────────────────────────────────────

        public static Font FontTitulo     => new Font("Segoe UI", 19F, FontStyle.Bold);       // título de módulo
        public static Font FontSubtitulo  => new Font("Segoe UI", 15F, FontStyle.Bold);       // subtítulos / cards
        public static Font FontBody       => new Font("Segoe UI", 13F, FontStyle.Regular);    // cuerpo general
        public static Font FontSmall      => new Font("Segoe UI", 11F, FontStyle.Regular);    // hints / labels
        public static Font FontMicro      => new Font("Segoe UI", 10F, FontStyle.Regular);    // meta / columnas
        public static Font FontLabel      => new Font("Segoe UI", 11F, FontStyle.Bold);       // labels de campo
        public static Font FontBoton      => new Font("Segoe UI", 13F, FontStyle.Bold);       // botones
        public static Font FontBotonGrande=> new Font("Segoe UI", 15F, FontStyle.Bold);       // cobrar F12
        public static Font FontPrecio     => new Font("Segoe UI", 28F, FontStyle.Bold);       // total carrito
        public static Font FontInput      => new Font("Segoe UI", 13F, FontStyle.Regular);    // inputs estándar
        public static Font FontInputGrande=> new Font("Segoe UI", 15F, FontStyle.Regular);    // scanner / monto
        public static Font FontSidebar    => new Font("Segoe UI", 14F, FontStyle.Regular);    // botones sidebar
        public static Font FontMetrica    => new Font("Segoe UI", 23F, FontStyle.Bold);       // métricas caja
        public static Font FontTabla      => new Font("Segoe UI", 12.5F, FontStyle.Regular);  // filas tabla
        public static Font FontTablaHead  => new Font("Segoe UI", 11F, FontStyle.Bold);       // cabeceras tabla

        // ── Alturas estándar ─────────────────────────────────────────────
        public const int AlturaInput       = 38;
        public const int AlturaInputGrande = 46;
        public const int AlturaBoton       = 38;
        public const int AlturaBotonGrande = 50;
        public const int AlturaFilaTabla   = 38;
        public const int AlturaCabTabla    = 40;
        public const int AlturaTopbar      = 52;
        public const int AlturaSidebarBtn  = 58;

        // ── Helpers para aplicar estilos a controles ──────────────────────

        public static void AplicarInput(TextBox txt, bool grande = false)
        {
            txt.Font        = grande ? FontInputGrande : FontInput;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.BackColor   = Surface;
            txt.ForeColor   = Ink1;
            txt.Height      = grande ? AlturaInputGrande : AlturaInput;
        }

        public static void AplicarCombo(ComboBox combo)
        {
            combo.Font        = FontInput;
            combo.FlatStyle   = FlatStyle.Flat;
            combo.BackColor   = Surface;
            combo.ForeColor   = Ink1;
            combo.Height      = AlturaInput;
        }

        public static void AplicarBotonPrimario(Button btn, bool grande = false)
        {
            btn.Font                          = grande ? FontBotonGrande : FontBoton;
            btn.BackColor                     = Ink1;
            btn.ForeColor                     = Color.White;
            btn.FlatStyle                     = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize     = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 35, 40);
            btn.Cursor                        = Cursors.Hand;
            btn.UseVisualStyleBackColor       = false;
            btn.Height                        = grande ? AlturaBotonGrande : AlturaBoton;
        }

        public static void AplicarBotonSecundario(Button btn, Color? colorTexto = null)
        {
            btn.Font                          = FontBoton;
            btn.BackColor                     = Surface;
            btn.ForeColor                     = colorTexto ?? Ink2;
            btn.FlatStyle                     = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize     = 1;
            btn.FlatAppearance.BorderColor    = Border;
            btn.FlatAppearance.MouseOverBackColor = Hover;
            btn.Cursor                        = Cursors.Hand;
            btn.UseVisualStyleBackColor       = false;
            btn.Height                        = AlturaBoton;
        }

        public static void AplicarLabel(Label lbl, string texto,
            Font font = null, Color? color = null)
        {
            lbl.Text      = texto;
            lbl.Font      = font  ?? FontBody;
            lbl.ForeColor = color ?? Ink2;
            lbl.AutoSize  = true;
        }

        public static DataGridViewCellStyle EstiloEncabezadoTabla()
        {
            return new DataGridViewCellStyle
            {
                Font             = FontTablaHead,
                BackColor        = Color.FromArgb(240, 240, 236),
                ForeColor        = Ink2,
                Padding          = new Padding(10, 0, 0, 0),
                SelectionBackColor = Color.FromArgb(240, 240, 236),
                SelectionForeColor = Ink2
            };
        }

        public static DataGridViewCellStyle EstiloCeldaTabla()
        {
            return new DataGridViewCellStyle
            {
                Font             = FontTabla,
                BackColor        = Surface,
                ForeColor        = Ink1,
                Padding          = new Padding(10, 0, 0, 0),
                SelectionBackColor = Seleccion,
                SelectionForeColor = Ink1
            };
        }

        public static void AplicarGrid(DataGridView dgv)
        {
            dgv.Font                           = FontTabla;
            dgv.BackgroundColor                = Surface;
            dgv.BorderStyle                    = BorderStyle.None;
            dgv.GridColor                      = Border;
            dgv.EnableHeadersVisualStyles      = false;
            dgv.ColumnHeadersDefaultCellStyle  = EstiloEncabezadoTabla();
            dgv.DefaultCellStyle               = EstiloCeldaTabla();
            dgv.ColumnHeadersHeight            = AlturaCabTabla;
            dgv.ColumnHeadersHeightSizeMode    = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowTemplate.Height             = AlturaFilaTabla;
            dgv.RowHeadersVisible              = false;
            dgv.AllowUserToResizeColumns      = false;   // el usuario no puede mover/redimensionar columnas
            dgv.AllowUserToResizeRows         = false;
            dgv.AllowUserToOrderColumns       = false;
            dgv.AllowUserToAddRows             = false;
            dgv.AllowUserToDeleteRows          = false;
            dgv.ReadOnly                       = true;
            dgv.SelectionMode                  = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect                    = false;
            dgv.AutoSizeColumnsMode            = DataGridViewAutoSizeColumnsMode.Fill;

            // Resaltado de fila al pasar el mouse (guarda el color base en Tag para restaurarlo)
            dgv.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgv.Rows[e.RowIndex];
                if (row.Selected) return;
                if (!(row.Tag is Color)) row.Tag = row.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.BackColor = HoverFila;
            };
            dgv.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgv.Rows[e.RowIndex];
                if (row.Tag is Color c) row.DefaultCellStyle.BackColor = c;
            };
        }
    }
}

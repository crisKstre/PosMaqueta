using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Presentacion
{
    public enum TipoAviso { Exito, Error, Advertencia, Info }

    /// <summary>
    /// Fachada para mostrar diálogos de mensaje cohesivos con el sistema de diseño (EstiloPos).
    /// Reemplaza los MessageBox nativos para mantener una estética unificada en todo el POS.
    ///
    ///   Aviso.Exito(this, "Venta registrada");
    ///   if (Aviso.Confirmar(this, "Se perderán los datos.", "¿Cancelar?", "Sí, cancelar")) ...
    /// </summary>
    public static class Aviso
    {
        public static void Exito(IWin32Window owner, string mensaje, string titulo = "Listo")
            => FormMensaje.Mostrar(owner, TipoAviso.Exito, titulo, mensaje, null, null);

        public static void Error(IWin32Window owner, string mensaje, string titulo = "Ocurrió un problema")
            => FormMensaje.Mostrar(owner, TipoAviso.Error, titulo, mensaje, null, null);

        public static void Advertencia(IWin32Window owner, string mensaje, string titulo = "Atención")
            => FormMensaje.Mostrar(owner, TipoAviso.Advertencia, titulo, mensaje, null, null);

        public static void Info(IWin32Window owner, string mensaje, string titulo = "Información")
            => FormMensaje.Mostrar(owner, TipoAviso.Info, titulo, mensaje, null, null);

        /// <summary>Diálogo de confirmación. Devuelve true si el usuario confirma.</summary>
        public static bool Confirmar(IWin32Window owner, string mensaje, string titulo = "Confirmar",
            string textoConfirmar = "Sí", TipoAviso tipo = TipoAviso.Advertencia)
            => FormMensaje.Mostrar(owner, tipo, titulo, mensaje, textoConfirmar, "Cancelar") == DialogResult.Yes;
    }

    /// <summary>Diálogo modal estilizado. Privado al facade Aviso: se crea solo a través de él.</summary>
    internal class FormMensaje : Form
    {
        private FormMensaje(TipoAviso tipo, string titulo, string mensaje,
            string textoConfirmar, string textoCancelar)
        {
            ConstruirUI(tipo, titulo, mensaje, textoConfirmar, textoCancelar);
        }

        public static DialogResult Mostrar(IWin32Window owner, TipoAviso tipo, string titulo,
            string mensaje, string textoConfirmar, string textoCancelar)
        {
            using (var f = new FormMensaje(tipo, titulo, mensaje, textoConfirmar, textoCancelar))
                return f.ShowDialog(owner);
        }

        private void ConstruirUI(TipoAviso tipo, string titulo, string mensaje,
            string textoConfirmar, string textoCancelar)
        {
            Color acento  = ColorDe(tipo);
            Color acentoBg = FondoDe(tipo);
            string simbolo = SimboloDe(tipo);
            bool esConfirmacion = textoConfirmar != null;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.ShowIcon = false; this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = EstiloPos.Surface;
            this.Text = titulo;

            const int ancho = 440;
            const int margen = 32;
            int anchoContenido = ancho - margen * 2;

            // Ícono circular de color con el símbolo del tipo
            var ico = new Panel
            {
                Size = new Size(64, 64),
                Location = new Point((ancho - 64) / 2, 28),
                BackColor = EstiloPos.Surface
            };
            ico.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(acentoBg))
                    e.Graphics.FillEllipse(br, 0, 0, 63, 63);
                using (var br = new SolidBrush(acento))
                using (var f = new Font("Segoe UI", 26F, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(simbolo, f, br, new RectangleF(0, 0, 64, 64), sf);
            };

            var lblTitulo = new Label
            {
                Text = titulo,
                Font = EstiloPos.FontSubtitulo,
                ForeColor = EstiloPos.Ink1,
                AutoSize = false,
                Size = new Size(anchoContenido, 30),
                Location = new Point(margen, 104),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Mensaje: se mide para ajustar el alto del diálogo al contenido
            Size medido = TextRenderer.MeasureText(mensaje, EstiloPos.FontBody,
                new Size(anchoContenido, 0),
                TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter);

            var lblMensaje = new Label
            {
                Text = mensaje,
                Font = EstiloPos.FontBody,
                ForeColor = EstiloPos.Ink2,
                AutoSize = false,
                Size = new Size(anchoContenido, Math.Max(22, medido.Height)),
                Location = new Point(margen, 142),
                TextAlign = ContentAlignment.TopCenter
            };

            int yBotones = lblMensaje.Bottom + 26;
            const int altoBtn = 46;

            this.Controls.Add(ico);
            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblMensaje);

            if (esConfirmacion)
            {
                var btnCancelar  = CrearBoton(textoCancelar, false);
                var btnConfirmar = CrearBoton(textoConfirmar, true);
                int bw = 150, gap = 14;
                int x0 = (ancho - (bw * 2 + gap)) / 2;
                btnCancelar.Size  = new Size(bw, altoBtn); btnCancelar.Location  = new Point(x0, yBotones);
                btnConfirmar.Size = new Size(bw, altoBtn); btnConfirmar.Location = new Point(x0 + bw + gap, yBotones);
                btnCancelar.DialogResult  = DialogResult.No;
                btnConfirmar.DialogResult = DialogResult.Yes;
                this.AcceptButton = btnConfirmar;
                this.CancelButton = btnCancelar;
                this.Controls.Add(btnCancelar);
                this.Controls.Add(btnConfirmar);
            }
            else
            {
                var btnAceptar = CrearBoton("Aceptar", true);
                int bw = 160;
                btnAceptar.Size = new Size(bw, altoBtn);
                btnAceptar.Location = new Point((ancho - bw) / 2, yBotones);
                btnAceptar.DialogResult = DialogResult.OK;
                this.AcceptButton = btnAceptar;
                this.CancelButton = btnAceptar;
                this.Controls.Add(btnAceptar);
            }

            this.ClientSize = new Size(ancho, yBotones + altoBtn + 26);
        }

        private Button CrearBoton(string texto, bool primario)
        {
            var b = new Button
            {
                Text = texto,
                Font = EstiloPos.FontBoton,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            if (primario)
            {
                b.BackColor = EstiloPos.Ink1;
                b.ForeColor = Color.White;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 35, 40);
            }
            else
            {
                b.BackColor = EstiloPos.Surface;
                b.ForeColor = EstiloPos.Ink2;
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.BorderColor = EstiloPos.Border;
                b.FlatAppearance.MouseOverBackColor = EstiloPos.Fondo;
            }
            return b;
        }

        private static Color ColorDe(TipoAviso t)
        {
            switch (t)
            {
                case TipoAviso.Exito:       return EstiloPos.Verde;
                case TipoAviso.Error:       return EstiloPos.Rojo;
                case TipoAviso.Advertencia: return EstiloPos.Amber;
                default:                    return EstiloPos.Azul;
            }
        }

        private static Color FondoDe(TipoAviso t)
        {
            switch (t)
            {
                case TipoAviso.Exito:       return EstiloPos.VerdeBg;
                case TipoAviso.Error:       return EstiloPos.RojoBg;
                case TipoAviso.Advertencia: return EstiloPos.AmberBg;
                default:                    return EstiloPos.AzulBg;
            }
        }

        private static string SimboloDe(TipoAviso t)
        {
            switch (t)
            {
                case TipoAviso.Exito:       return "✓";
                case TipoAviso.Error:       return "✕";
                case TipoAviso.Advertencia: return "!";
                default:                    return "i";
            }
        }
    }
}

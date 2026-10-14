using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Dominio;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public sealed class FormFotoProducto : Form
    {
        private readonly Producto producto;
        private readonly FotoProductoService servicio = new FotoProductoService();
        private readonly PictureBox vista;
        private readonly Label sinFoto;
        private readonly Button guardar;
        private readonly Button quitar;
        private byte[] archivoPendiente;

        public FormFotoProducto(Producto producto)
        {
            this.producto = producto;
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Foto del producto";
            ClientSize = new Size(480, 530);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = ShowInTaskbar = ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = EstiloPos.Surface;
            Font = EstiloPos.FontLabel;

            var nombre = new Label { Text = producto.Nombre, AutoEllipsis = true, Dock = DockStyle.Top,
                Height = 52, Padding = new Padding(20, 12, 20, 4), Font = EstiloPos.FontSubtitulo };
            var panelVista = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            vista = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
            sinFoto = new Label { Text = "Sin foto", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = EstiloPos.Ink3, BackColor = EstiloPos.Fondo };
            panelVista.Controls.Add(vista);
            panelVista.Controls.Add(sinFoto);
            var ayuda = new Label { Text = "JPG, PNG o BMP · Hasta 10 MB.\nLa foto se guarda al pulsar Guardar.",
                Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(20, 4, 20, 0), Font = EstiloPos.FontSmall };
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 110, Padding = new Padding(18, 8, 18, 8) };
            var seleccionar = new Button { Text = "Elegir foto…", Size = new Size(210, 38), Visible = Sesion.EsAdmin };
            quitar = new Button { Text = "Quitar foto", Size = new Size(210, 38), Visible = Sesion.EsAdmin };
            guardar = new Button { Text = "Guardar", Size = new Size(210, 38), Enabled = false, Visible = Sesion.EsAdmin };
            var cancelar = new Button { Text = Sesion.EsAdmin ? "Cancelar" : "Cerrar", Size = new Size(210, 38), DialogResult = DialogResult.Cancel };
            EstiloPos.AplicarBotonSecundario(seleccionar);
            EstiloPos.AplicarBotonSecundario(quitar);
            EstiloPos.AplicarBotonPrimario(guardar);
            EstiloPos.AplicarBotonSecundario(cancelar);
            acciones.Controls.AddRange(new Control[] { seleccionar, quitar, guardar, cancelar });
            Controls.AddRange(new Control[] { panelVista, ayuda, acciones, nombre });
            seleccionar.Click += (s, e) => Elegir();
            quitar.Click += (s, e) => { archivoPendiente = null; Mostrar(null); guardar.Enabled = true; };
            guardar.Click += (s, e) => Guardar();
            AcceptButton = guardar;
            CancelButton = cancelar;
            Disposed += (s, e) => { vista.Image?.Dispose(); vista.Image = null; };
            Mostrar(servicio.ObtenerImagen(producto.IdProducto));
            ResumeLayout(true);
        }

        private void Mostrar(byte[] bytes)
        {
            var imagen = CacheFotosProducto.Decodificar(bytes);
            var anterior = vista.Image;
            vista.Image = imagen;
            anterior?.Dispose();
            vista.Visible = imagen != null;
            sinFoto.Visible = imagen == null;
            quitar.Enabled = bytes != null;
        }

        private void Elegir()
        {
            using (var dialogo = new OpenFileDialog { Filter = "Fotos (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp", CheckFileExists = true })
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    byte[] bytes;
                    using (var archivo = new FileStream(dialogo.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (archivo.Length == 0 || archivo.Length > ProcesadorFotoProducto.MaxArchivo)
                            throw new NegocioException("Selecciona una foto de hasta 10 MB.");
                        bytes = new byte[(int)archivo.Length];
                        int posicion = 0;
                        while (posicion < bytes.Length)
                        {
                            int leidos = archivo.Read(bytes, posicion, bytes.Length - posicion);
                            if (leidos == 0) throw new IOException("No se pudo leer la foto completa.");
                            posicion += leidos;
                        }
                    }
                    var preparada = ProcesadorFotoProducto.Preparar(bytes);
                    Mostrar(preparada.Imagen);
                    archivoPendiente = bytes;
                    guardar.Enabled = true;
                }
                catch (Exception ex) { Errores.Mostrar(this, ex); }
            }
        }

        private void Guardar()
        {
            try
            {
                if (archivoPendiente == null) servicio.Quitar(producto.IdProducto);
                else servicio.Guardar(producto.IdProducto, archivoPendiente);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { Errores.Mostrar(this, ex); }
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Entidades;

namespace Dominio
{
    // Conversión local de fotografías: no conserva rutas, originales ni metadatos.
    public static class ProcesadorFotoProducto
    {
        public const int MaxArchivo = 10 * 1024 * 1024;
        public const int MaxImagen = 256 * 1024;
        public const int MaxMiniatura = 24 * 1024;

        public static FotoProducto Preparar(byte[] archivo)
        {
            if (archivo == null || archivo.Length == 0 || archivo.Length > MaxArchivo)
                throw new NegocioException("Selecciona una foto JPG, PNG o BMP de hasta 10 MB.");
            try
            {
                using (var memoria = new MemoryStream(archivo, false))
                using (var imagen = Image.FromStream(memoria, true, true))
                {
                    if (imagen.RawFormat.Guid != ImageFormat.Jpeg.Guid && imagen.RawFormat.Guid != ImageFormat.Png.Guid &&
                        imagen.RawFormat.Guid != ImageFormat.Bmp.Guid)
                        throw new NegocioException("Formato no admitido. Usa JPG, PNG o BMP.");
                    if (imagen.Width > 10000 || imagen.Height > 10000 || (long)imagen.Width * imagen.Height > 24000000)
                        throw new NegocioException("La foto es demasiado grande. Usa una imagen de hasta 24 megapíxeles.");
                    Orientar(imagen);
                    var foto = new FotoProducto { Imagen = Reducir(imagen, 512), Miniatura = Reducir(imagen, 96) };
                    if (foto.Imagen.Length > MaxImagen || foto.Miniatura.Length > MaxMiniatura)
                        throw new NegocioException("No se pudo optimizar la foto. Selecciona otra imagen.");
                    return foto;
                }
            }
            catch (ArgumentException) { throw new NegocioException("El archivo no contiene una imagen válida."); }
            catch (OutOfMemoryException) { throw new NegocioException("No se pudo leer la foto. Puede estar dañada o ser demasiado grande."); }
        }

        private static void Orientar(Image imagen)
        {
            if (!imagen.PropertyIdList.Contains(0x112)) return;
            var valor = imagen.GetPropertyItem(0x112).Value;
            if (valor == null || valor.Length == 0) return;
            switch (valor[0])
            {
                case 2: imagen.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                case 3: imagen.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                case 4: imagen.RotateFlip(RotateFlipType.Rotate180FlipX); break;
                case 5: imagen.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                case 6: imagen.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                case 7: imagen.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                case 8: imagen.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
            }
        }

        private static byte[] Reducir(Image origen, int maximo)
        {
            double factor = Math.Min(1d, (double)maximo / Math.Max(origen.Width, origen.Height));
            int ancho = Math.Max(1, (int)Math.Round(origen.Width * factor));
            int alto = Math.Max(1, (int)Math.Round(origen.Height * factor));
            using (var destino = new Bitmap(ancho, alto, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(destino))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(origen, new Rectangle(0, 0, ancho, alto));
                }
                using (var memoria = new MemoryStream())
                using (var opciones = new EncoderParameters(1))
                {
                    opciones.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
                    var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    destino.Save(memoria, codec, opciones);
                    return memoria.ToArray();
                }
            }
        }
    }
}

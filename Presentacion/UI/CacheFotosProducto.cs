using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Dominio.Servicios;

namespace Presentacion
{
    // El dueño del cache dispone las imágenes. Las celdas solo las dibujan.
    internal sealed class CacheFotosProducto : IDisposable
    {
        private readonly Dictionary<int, Image> imagenes = new Dictionary<int, Image>();
        private readonly Queue<int> orden = new Queue<int>();
        private readonly FotoProductoService servicio = new FotoProductoService();
        private const int MaxEntradas = 128;

        public Image Obtener(int id)
        {
            if (imagenes.TryGetValue(id, out Image existente)) return existente;
            Image imagen = null;
            try { imagen = Decodificar(servicio.ObtenerMiniatura(id)); }
            catch (Exception ex) { AccesoData.Log.Error("No se pudo cargar la miniatura del producto " + id, ex); }
            if (imagenes.Count >= MaxEntradas)
            {
                int anterior = orden.Dequeue();
                imagenes[anterior]?.Dispose();
                imagenes.Remove(anterior);
            }
            imagenes.Add(id, imagen);
            orden.Enqueue(id);
            return imagen;
        }

        public static Image Decodificar(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0 || bytes.Length > Dominio.ProcesadorFotoProducto.MaxImagen) return null;
            try
            {
                using (var memoria = new MemoryStream(bytes, false))
                using (var imagen = Image.FromStream(memoria, true, true))
                {
                    if (imagen.Width > 512 || imagen.Height > 512) return null;
                    return new Bitmap(imagen); // Independiente del stream y del archivo de origen.
                }
            }
            catch (ArgumentException) { return null; }
            catch (OutOfMemoryException) { return null; }
        }

        public void Limpiar()
        {
            foreach (var imagen in imagenes.Values) imagen?.Dispose();
            imagenes.Clear();
            orden.Clear();
        }

        public void Dispose() => Limpiar();
    }
}

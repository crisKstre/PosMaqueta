using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AccesoData;
using AccesoData.DAO;
using Dominio.Eventos;
using Entidades;

namespace Dominio.Servicios
{
    /// <summary>
    /// Importación masiva de productos desde un archivo CSV (alta de catálogo inicial).
    /// Formato esperado (una fila por producto), con encabezado opcional:
    ///   CodigoBarras , Nombre , Categoria , Precio , Stock , StockMinimo , Unidad
    /// Detecta el separador (coma o punto y coma), admite campos entre comillas y crea las
    /// categorías nuevas que aparezcan. Las filas inválidas se omiten y se reportan.
    /// </summary>
    public class ImportacionService
    {
        private readonly ProductoDao     productoDao     = new ProductoDao();
        private readonly ProductoService productoService = new ProductoService();
        private readonly CategoriaDao    categoriaDao    = new CategoriaDao();
        private readonly LogService      logService      = new LogService();

        public ResultadoImportacion ImportarProductos(string rutaCsv)
        {
            Autorizacion.ExigirAdmin();
            if (string.IsNullOrWhiteSpace(rutaCsv) || !File.Exists(rutaCsv))
                throw new NegocioException("No se encontró el archivo CSV.");

            string[] lineas;
            try { lineas = File.ReadAllLines(rutaCsv); }
            catch (Exception ex) { throw new NegocioException("No se pudo leer el archivo: " + ex.Message); }

            var res = new ResultadoImportacion();
            char delim = DetectarDelimitador(lineas);
            bool primera = true;
            int n = 0;

            foreach (var cruda in lineas)
            {
                n++;
                if (string.IsNullOrWhiteSpace(cruda)) continue;
                var campos = ParsearLinea(cruda, delim);

                if (primera)
                {
                    primera = false;
                    if (EsEncabezado(campos)) continue;   // saltar títulos
                }

                try
                {
                    string errCampos;
                    var p = MapearProducto(campos, out errCampos);
                    if (errCampos != null) { res.Errores.Add("Línea " + n + ": " + errCampos); continue; }

                    // Match: por código de barras si lo trae; si no, por nombre exacto (así un
                    // producto sin código NO se duplica al reimportar). 5.c del roadmap.
                    Producto existente = !string.IsNullOrWhiteSpace(p.CodigoBarras)
                        ? productoDao.ObtenerPorCodigo(p.CodigoBarras)
                        : productoDao.ObtenerPorNombre(p.Nombre);
                    bool esNuevo = existente == null;
                    if (!esNuevo) p.IdProducto = existente.IdProducto;

                    string error = productoService.Validar(p, esNuevo);
                    if (error != null) { res.Errores.Add("Línea " + n + ": " + error); continue; }

                    EnsureCategoria(p.Categoria);
                    if (esNuevo) { productoDao.Insertar(p); res.Creados++; }
                    else { productoDao.Actualizar(p); res.Actualizados++; }
                }
                catch (Exception ex) { res.Errores.Add("Línea " + n + ": " + ex.Message); }
            }

            if (res.TotalCorrectos > 0)
                NotificadorCambios.Notificar(Entidad.Producto);

            Log.Info("Importación CSV (" + Path.GetFileName(rutaCsv) + "): " + res.Creados + " creados, " +
                     res.Actualizados + " actualizados, " + res.Errores.Count + " con error");
            logService.Registrar(ModuloLog.Productos, "Importación CSV",
                res.Creados + " creados, " + res.Actualizados + " actualizados, " + res.Errores.Count + " errores");
            return res;
        }

        // ── parsing ──────────────────────────────────────────────

        private static char DetectarDelimitador(string[] lineas)
        {
            foreach (var l in lineas)
                if (!string.IsNullOrWhiteSpace(l))
                    return l.Split(';').Length > l.Split(',').Length ? ';' : ',';
            return ',';
        }

        // Es encabezado si las columnas clave traen los TÍTULOS exactos (no un dato que los contenga,
        // p. ej. un producto llamado "Nuevo Nombre" no debe confundirse con la fila de títulos).
        private static bool EsEncabezado(List<string> c)
        {
            string col1 = c.Count > 1 ? c[1].Trim().ToLowerInvariant() : "";
            string col3 = c.Count > 3 ? c[3].Trim().ToLowerInvariant() : "";
            return col1 == "nombre" || col3 == "precio";
        }

        private static List<string> ParsearLinea(string linea, char delim)
        {
            var campos = new List<string>();
            var sb = new StringBuilder();
            bool enComillas = false;
            for (int i = 0; i < linea.Length; i++)
            {
                char ch = linea[i];
                if (enComillas)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < linea.Length && linea[i + 1] == '"') { sb.Append('"'); i++; }
                        else enComillas = false;
                    }
                    else sb.Append(ch);
                }
                else
                {
                    if (ch == '"') enComillas = true;
                    else if (ch == delim) { campos.Add(sb.ToString()); sb.Clear(); }
                    else sb.Append(ch);
                }
            }
            campos.Add(sb.ToString());
            return campos;
        }

        private Producto MapearProducto(List<string> c, out string error)
        {
            error = null;
            string Campo(int i) { return i < c.Count ? c[i].Trim() : ""; }

            string nombre = Campo(1);
            string sPrecio = Campo(3);

            if (string.IsNullOrWhiteSpace(nombre)) { error = "el nombre es obligatorio."; return null; }
            decimal precio;
            if (!TryDecimal(sPrecio, out precio)) { error = "precio inválido (\"" + sPrecio + "\")."; return null; }

            decimal stock = 0, stockMin = 0;
            string sStock = Campo(4), sStockMin = Campo(5);
            if (sStock.Length > 0 && !TryDecimal(sStock, out stock)) { error = "stock inválido (\"" + sStock + "\")."; return null; }
            if (sStockMin.Length > 0 && !TryDecimal(sStockMin, out stockMin)) { error = "stock mínimo inválido (\"" + sStockMin + "\")."; return null; }

            return new Producto
            {
                CodigoBarras = Campo(0),
                Nombre = nombre,
                Categoria = Campo(2),
                Precio = precio,
                Stock = stock,
                StockMinimo = stockMin,
                UnidadMedida = MapearUnidad(Campo(6)),
                DescuentoPorcentaje = 0
            };
        }

        private static string MapearUnidad(string u)
        {
            u = (u ?? "").Trim().ToLowerInvariant();
            return u.StartsWith("k") ? UnidadMedida.Kilogramo : UnidadMedida.Unidad;   // kg / kilo / kilogramo
        }

        private static bool TryDecimal(string s, out decimal d)
        {
            s = (s ?? "").Trim();
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out d)) return true;
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out d);
        }

        private void EnsureCategoria(string cat)
        {
            cat = (cat ?? "").Trim();
            if (cat.Length == 0) return;
            try { if (!categoriaDao.Existe(cat)) categoriaDao.Insertar(cat); }
            catch { /* duplicado / carrera: ignorar */ }
        }
    }
}

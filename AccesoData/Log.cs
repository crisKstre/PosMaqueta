using System;
using System.IO;

namespace AccesoData
{
    /// <summary>
    /// Logger técnico simple a archivo (un archivo por día en la carpeta "Logs").
    /// Sin dependencias externas. Nunca lanza: si falla el log, la app sigue.
    /// </summary>
    public static class Log
    {
        private static readonly object candado = new object();

        public static void Info(string mensaje)        => Escribir("INFO ", mensaje, null);
        public static void Advertencia(string mensaje)  => Escribir("WARN ", mensaje, null);
        public static void Error(string mensaje, Exception ex = null) => Escribir("ERROR", mensaje, ex);
        public static void Fatal(string mensaje, Exception ex = null) => Escribir("FATAL", mensaje, ex);

        private static void Escribir(string nivel, string mensaje, Exception ex)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                Directory.CreateDirectory(dir);
                string archivo = Path.Combine(dir, "pos-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

                string linea = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + nivel + "] " + mensaje;
                if (ex != null)
                    linea += Environment.NewLine + "    " + ex.GetType().Name + ": " + ex.Message +
                             Environment.NewLine + "    " + (ex.StackTrace ?? "");

                lock (candado)
                {
                    // FileShare.ReadWrite tolera que otra instancia/terminal escriba el mismo archivo;
                    // con un par de reintentos ante bloqueos transitorios para no perder líneas en silencio.
                    for (int intento = 0; ; intento++)
                    {
                        try
                        {
                            using (var fs = new FileStream(archivo, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            using (var sw = new StreamWriter(fs))
                                sw.WriteLine(linea);
                            break;
                        }
                        catch (IOException) when (intento < 3) { System.Threading.Thread.Sleep(15); }
                    }
                }
            }
            catch
            {
                // El logging nunca debe interrumpir la operación del POS.
            }
        }

        /// <summary>Borra archivos de log con más de 'dias' días de antigüedad.</summary>
        public static void LimpiarAntiguos(int dias)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(dir)) return;
                foreach (var f in Directory.GetFiles(dir, "pos-*.txt"))
                    if ((DateTime.Now - File.GetLastWriteTime(f)).TotalDays > dias)
                        File.Delete(f);
            }
            catch { }
        }
    }
}

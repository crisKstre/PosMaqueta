using System;

namespace Entidades
{
    /// <summary>Un archivo de respaldo de la base de datos, para mostrar y restaurar.</summary>
    public class RespaldoInfo
    {
        public string   Nombre { get; set; }
        public string   Ruta   { get; set; }
        public DateTime Fecha  { get; set; }
        public long     TamanoBytes { get; set; }
        public bool     Externo { get; set; }   // true = está en la carpeta externa (red/USB/nube)

        public string TamanoLegible
        {
            get
            {
                double kb = TamanoBytes / 1024.0;
                return kb < 1024 ? kb.ToString("0.#") + " KB" : (kb / 1024.0).ToString("0.#") + " MB";
            }
        }

        public string Ubicacion { get { return Externo ? "Externa" : "Local"; } }
    }
}

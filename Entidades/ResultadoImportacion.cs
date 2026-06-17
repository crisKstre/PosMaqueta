using System.Collections.Generic;

namespace Entidades
{
    /// <summary>Resumen de una importación de catálogo desde CSV.</summary>
    public class ResultadoImportacion
    {
        public int Creados { get; set; }
        public int Actualizados { get; set; }
        public List<string> Errores { get; } = new List<string>();

        public int TotalCorrectos { get { return Creados + Actualizados; } }
        public bool HuboErrores  { get { return Errores.Count > 0; } }
    }
}

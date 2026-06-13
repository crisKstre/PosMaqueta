using System;
using System.IO;

namespace AccesoData
{
    public static class ConfigBD
    {
        public static string RutaBaseDatos
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos.db"); }
        }

        public static string CadenaConexion
        {
            get { return "Data Source=" + RutaBaseDatos; }
        }
    }
}

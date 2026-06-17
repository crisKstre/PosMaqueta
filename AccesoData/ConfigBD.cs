using System;
using System.IO;

namespace AccesoData
{
    public static class ConfigBD
    {
        // Permite redirigir la BD (p. ej. a una base temporal en los tests). null = ubicación por defecto.
        private static string cadenaPersonalizada;

        public static string RutaBaseDatos
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos.db"); }
        }

        public static string CadenaConexion
        {
            get { return cadenaPersonalizada ?? "Data Source=" + RutaBaseDatos + ";Foreign Keys=True"; }
            set { cadenaPersonalizada = value; }
        }
    }
}

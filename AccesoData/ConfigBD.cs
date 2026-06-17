using System;
using System.IO;

namespace AccesoData
{
    public static class ConfigBD
    {
        // null = usar la cadena por defecto del proveedor. Se puede fijar (tests, config por caja).
        private static string cadenaPersonalizada;

        /// <summary>
        /// Motor activo. Por defecto SQLite (una caja). Para varias cajas, SQL Server
        /// (lo fija la instalación: Program.cs / config, o los tests).
        /// </summary>
        public static ProveedorBD Proveedor { get; set; } = ProveedorBD.Sqlite;

        public static string RutaBaseDatos
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos.db"); }
        }

        public static string CadenaConexion
        {
            get
            {
                if (cadenaPersonalizada != null) return cadenaPersonalizada;
                return Proveedor == ProveedorBD.SqlServer
                    ? "Server=(localdb)\\MSSQLLocalDB;Database=PosMaqueta;Integrated Security=true;TrustServerCertificate=true;"
                    : "Data Source=" + RutaBaseDatos + ";Foreign Keys=True";
            }
            set { cadenaPersonalizada = value; }
        }
    }
}

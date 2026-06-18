using System;
using System.Data.Common;
using System.Globalization;

namespace AccesoData
{
    /// <summary>
    /// Helpers para escribir los DAOs de forma agnóstica del proveedor (SQLite o SQL Server):
    /// crean comandos y parámetros sobre las clases base de ADO.NET (DbCommand/DbParameter),
    /// no sobre los tipos concretos de cada motor.
    /// </summary>
    internal static class Persistencia
    {
        public static DbCommand Comando(this DbConnection con, string sql, DbTransaction tran = null)
        {
            var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            if (tran != null) cmd.Transaction = tran;
            return cmd;
        }

        public static void AddParam(this DbCommand cmd, string nombre, object valor)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = nombre;
            p.Value = valor ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        // Lee una fecha guardada como texto ("yyyy-MM-dd HH:mm:ss") de forma robusta e independiente
        // de la cultura del equipo (cajas distintas pueden tener culturas distintas). 5.b del roadmap.
        public static DateTime LeerFecha(string texto)
        {
            if (DateTime.TryParseExact(texto, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var f))
                return f;
            return DateTime.Parse(texto, CultureInfo.InvariantCulture);   // tolerante a formatos antiguos
        }
    }

    /// <summary>SQL que difiere entre motores.</summary>
    internal static class Dialecto
    {
        public static bool EsSqlServer => ConfigBD.Proveedor == ProveedorBD.SqlServer;

        /// <summary>Devuelve el Id autogenerado del último INSERT (se agrega al final del INSERT).</summary>
        public static string UltimoId => EsSqlServer
            ? "SELECT CAST(SCOPE_IDENTITY() AS INT);"
            : "SELECT last_insert_rowid();";
    }
}

using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace AccesoData
{
    /// <summary>
    /// Respaldo automático de la base de datos. Crea una copia con fecha en la carpeta
    /// "Backups" (una por día) y conserva solo las más recientes.
    /// </summary>
    public static class RespaldoBD
    {
        public static void RespaldarSiCorresponde(int maxBackups = 15)
        {
            try
            {
                string db = ConfigBD.RutaBaseDatos;
                if (!File.Exists(db)) return;

                string dir = Path.Combine(Path.GetDirectoryName(db), "Backups");
                Directory.CreateDirectory(dir);

                // Solo un respaldo por día
                string hoy = DateTime.Now.ToString("yyyyMMdd");
                if (Directory.GetFiles(dir, "pos_" + hoy + "_*.db").Any()) return;

                string destino = Path.Combine(dir, "pos_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".db");

                // Con WAL hay transacciones en el archivo -wal: volcarlas al .db antes de copiar
                // para que el respaldo sea consistente (un simple File.Copy del .db las omitiría).
                using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
                {
                    con.Open();
                    using (var cmd = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE);", con))
                        cmd.ExecuteNonQuery();
                }
                File.Copy(db, destino, overwrite: true);
                Log.Info("Respaldo de base de datos creado: " + Path.GetFileName(destino));

                Rotar(dir, maxBackups);
            }
            catch (Exception ex)
            {
                Log.Error("No se pudo crear el respaldo de la base de datos", ex);
                // El respaldo nunca debe impedir que la aplicación arranque.
            }
        }

        // Conserva los 'max' respaldos más recientes y elimina el resto.
        private static void Rotar(string dir, int max)
        {
            var sobrantes = Directory.GetFiles(dir, "pos_*.db")
                .OrderByDescending(f => f)   // el nombre con timestamp ordena cronológicamente
                .Skip(max);
            foreach (var f in sobrantes)
            {
                try { File.Delete(f); } catch { /* ignorar */ }
            }
        }
    }
}

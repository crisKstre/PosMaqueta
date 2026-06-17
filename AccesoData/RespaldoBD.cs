using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entidades;
using Microsoft.Data.Sqlite;

namespace AccesoData
{
    /// <summary>
    /// Respaldo y restauración de la base de datos (SQLite). Crea copias con fecha en la carpeta
    /// "Backups" (junto al .db) y, si se configuró <see cref="ConfigBD.CarpetaRespaldoExterno"/>,
    /// también fuera del disco. En SQL Server el respaldo lo gestiona el servidor, no la aplicación.
    /// </summary>
    public static class RespaldoBD
    {
        /// <summary>¿El motor activo admite respaldo/restauración por archivo desde la app? Solo SQLite.</summary>
        public static bool SoportaArchivo { get { return ConfigBD.Proveedor == ProveedorBD.Sqlite; } }

        /// <summary>Respaldo automático diario (lo llama Program.cs al arrancar). Nunca lanza.</summary>
        public static void RespaldarSiCorresponde(int maxBackups = 15)
        {
            try
            {
                if (!SoportaArchivo) return;   // en SQL Server lo gestiona el servidor
                string db = RutaDb();
                if (!File.Exists(db)) return;

                string dir = CarpetaBackups(db);
                Directory.CreateDirectory(dir);

                string hoy = DateTime.Now.ToString("yyyyMMdd");
                if (Directory.GetFiles(dir, "pos_" + hoy + "_*.db").Any()) return;   // uno por día

                string destino = CrearCopia(db, dir);
                Log.Info("Respaldo automático creado: " + Path.GetFileName(destino));
                CopiarAExterno(destino);
                Rotar(dir, maxBackups);
            }
            catch (Exception ex)
            {
                Log.Error("No se pudo crear el respaldo automático", ex);
                // El respaldo nunca debe impedir que la aplicación arranque.
            }
        }

        /// <summary>Respaldo manual inmediato. Devuelve la ruta del archivo creado. Lanza si falla.</summary>
        public static string CrearRespaldo(int maxBackups = 15)
        {
            string db = RutaDb();
            if (!File.Exists(db))
                throw new InvalidOperationException("No se encontró la base de datos para respaldar.");

            string dir = CarpetaBackups(db);
            Directory.CreateDirectory(dir);
            string destino = CrearCopia(db, dir);
            Log.Info("Respaldo manual creado: " + Path.GetFileName(destino));
            CopiarAExterno(destino);
            Rotar(dir, maxBackups);
            return destino;
        }

        /// <summary>Respaldos disponibles (carpeta local + externa si está configurada), recientes primero.</summary>
        public static List<RespaldoInfo> Listar()
        {
            var lista = new List<RespaldoInfo>();
            if (!SoportaArchivo) return lista;

            AgregarDesde(lista, CarpetaBackups(RutaDb()), externo: false);
            if (!string.IsNullOrWhiteSpace(ConfigBD.CarpetaRespaldoExterno))
                AgregarDesde(lista, ConfigBD.CarpetaRespaldoExterno, externo: true);

            return lista.OrderByDescending(r => r.Fecha).ToList();
        }

        /// <summary>
        /// Restaura la base desde un respaldo (sobrescribe la actual). Antes guarda una copia
        /// de seguridad del estado vigente (.previo). La aplicación debe REINICIARSE después.
        /// </summary>
        public static void Restaurar(string rutaBackup)
        {
            if (!SoportaArchivo)
                throw new InvalidOperationException("La restauración por archivo solo está disponible con SQLite.");
            if (string.IsNullOrWhiteSpace(rutaBackup) || !File.Exists(rutaBackup))
                throw new InvalidOperationException("El archivo de respaldo no existe.");

            string db = RutaDb();
            SqliteConnection.ClearAllPools();   // libera los handles del archivo en uso

            if (File.Exists(db))
                File.Copy(db, db + ".previo", overwrite: true);   // por si hay que deshacer

            // Elimina los sidecar del WAL para que no se reapliquen sobre la base restaurada.
            foreach (var sc in new[] { db + "-wal", db + "-shm" })
                if (File.Exists(sc)) File.Delete(sc);

            File.Copy(rutaBackup, db, overwrite: true);
            Log.Advertencia("Base de datos RESTAURADA desde: " + rutaBackup);
        }

        public static string CarpetaLocal()
        {
            return SoportaArchivo ? CarpetaBackups(RutaDb()) : null;
        }

        // ── internos ─────────────────────────────────────────────

        // Deriva el archivo .db de la cadena activa (así también funciona contra la BD temporal de los tests).
        private static string RutaDb()
        {
            return new SqliteConnectionStringBuilder(ConfigBD.CadenaConexion).DataSource;
        }

        private static string CarpetaBackups(string db)
        {
            string dir = Path.GetDirectoryName(db);
            if (string.IsNullOrEmpty(dir)) dir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(dir, "Backups");
        }

        private static string CrearCopia(string db, string dir)
        {
            string destino = Path.Combine(dir, "pos_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".db");

            // Con WAL hay transacciones en el archivo -wal: volcarlas al .db antes de copiar para
            // que el respaldo sea consistente (un simple File.Copy del .db las omitiría).
            using (var con = new SqliteConnection(ConfigBD.CadenaConexion))
            {
                con.Open();
                using (var cmd = new SqliteCommand("PRAGMA wal_checkpoint(TRUNCATE);", con))
                    cmd.ExecuteNonQuery();
            }
            File.Copy(db, destino, overwrite: true);
            return destino;
        }

        private static void CopiarAExterno(string archivo)
        {
            string ext = ConfigBD.CarpetaRespaldoExterno;
            if (string.IsNullOrWhiteSpace(ext)) return;
            try
            {
                Directory.CreateDirectory(ext);
                File.Copy(archivo, Path.Combine(ext, Path.GetFileName(archivo)), overwrite: true);
                Log.Info("Respaldo copiado a carpeta externa: " + ext);
            }
            catch (Exception ex)
            {
                // El respaldo local ya quedó; no abortamos por un problema con la carpeta externa.
                Log.Error("No se pudo copiar el respaldo a la carpeta externa (" + ext + ")", ex);
            }
        }

        private static void AgregarDesde(List<RespaldoInfo> lista, string dir, bool externo)
        {
            if (!Directory.Exists(dir)) return;
            foreach (var f in Directory.GetFiles(dir, "pos_*.db"))
            {
                var fi = new FileInfo(f);
                lista.Add(new RespaldoInfo
                {
                    Nombre = fi.Name,
                    Ruta = fi.FullName,
                    Fecha = fi.LastWriteTime,
                    TamanoBytes = fi.Length,
                    Externo = externo
                });
            }
        }

        // Conserva los 'max' respaldos más recientes (por nombre con timestamp) y elimina el resto.
        private static void Rotar(string dir, int max)
        {
            var sobrantes = Directory.GetFiles(dir, "pos_*.db")
                .OrderByDescending(f => f)
                .Skip(max);
            foreach (var f in sobrantes)
                try { File.Delete(f); } catch { /* ignorar */ }
        }
    }
}

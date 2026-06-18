using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace AccesoData
{
    /// <summary>
    /// Envío asíncrono de fallos técnicos (ERROR/FATAL) a la SEDE (PosCentral.LogFallo) como
    /// telemetría de monitoreo. Es OPT-IN: sin cadena configurada queda inactivo. Nunca bloquea
    /// la caja (solo encola; la red va en un hilo de fondo) ni lanza. Tolerante a offline:
    /// persiste en una bandeja local (Logs/outbox-fallos.jsonl) y reintenta en cada tick.
    ///
    /// IMPORTANTE: esto es independiente de la BD operativa (ConfigBD). El servidor de la SEDE
    /// (telemetría) es distinto del servidor de la TIENDA (ventas/stock). La venta NO depende de esto.
    /// </summary>
    public static class LogRemoto
    {
        [DataContract]
        public sealed class EventoFallo
        {
            [DataMember] public string Tienda { get; set; }
            [DataMember] public string Caja { get; set; }
            [DataMember] public string Fecha { get; set; }      // "yyyy-MM-dd HH:mm:ss" (hora local de la caja)
            [DataMember] public string Nivel { get; set; }      // ERROR / FATAL
            [DataMember] public string Mensaje { get; set; }
            [DataMember] public string Excepcion { get; set; }  // tipo + mensaje + stack, o null
            [DataMember] public string Version { get; set; }
        }

        private static readonly object sync = new object();
        private static readonly List<EventoFallo> bandeja = new List<EventoFallo>();   // fuente de verdad
        private static readonly DataContractJsonSerializer serializador =
            new DataContractJsonSerializer(typeof(EventoFallo));

        private static bool activo;
        private static string tienda, caja, cadena, version;
        private static int nivelMinimo = 2;     // ERROR
        private static int maxEventos = 5000;
        private static string rutaOutbox;
        private static Timer worker;
        private static int enRonda;              // reentrancia del worker (Interlocked, no bool)

        // Evita que un Log.* hecho DENTRO de LogRemoto (p. ej. el aviso de bandeja llena, o un
        // fallo del propio envío) se re-encole y genere recursión/bucle. Por hilo.
        [ThreadStatic] private static bool enLogRemoto;

        /// <summary>Cantidad de fallos en la bandeja aún sin enviar a la sede.</summary>
        public static int PendientesDeEnvio { get { lock (sync) return bandeja.Count; } }

        /// <summary>
        /// Inicializa el envío (llamar una vez al arranque). Si cadenaConexion está vacía, queda
        /// inactivo (no-op). carpetaOutbox y capEventos son para pruebas; en producción se omiten.
        /// </summary>
        public static void Configurar(string tiendaId, string cajaId, string cadenaConexion,
            string versionApp, string nivelMin, string carpetaOutbox = null, int capEventos = 5000)
        {
            try
            {
                lock (sync)
                {
                    worker?.Dispose();
                    worker = null;
                    bandeja.Clear();
                    activo = false;

                    if (string.IsNullOrWhiteSpace(cadenaConexion)) return;   // desactivado (1 caja sin monitoreo)

                    tienda  = Recortar(string.IsNullOrWhiteSpace(tiendaId) ? Environment.MachineName : tiendaId, 60);
                    caja    = Recortar(string.IsNullOrWhiteSpace(cajaId)   ? Environment.MachineName : cajaId, 60);
                    cadena  = cadenaConexion;
                    version = Recortar(versionApp ?? "", 30);
                    nivelMinimo = RankNivel(nivelMin, 2);
                    maxEventos  = capEventos > 0 ? capEventos : 5000;

                    string dir = string.IsNullOrWhiteSpace(carpetaOutbox)
                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                        : carpetaOutbox;
                    Directory.CreateDirectory(dir);
                    rutaOutbox = Path.Combine(dir, "outbox-fallos.jsonl");

                    CargarOutbox();      // recupera pendientes de una sesión anterior
                    activo = true;
                    // Primer intento a los 20 s y luego cada 20 s (no bloquea el arranque).
                    worker = new Timer(_ => Ronda(), null, 20000, 20000);
                }
            }
            catch { activo = false; }   // el logging central jamás impide arrancar
        }

        /// <summary>Encola un fallo (lo llama Log.Escribir). No abre red, no bloquea, no lanza.</summary>
        public static void Encolar(string nivel, string mensaje, Exception ex)
        {
            if (!activo || enLogRemoto) return;
            enLogRemoto = true;
            try
            {
                if (RankNivel(nivel, -1) < nivelMinimo) return;   // por defecto solo ERROR/FATAL

                var ev = new EventoFallo
                {
                    Tienda = tienda,
                    Caja = caja,
                    Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Nivel = Recortar((nivel ?? "").Trim(), 10),
                    Mensaje = Recortar(mensaje ?? "", 1000),
                    Excepcion = ex == null ? null
                        : ex.GetType().Name + ": " + ex.Message + Environment.NewLine + (ex.StackTrace ?? ""),
                    Version = version
                };

                bool capeo = false;
                lock (sync)
                {
                    bandeja.Add(ev);
                    if (bandeja.Count > maxEventos)
                    {
                        bandeja.RemoveRange(0, bandeja.Count - maxEventos);   // descarta los más viejos
                        ReescribirOutbox();
                        capeo = true;
                    }
                    else AnexarOutbox(ev);   // append O(1)
                }
                if (capeo)   // fuera del lock; el guard enLogRemoto evita que esto se re-encole
                    Log.Advertencia("LogRemoto: bandeja llena (" + maxEventos + " máx), se descartan fallos antiguos sin enviar.");
            }
            catch { /* jamás propagar al POS */ }
            finally { enLogRemoto = false; }
        }

        /// <summary>
        /// Al cerrar la app: NO bloquea esperando la red. Los eventos ya están persistidos al
        /// encolar, así que solo asegura el archivo; se envían en el próximo arranque si hace falta.
        /// </summary>
        public static void Flush()
        {
            try { lock (sync) { if (activo) ReescribirOutbox(); } } catch { }
        }

        // ── worker de fondo (hilo del Timer) ─────────────────────

        private static void Ronda()
        {
            if (Interlocked.CompareExchange(ref enRonda, 1, 0) != 0) return;   // ya hay una ronda en curso
            enLogRemoto = true;   // nada de lo que loguee este hilo debe re-encolarse
            try
            {
                EventoFallo[] pendientes;
                lock (sync)
                {
                    if (!activo || bandeja.Count == 0) return;
                    pendientes = bandeja.ToArray();
                }

                // Envío FUERA del lock: la caja nunca espera por la red.
                var enviados = new List<EventoFallo>();
                try
                {
                    using (var con = new SqlConnection(cadena))
                    {
                        con.Open();
                        foreach (var ev in pendientes)
                        {
                            if (!Insertar(con, ev)) break;   // primer fallo: cortar la ronda (backoff al próximo tick)
                            enviados.Add(ev);
                        }
                    }
                }
                catch { /* sede/red caída: lo enviado cuenta; el resto reintenta solo */ }

                if (enviados.Count > 0)
                    lock (sync)
                    {
                        foreach (var ev in enviados) bandeja.Remove(ev);   // por referencia: exacto, sin carreras
                        ReescribirOutbox();
                    }
            }
            catch { /* el worker nunca propaga */ }
            finally
            {
                enLogRemoto = false;
                Interlocked.Exchange(ref enRonda, 0);
            }
        }

        private static bool Insertar(SqlConnection con, EventoFallo ev)
        {
            try
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO dbo.LogFallo (Tienda, Caja, FechaEvento, Nivel, Mensaje, Excepcion, Version) " +
                        "VALUES (@tienda, @caja, @fecha, @nivel, @mensaje, @excepcion, @version);";
                    Param(cmd, "@tienda", ev.Tienda);
                    Param(cmd, "@caja", ev.Caja);
                    Param(cmd, "@fecha", ParseFecha(ev.Fecha));
                    Param(cmd, "@nivel", ev.Nivel);
                    Param(cmd, "@mensaje", ev.Mensaje);
                    Param(cmd, "@excepcion", (object)ev.Excepcion ?? DBNull.Value);
                    Param(cmd, "@version", (object)ev.Version ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }   // este evento no se envió; se reintenta. No re-loguear (evita bucle).
        }

        private static void Param(SqlCommand cmd, string nombre, object valor)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = nombre;
            p.Value = valor ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static object ParseFecha(string s)
        {
            return DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime f) ? (object)f : DateTime.Now;
        }

        // ── bandeja en disco (todo bajo el lock 'sync') ──────────

        private static void CargarOutbox()
        {
            bandeja.Clear();
            if (!File.Exists(rutaOutbox)) return;
            foreach (var linea in File.ReadAllLines(rutaOutbox))
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;
                var ev = Deserializar(linea);
                if (ev != null) bandeja.Add(ev);     // las líneas corruptas se descartan en silencio
            }
            if (bandeja.Count > maxEventos)
                bandeja.RemoveRange(0, bandeja.Count - maxEventos);
        }

        private static void AnexarOutbox(EventoFallo ev)
        {
            try { File.AppendAllText(rutaOutbox, Serializar(ev) + "\n", Encoding.UTF8); } catch { }
        }

        private static void ReescribirOutbox()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var ev in bandeja) sb.Append(Serializar(ev)).Append('\n');
                File.WriteAllText(rutaOutbox, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        private static string Serializar(EventoFallo ev)
        {
            using (var ms = new MemoryStream())
            {
                serializador.WriteObject(ms, ev);   // JSON compacto en una sola línea
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static EventoFallo Deserializar(string linea)
        {
            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(linea)))
                    return (EventoFallo)serializador.ReadObject(ms);
            }
            catch { return null; }
        }

        private static int RankNivel(string nivel, int defecto)
        {
            switch ((nivel ?? "").Trim().ToUpperInvariant())
            {
                case "INFO":  return 0;
                case "WARN":  return 1;
                case "ERROR": return 2;
                case "FATAL": return 3;
                default:      return defecto;
            }
        }

        private static string Recortar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}

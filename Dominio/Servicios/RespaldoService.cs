using System;
using System.Collections.Generic;
using System.IO;
using AccesoData;
using Entidades;

namespace Dominio.Servicios
{
    /// <summary>
    /// Orquesta los respaldos de la base de datos: respaldo manual, listado y restauración,
    /// con validación de negocio (NegocioException) y auditoría. La copia automática diaria la
    /// dispara Program.cs vía RespaldoBD. Solo aplica a SQLite; en SQL Server lo gestiona el servidor.
    /// </summary>
    public class RespaldoService
    {
        private readonly LogService logService = new LogService();

        public bool   SoportaArchivo  { get { return RespaldoBD.SoportaArchivo; } }
        public string CarpetaLocal    { get { return RespaldoBD.CarpetaLocal(); } }
        public string CarpetaExterna  { get { return ConfigBD.CarpetaRespaldoExterno; } }

        public List<RespaldoInfo> Obtener()
        {
            return RespaldoBD.Listar();
        }

        // Crea un respaldo inmediato y devuelve su ruta.
        public string RespaldarAhora()
        {
            Autorizacion.ExigirAdmin();
            VerificarSoporte();
            try
            {
                string ruta = RespaldoBD.CrearRespaldo();
                logService.Registrar(ModuloLog.Respaldos, "Respaldo manual", Path.GetFileName(ruta));
                return ruta;
            }
            catch (NegocioException) { throw; }
            catch (Exception ex)
            {
                Log.Error("Falló el respaldo manual", ex);
                throw new NegocioException("No se pudo crear el respaldo. " + ex.Message);
            }
        }

        // Restaura desde un respaldo. Tras esto, la aplicación debe reiniciarse.
        public void Restaurar(string rutaBackup)
        {
            Autorizacion.ExigirAdmin();
            VerificarSoporte();
            try
            {
                RespaldoBD.Restaurar(rutaBackup);
                logService.Registrar(ModuloLog.Respaldos, "Restauración",
                    Path.GetFileName(rutaBackup ?? ""));
            }
            catch (NegocioException) { throw; }
            catch (Exception ex)
            {
                Log.Error("Falló la restauración", ex);
                throw new NegocioException("No se pudo restaurar la base de datos. " + ex.Message);
            }
        }

        private void VerificarSoporte()
        {
            if (!RespaldoBD.SoportaArchivo)
                throw new NegocioException(
                    "Con SQL Server los respaldos se gestionan en el servidor (ver la guía de despliegue).");
        }
    }
}

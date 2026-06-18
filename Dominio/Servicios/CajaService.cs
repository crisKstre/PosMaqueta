using System;
using AccesoData;
using AccesoData.DAO;
using Dominio.Eventos;
using Entidades;

namespace Dominio.Servicios
{
    public class CajaService
    {
        private readonly CajaDao cajaDao = new CajaDao();

        public Caja ObtenerCajaAbierta()
        {
            return cajaDao.ObtenerCajaAbierta();
        }

        public bool HayCajaAbierta()
        {
            return cajaDao.ObtenerCajaAbierta() != null;
        }

        public ResumenCaja ObtenerResumen(int idCaja)
        {
            return cajaDao.ObtenerResumen(idCaja);
        }

        public System.Collections.Generic.List<Caja> ObtenerHistorial(DateTime desde, DateTime hasta)
        {
            return cajaDao.ObtenerHistorial(desde, hasta);
        }

        // Efectivo que debería haber físicamente: fondo inicial + ventas en efectivo − devoluciones.
        public decimal CalcularEfectivoEsperado(Caja caja, ResumenCaja resumen)
        {
            return caja.MontoInicial + resumen.TotalEfectivo - resumen.TotalDevoluciones;
        }

        public int AbrirCaja(int idUsuario, decimal montoInicial)
        {
            if (HayCajaAbierta())
                throw new NegocioException("Ya hay una caja abierta. Ciérrala antes de abrir otra.");

            if (montoInicial < 0)
                throw new NegocioException("El monto inicial no puede ser negativo.");

            var caja = new Caja
            {
                IdUsuario = idUsuario,
                FechaApertura = DateTime.Now,
                MontoInicial = montoInicial,
                Estado = EstadoCaja.Abierta
            };

            int id = cajaDao.AbrirCaja(caja);
            Log.Info("Caja abierta N°" + id + " | usuario " + idUsuario + " | fondo inicial $" + montoInicial.ToString("N0"));
            NotificadorCambios.Notificar(Entidad.Caja);
            return id;
        }

        // Cierra la caja abierta. Devuelve la diferencia de arqueo
        // (montoReal - efectivoEsperado): positivo = sobrante, negativo = faltante.
        public decimal CerrarCaja(decimal montoReal)
        {
            var caja = cajaDao.ObtenerCajaAbierta();
            if (caja == null)
                throw new NegocioException("No hay ninguna caja abierta.");

            if (montoReal < 0)
                throw new NegocioException("El monto contado no puede ser negativo.");

            var resumen = cajaDao.ObtenerResumen(caja.IdCaja);
            decimal efectivoEsperado = CalcularEfectivoEsperado(caja, resumen);
            decimal diferencia = montoReal - efectivoEsperado;

            cajaDao.CerrarCaja(caja.IdCaja, efectivoEsperado, montoReal);
            Log.Info("Caja cerrada N°" + caja.IdCaja + " | esperado $" + efectivoEsperado.ToString("N0") +
                     " | contado $" + montoReal.ToString("N0") + " | diferencia $" + diferencia.ToString("N0") +
                     (diferencia < 0 ? " (FALTANTE)" : diferencia > 0 ? " (sobrante)" : " (cuadrada)"));
            NotificadorCambios.Notificar(Entidad.Caja);

            return diferencia;
        }
    }
}

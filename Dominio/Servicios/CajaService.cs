using System;
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

        // Efectivo que debería haber físicamente: fondo inicial + ventas en efectivo.
        public decimal CalcularEfectivoEsperado(Caja caja, ResumenCaja resumen)
        {
            return caja.MontoInicial + resumen.TotalEfectivo;
        }

        public int AbrirCaja(int idUsuario, decimal montoInicial)
        {
            if (HayCajaAbierta())
                throw new InvalidOperationException("Ya hay una caja abierta. Ciérrala antes de abrir otra.");

            if (montoInicial < 0)
                throw new InvalidOperationException("El monto inicial no puede ser negativo.");

            var caja = new Caja
            {
                IdUsuario = idUsuario,
                FechaApertura = DateTime.Now,
                MontoInicial = montoInicial,
                Estado = EstadoCaja.Abierta
            };

            int id = cajaDao.AbrirCaja(caja);
            NotificadorCambios.Notificar(Entidad.Caja);
            return id;
        }

        // Cierra la caja abierta. Devuelve la diferencia de arqueo
        // (montoReal - efectivoEsperado): positivo = sobrante, negativo = faltante.
        public decimal CerrarCaja(decimal montoReal)
        {
            var caja = cajaDao.ObtenerCajaAbierta();
            if (caja == null)
                throw new InvalidOperationException("No hay ninguna caja abierta.");

            if (montoReal < 0)
                throw new InvalidOperationException("El monto contado no puede ser negativo.");

            var resumen = cajaDao.ObtenerResumen(caja.IdCaja);
            decimal efectivoEsperado = CalcularEfectivoEsperado(caja, resumen);
            decimal diferencia = montoReal - efectivoEsperado;

            cajaDao.CerrarCaja(caja.IdCaja, efectivoEsperado, montoReal);
            NotificadorCambios.Notificar(Entidad.Caja);

            return diferencia;
        }
    }
}

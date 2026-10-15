using System;
using System.Collections.Generic;
using AccesoData.DAO;
using Dominio.Eventos;
using Entidades;

namespace Dominio.Servicios
{
    public sealed class MovimientoCajaService
    {
        private readonly MovimientoCajaDao dao = new MovimientoCajaDao();

        public List<MovimientoCaja> Obtener(DateTime desde, DateTime hasta)
        {
            if (Sesion.UsuarioActual == null) throw new NegocioException("Inicia sesión para consultar movimientos.");
            if (hasta.Date < desde.Date || hasta.Date == DateTime.MaxValue.Date)
                throw new NegocioException("Revisa el rango de fechas.");
            return dao.Obtener(desde, hasta);
        }

        public int Registrar(MovimientoCaja entrada)
        {
            Autorizacion.ExigirAdmin();
            if (entrada == null) throw new NegocioException("Completa el movimiento.");
            if (entrada.Monto <= 0 || entrada.Monto > 999999999999m || decimal.Truncate(entrada.Monto) != entrada.Monto)
                throw new NegocioException("Ingresa un monto positivo en pesos enteros (máximo $999.999.999.999).");
            if (entrada.Tipo != TipoMovimientoCaja.Gasto && entrada.Tipo != TipoMovimientoCaja.Retiro && entrada.Tipo != TipoMovimientoCaja.Reposicion)
                throw new NegocioException("Selecciona un tipo de movimiento válido.");
            if (entrada.Origen != OrigenMovimientoCaja.Cajon && entrada.Origen != OrigenMovimientoCaja.Externo)
                throw new NegocioException("Selecciona el origen del dinero.");
            if (entrada.MedioPago != MedioPago.Efectivo && entrada.MedioPago != MedioPago.Tarjeta && entrada.MedioPago != MedioPago.Transferencia)
                throw new NegocioException("Selecciona un medio de pago válido.");
            if (entrada.Origen == OrigenMovimientoCaja.Cajon && entrada.MedioPago != MedioPago.Efectivo)
                throw new NegocioException("Solo el efectivo puede entrar o salir del cajón.");
            if (entrada.Tipo != TipoMovimientoCaja.Gasto && entrada.Origen != OrigenMovimientoCaja.Cajon)
                throw new NegocioException("Los retiros y reposiciones corresponden a efectivo del cajón.");

            var caja = new CajaDao().ObtenerCajaAbierta();
            if (entrada.Origen == OrigenMovimientoCaja.Cajon && caja == null)
                throw new NegocioException("Abre una caja antes de registrar entradas o salidas de efectivo.");
            // El servicio asigna fecha, usuario y turno: no permite retrofechar ni usar un turno pasado.
            var m = new MovimientoCaja
            {
                Fecha = DateTime.Now, IdUsuario = Sesion.UsuarioActual.IdUsuario,
                IdCaja = caja?.IdCaja, Tipo = entrada.Tipo, Monto = entrada.Monto,
                MedioPago = entrada.MedioPago, Origen = entrada.Origen,
                Categoria = Texto(entrada.Categoria, "categoría", 80, entrada.Tipo == TipoMovimientoCaja.Gasto),
                Descripcion = Texto(entrada.Descripcion, "descripción", 300, true),
                Referencia = Texto(entrada.Referencia, "referencia", 120, false)
            };
            int id;
            try { id = dao.Registrar(m, Log("Registrar movimiento", m.Tipo + " | $" + m.Monto + " | " + m.Origen + " | " + m.Descripcion)); }
            catch (InvalidOperationException ex) { throw new NegocioException(ex.Message); }
            NotificadorCambios.Notificar(Entidad.Caja);
            return id;
        }

        public void Anular(int id, string motivo)
        {
            Autorizacion.ExigirAdmin();
            motivo = Texto(motivo, "motivo de anulación", 300, true);
            try { dao.Anular(id, motivo, Log("Anular movimiento", motivo)); }
            catch (InvalidOperationException ex) { throw new NegocioException(ex.Message); }
            NotificadorCambios.Notificar(Entidad.Caja);
        }

        private static string Texto(string valor, string nombre, int max, bool obligatorio)
        {
            valor = (valor ?? "").Trim();
            if ((obligatorio && valor.Length == 0) || valor.Length > max)
                throw new NegocioException("Revisa " + nombre + " (máximo " + max + " caracteres).");
            return valor;
        }

        private static LogMovimiento Log(string accion, string detalle) => new LogMovimiento
        {
            Fecha = DateTime.Now, IdUsuario = Sesion.UsuarioActual.IdUsuario,
            NombreUsuario = Sesion.UsuarioActual.Nombre, Accion = accion, Detalle = detalle
        };
    }
}

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using AccesoData.DAO;
using Entidades;

namespace Dominio.Servicios
{
    public sealed class ProveedorService
    {
        private readonly ProveedorDao dao = new ProveedorDao();
        private static void ExigirSesion() { if (Sesion.UsuarioActual == null) throw new NegocioException("Inicia sesión para consultar proveedores."); }
        public List<Proveedor> Obtener(string buscar = "", bool? activo = true)
        { ExigirSesion(); return dao.Obtener((buscar ?? "").Trim(), activo); }
        public Proveedor ObtenerPorId(int id) { ExigirSesion(); return dao.ObtenerPorId(id); }

        public int Crear(Proveedor entrada) => Guardar(entrada, true);
        public void Actualizar(Proveedor entrada) => Guardar(entrada, false);

        private int Guardar(Proveedor entrada, bool nuevo)
        {
            Autorizacion.ExigirAdmin();
            if (entrada == null) throw new NegocioException("Completa los datos del proveedor.");
            if (!nuevo && entrada.IdProveedor <= 0) throw new NegocioException("Selecciona un proveedor.");
            var p = new Proveedor { IdProveedor = entrada.IdProveedor,
                Nombre = Texto(entrada.Nombre, "nombre o razón social", 120, true), Rut = NormalizarRut(entrada.Rut),
                Contacto = Texto(entrada.Contacto, "contacto", 120), Telefono = Texto(entrada.Telefono, "teléfono", 40),
                Correo = Texto(entrada.Correo, "correo", 160), Direccion = Texto(entrada.Direccion, "dirección", 240),
                Notas = Texto(entrada.Notas, "notas", 1000) };
            if (p.Correo.Length > 0)
            {
                try { if (new MailAddress(p.Correo).Address != p.Correo) throw new FormatException(); }
                catch (FormatException) { throw new NegocioException("Revisa el formato del correo."); }
            }
            try { return dao.Guardar(p, nuevo, Auditoria(nuevo ? "Crear proveedor" : "Editar proveedor", p.Nombre)); }
            catch (InvalidOperationException ex) { throw new NegocioException(ex.Message); }
        }

        public void CambiarEstado(int id, bool activo)
        {
            Autorizacion.ExigirAdmin();
            try { dao.CambiarEstado(id, activo, Auditoria(activo ? "Reactivar proveedor" : "Desactivar proveedor", activo ? "Activo" : "Inactivo")); }
            catch (InvalidOperationException ex) { throw new NegocioException(ex.Message); }
        }

        private static string Texto(string valor, string campo, int max, bool requerido = false)
        {
            valor = (valor ?? "").Trim();
            if ((requerido && valor.Length == 0) || valor.Length > max) throw new NegocioException("Revisa " + campo + " (máximo " + max + " caracteres).");
            return valor;
        }

        private static string NormalizarRut(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return "";
            var rut = valor.Replace(".", "").Replace("-", "").Replace(" ", "").Trim().ToUpperInvariant();
            if (!Regex.IsMatch(rut, "^[0-9]{1,8}[0-9K]$")) throw new NegocioException("Revisa el RUT y su dígito verificador, o déjalo vacío.");
            var cuerpo = rut.Substring(0, rut.Length - 1).TrimStart('0');
            int suma = 0, factor = 2;
            for (int i = cuerpo.Length - 1; i >= 0; i--) { suma += (cuerpo[i] - '0') * factor; factor = factor == 7 ? 2 : factor + 1; }
            int resultado = 11 - suma % 11;
            string dv = resultado == 11 ? "0" : resultado == 10 ? "K" : resultado.ToString();
            if (cuerpo.Length == 0 || dv != rut.Substring(rut.Length - 1)) throw new NegocioException("El dígito verificador del RUT no coincide.");
            return cuerpo + "-" + dv;
        }

        private static LogMovimiento Auditoria(string accion, string detalle) => new LogMovimiento
        { Fecha = DateTime.Now, IdUsuario = Sesion.UsuarioActual.IdUsuario, NombreUsuario = Sesion.UsuarioActual.Nombre, Accion = accion, Detalle = detalle };
    }
}

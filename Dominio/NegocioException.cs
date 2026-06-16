using System;

namespace Dominio
{
    /// <summary>
    /// Error de regla de negocio esperado: stock insuficiente, caja ya abierta,
    /// datos inválidos, etc. Su mensaje está redactado para mostrarse directamente
    /// al usuario. No representa un fallo del sistema (a diferencia de una excepción
    /// técnica de SQLite o de E/S), por lo que no se registra como error.
    /// </summary>
    public class NegocioException : Exception
    {
        public NegocioException(string mensaje) : base(mensaje) { }
        public NegocioException(string mensaje, Exception interna) : base(mensaje, interna) { }
    }
}

using System;

namespace Entidades
{
    /// <summary>
    /// Cálculo de IVA. En este sistema los precios YA incluyen IVA (modelo retail chileno):
    /// el precio de góndola es el final al público y se desglosa el neto + IVA contenido.
    /// </summary>
    public static class Impuestos
    {
        public const decimal TasaIva = 0.19m;   // IVA Chile = 19%

        /// <summary>Valor neto (sin IVA) contenido en un monto que ya incluye IVA. Redondeado a peso.</summary>
        public static decimal Neto(decimal totalConIva)
            => Math.Round(totalConIva / (1m + TasaIva), 0, MidpointRounding.AwayFromZero);

        /// <summary>IVA contenido en un monto que ya incluye IVA (total − neto, así siempre cuadra).</summary>
        public static decimal Iva(decimal totalConIva)
            => totalConIva - Neto(totalConIva);
    }
}

using System;

namespace Entidades
{
    /// <summary>
    /// Redondeo de dinero a peso chileno ENTERO (sin decimales). Se aplica a cada subtotal y total
    /// para que nada persista con fracciones de peso y el arqueo/reportes cuadren al peso, incluso
    /// vendiendo por kilo (cantidades fraccionarias, p. ej. pan o verduras). 0.C del roadmap.
    /// </summary>
    public static class Dinero
    {
        public static decimal Redondear(decimal monto)
        {
            return Math.Round(monto, 0, MidpointRounding.AwayFromZero);
        }
    }
}

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Entidades;

namespace Presentacion
{
    // Coma o punto decimal; nunca separadores de miles ni un valor implícito de 1.
    internal static class EntradaCantidad
    {
        internal static bool TryParse(string texto, bool porPeso, out decimal cantidad)
        {
            cantidad = 0m;
            texto = (texto ?? "").Trim();
            if (texto.Length > 32 || !Regex.IsMatch(texto,
                porPeso ? @"\A[0-9]+(?:[.,][0-9]{1,3})?\z" : @"\A[0-9]+\z")) return false;
            return decimal.TryParse(texto.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out cantidad) && cantidad > 0m;
        }

        internal static bool TryCalcular(string texto, bool porPeso, bool enGramos,
            decimal disponible, decimal precio, out decimal cantidad, out decimal subtotal, out string error)
        {
            cantidad = subtotal = 0m;
            error = null;
            if (!TryParse(texto, porPeso && !enGramos, out decimal entrada))
            {
                error = porPeso && !enGramos
                    ? "Ingresa kg con hasta 3 decimales (ej.: 0,350). Sin separadores de miles."
                    : "Ingresa una cantidad entera mayor que cero. Sin separadores de miles.";
                return false;
            }
            cantidad = porPeso && enGramos ? entrada / 1000m : entrada;
            if (cantidad > disponible)
            {
                error = "Stock disponible: " + disponible.ToString("0.###") + (porPeso ? " kg." : " unidades.");
                return false;
            }
            try { subtotal = Dinero.Redondear(cantidad * precio); }
            catch (OverflowException)
            {
                error = "La cantidad o el importe es demasiado grande.";
                return false;
            }
            return true;
        }
    }
}

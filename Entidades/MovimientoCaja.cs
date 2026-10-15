using System;

namespace Entidades
{
    public static class TipoMovimientoCaja
    {
        public const string Gasto = "Gasto";
        public const string Retiro = "Retiro";
        public const string Reposicion = "Reposición";
    }

    public static class OrigenMovimientoCaja
    {
        public const string Cajon = "Cajón";
        public const string Externo = "Fuera del cajón";
    }

    // Un gasto pagado y su efecto en caja son un único registro: nunca dos salidas.
    public sealed class MovimientoCaja
    {
        public int IdMovimiento { get; set; }
        public int? IdCaja { get; set; }
        public DateTime Fecha { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Tipo { get; set; }
        public string Categoria { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public string MedioPago { get; set; }
        public string Origen { get; set; }
        public string Referencia { get; set; }
        public bool Anulado { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public int? IdUsuarioAnulacion { get; set; }
        public string NombreUsuarioAnulacion { get; set; }
        public string MotivoAnulacion { get; set; }
        public decimal EfectoCaja => Anulado || Origen != OrigenMovimientoCaja.Cajon
            ? 0m : Tipo == TipoMovimientoCaja.Reposicion ? Monto : -Monto;
    }
}

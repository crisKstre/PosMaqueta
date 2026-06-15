using System;

namespace Entidades
{
    public class Caja
    {
        public int IdCaja { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal MontoEsperado { get; set; }
        public decimal MontoReal { get; set; }
        public string Estado { get; set; }
        public string NombreUsuario { get; set; }   // solo para el histórico (no se persiste)
    }

    public static class EstadoCaja
    {
        public const string Abierta = "Abierta";
        public const string Cerrada = "Cerrada";
    }
}

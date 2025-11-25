namespace Ascensores.Models
{
    public class Ascensor
    {
        public int Id { get; set; } = 1; // único ascensor
        public int PisoActual { get; set; } = 1;
        public string Direccion { get; set; } = "DETENIDO"; // SUBIENDO, BAJANDO, DETENIDO
        public bool PuertasAbiertas { get; set; } = false;
        public bool EnMovimiento { get; set; } = false;
    }
}

namespace Ascensores.Models
{
    public class Solicitud
    {
        public int Id { get; set; }
        public int Piso { get; set; }
        public bool Atendida { get; set; }
        public DateTime FechaSolicitud { get; set; }
    }
}

namespace SGEB.DAL.Entities
{
    public class IncidenteUnidad
    {
        public int IdIncidenteUnidad { get; set; }
        public int IdIncidente { get; set; }
        public int IdUnidad { get; set; }
        public DateTime FechaHoraAsignacion { get; set; }
        public DateTime? FechaHoraLlegada { get; set; }
        public string Estado { get; set; } = "Asignada";

        // Vienen del JOIN con Unidad
        public string? CodigoUnidad { get; set; }
        public string? TipoUnidad { get; set; }
    }
}
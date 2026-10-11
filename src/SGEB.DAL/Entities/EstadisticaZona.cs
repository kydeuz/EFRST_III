namespace SGEB.DAL.Entities
{
    public class EstadisticaZona
    {
        public string Zona { get; set; } = string.Empty;
        public int TotalIncidentes { get; set; }
        public int Cerrados { get; set; }
    }
}
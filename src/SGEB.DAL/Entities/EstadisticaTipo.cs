namespace SGEB.DAL.Entities
{
    public class EstadisticaTipo
    {
        public string TipoIncidente { get; set; } = string.Empty;
        public int TotalIncidentes { get; set; }
        public int Cerrados { get; set; }
        public int SinAsignar { get; set; }
        public int? MinutosPromedioAtencion { get; set; }
    }
}
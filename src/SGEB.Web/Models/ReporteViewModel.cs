namespace SGEB.Web.Models
{
    public class EstadisticaTipoViewModel
    {
        public string TipoIncidente { get; set; } = string.Empty;
        public int TotalIncidentes { get; set; }
        public int Cerrados { get; set; }
        public int SinAsignar { get; set; }
        public int? MinutosPromedioAtencion { get; set; }
        public double PorcentajeCierre => TotalIncidentes == 0 ? 0 : Cerrados * 100.0 / TotalIncidentes;
    }

    public class EstadisticaZonaViewModel
    {
        public string Zona { get; set; } = string.Empty;
        public int TotalIncidentes { get; set; }
        public int Cerrados { get; set; }
        public double PorcentajeCierre => TotalIncidentes == 0 ? 0 : Cerrados * 100.0 / TotalIncidentes;
    }
}
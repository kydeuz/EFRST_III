namespace SGEB.Web.Models
{
    public class AtencionIncidenteViewModel
    {
        public int IdIncidente { get; set; }

        public string TipoIncidente { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Zona { get; set; } = string.Empty;

        public string Prioridad { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaHoraRegistro { get; set; }

        public string NombreReportante { get; set; } = string.Empty;
    }
}
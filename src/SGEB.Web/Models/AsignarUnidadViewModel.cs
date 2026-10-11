using Microsoft.AspNetCore.Mvc.Rendering;

namespace SGEB.Web.Models
{
    public class UnidadAsignadaItemViewModel
    {
        public int IdIncidenteUnidad { get; set; }
        public string CodigoUnidad { get; set; } = string.Empty;
        public string TipoUnidad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaHoraAsignacion { get; set; }
    }

    public class AsignarUnidadViewModel
    {
        public int IdIncidente { get; set; }
        public string TipoIncidente { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string EstadoIncidente { get; set; } = string.Empty;

        public List<UnidadAsignadaItemViewModel> Asignadas { get; set; } = new();
        public List<SelectListItem> UnidadesDisponibles { get; set; } = new();
    }
}
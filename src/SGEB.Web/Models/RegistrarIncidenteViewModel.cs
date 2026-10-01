using System.ComponentModel.DataAnnotations;

namespace SGEB.Web.Models
{
    public class RegistrarIncidenteViewModel
    {
        [Required(ErrorMessage = "Seleccione el tipo de incidente.")]
        [Display(Name = "Tipo de incidente")]
        public string TipoIncidente { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese una descripción.")]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la dirección.")]
        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la zona.")]
        [StringLength(100)]
        public string Zona { get; set; } = string.Empty;

        [Display(Name = "Latitud")]
        public decimal? Latitud { get; set; }

        [Display(Name = "Longitud")]
        public decimal? Longitud { get; set; }

        [Required]
        [Display(Name = "Prioridad")]
        public string Prioridad { get; set; } = "Media";

        // --- Datos del reportante (find-or-create por documento) ---

        [Required(ErrorMessage = "Seleccione el tipo de documento.")]
        [Display(Name = "Tipo de documento")]
        public string TipoDocumento { get; set; } = "DNI";

        [Required(ErrorMessage = "Ingrese el número de documento.")]
        [StringLength(20)]
        [Display(Name = "Número de documento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese el nombre del reportante.")]
        [StringLength(150)]
        [Display(Name = "Nombre del reportante")]
        public string NombreReportante { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Ingrese un teléfono válido.")]
        [Display(Name = "Teléfono del reportante")]
        public string? TelefonoReportante { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [Display(Name = "Correo del reportante")]
        public string? CorreoReportante { get; set; }
    }

    public class IncidenteListItemViewModel
    {
        public int IdIncidente { get; set; }
        public string TipoIncidente { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Zona { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaHoraRegistro { get; set; }
        public string NombreReportante { get; set; } = string.Empty;
    }
}
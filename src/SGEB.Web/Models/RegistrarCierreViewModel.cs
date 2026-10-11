using System.ComponentModel.DataAnnotations;

namespace SGEB.Web.Models
{
    public class RegistrarCierreViewModel
    {
        [Required]
        public int IdIncidente { get; set; }

        public string TipoIncidente { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Zona { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione un resultado de cierre.")]
        [Display(Name = "Resultado del cierre")]
        public string ResultadoCierre { get; set; } = string.Empty;

        [StringLength(
            500,
            ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        [Range(
            0,
            1000,
            ErrorMessage = "El personal involucrado no puede ser negativo.")]
        [Display(Name = "Personal involucrado")]
        public int? PersonalInvolucrado { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SGEB.Web.Models
{
    public class RegistrarUnidadViewModel
    {
        [Required(ErrorMessage = "Ingrese el código de la unidad.")]
        [StringLength(20)]
        [Display(Name = "Código de unidad")]
        public string CodigoUnidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione el tipo de unidad.")]
        [StringLength(50)]
        [Display(Name = "Tipo de unidad")]
        public string TipoUnidad { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Placa")]
        public string? Placa { get; set; }

        [Range(1, 100, ErrorMessage = "La capacidad debe estar entre 1 y 100.")]
        [Display(Name = "Capacidad (personal)")]
        public int? Capacidad { get; set; }

        [StringLength(100)]
        [Display(Name = "Estación")]
        public string? Estacion { get; set; }
    }

    public class UnidadListItemViewModel
    {
        public int IdUnidad { get; set; }
        public string CodigoUnidad { get; set; } = string.Empty;
        public string TipoUnidad { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public int? Capacidad { get; set; }
        public string Estacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }
}

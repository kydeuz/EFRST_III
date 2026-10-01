namespace SGEB.DAL.Entities
{

    /// Entidad de negocio Incidente. No tiene atributos de EF ni de validación:
    /// eso es responsabilidad de la capa Web (ViewModels) y del BLL (reglas).

    public class Incidente
    {
        public int IdIncidente { get; set; }
        public int IdReportante { get; set; }
        public string TipoIncidente { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Zona { get; set; } = string.Empty;
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public string Prioridad { get; set; } = "Media";
        public string Estado { get; set; } = "Registrado";
        public DateTime FechaHoraRegistro { get; set; }

    
        public string? NombreReportante { get; set; }
        public string? TelefonoReportante { get; set; }
    }
}
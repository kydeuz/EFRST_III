namespace SGEB.DAL.Entities
{
    public class Reportante
    {
        public int IdReportante { get; set; }
        public string TipoDocumento { get; set; } = "DNI";
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
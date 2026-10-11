namespace SGEB.DAL.Entities
{
    /// Entidad de negocio Unidad (autobombas, ambulancias, etc.).
    /// Sin atributos de EF ni de validación, igual que Incidente.
    public class Unidad
    {
        public int IdUnidad { get; set; }
        public string CodigoUnidad { get; set; } = string.Empty;
        public string TipoUnidad { get; set; } = string.Empty;
        public string? Placa { get; set; }
        public int? Capacidad { get; set; }
        public string? Estacion { get; set; }
        public string Estado { get; set; } = "Disponible";
        public DateTime FechaRegistro { get; set; }
    }
}

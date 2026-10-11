namespace SGEB.DAL.Entities
{
    // Clase mínima para el desplegable. La entidad Unidad completa es de la parte 1.
    public class UnidadDisponible
    {
        public int IdUnidad { get; set; }
        public string CodigoUnidad { get; set; } = string.Empty;
        public string TipoUnidad { get; set; } = string.Empty;
    }
}
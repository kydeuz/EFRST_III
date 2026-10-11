using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public interface IReporteRepository
    {
        List<EstadisticaTipo> ObtenerEstadisticasPorTipo();
        List<EstadisticaZona> ObtenerEstadisticasPorZona();
    }
}
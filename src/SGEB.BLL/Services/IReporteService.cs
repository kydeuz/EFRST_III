using SGEB.DAL.Entities;

namespace SGEB.BLL.Services
{
    public interface IReporteService
    {
        List<EstadisticaTipo> ObtenerEstadisticasPorTipo();
        List<EstadisticaZona> ObtenerEstadisticasPorZona();
    }
}
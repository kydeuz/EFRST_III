using SGEB.DAL.Entities;
using SGEB.DAL.Repositories;

namespace SGEB.BLL.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IReporteRepository _reporteRepository;

        public ReporteService(IReporteRepository reporteRepository)
        {
            _reporteRepository = reporteRepository;
        }

        public List<EstadisticaTipo> ObtenerEstadisticasPorTipo() => _reporteRepository.ObtenerEstadisticasPorTipo();
        public List<EstadisticaZona> ObtenerEstadisticasPorZona() => _reporteRepository.ObtenerEstadisticasPorZona();
    }
}
using SGEB.DAL.Entities;
using SGEB.DAL.Repositories;

namespace SGEB.BLL.Services
{
    public class ReportanteService : IReportanteService
    {
        private readonly IReportanteRepository _reportanteRepository;

        public ReportanteService(IReportanteRepository reportanteRepository)
        {
            _reportanteRepository = reportanteRepository;
        }

        public int ObtenerOCrear(Reportante reportante)
        {
            if (string.IsNullOrWhiteSpace(reportante.NumeroDocumento))
                throw new ArgumentException("El número de documento del reportante es obligatorio.");

            if (string.IsNullOrWhiteSpace(reportante.Nombre))
                throw new ArgumentException("El nombre del reportante es obligatorio.");

            var existente = _reportanteRepository.ObtenerPorDocumento(
                reportante.TipoDocumento, reportante.NumeroDocumento);

            if (existente != null)
                return existente.IdReportante;

            reportante.FechaRegistro = DateTime.Now;
            return _reportanteRepository.Registrar(reportante);
        }

        public List<Reportante> ObtenerTodos() => _reportanteRepository.Listar();

        public Reportante? ObtenerPorId(int idReportante) => _reportanteRepository.ObtenerPorId(idReportante);
    }
}
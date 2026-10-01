using SGEB.DAL.Entities;
using SGEB.DAL.Repositories;

namespace SGEB.BLL.Services
{
  
    /// Reglas de negocio de Incidente. Orquesta con IReportanteService porque
    /// registrar un incidente implica resolver "quién reporta" primero
    /// ; eso no es responsabilidad del Repository.
 
    public class IncidenteService : IIncidenteService
    {
        private readonly IIncidenteRepository _incidenteRepository;
        private readonly IReportanteService _reportanteService;

        public IncidenteService(
            IIncidenteRepository incidenteRepository,
            IReportanteService reportanteService)
        {
            _incidenteRepository = incidenteRepository;
            _reportanteService = reportanteService;
        }

        public int RegistrarIncidente(Incidente incidente, Reportante reportante)
        {
            if (string.IsNullOrWhiteSpace(incidente.Descripcion))
                throw new ArgumentException("La descripción del incidente es obligatoria.");

            var prioridadesValidas = new[] { "Alta", "Media", "Baja" };
            if (!prioridadesValidas.Contains(incidente.Prioridad))
                incidente.Prioridad = "Media";

            // Resuelve (o crea) al reportante antes de insertar el incidente.
            var idReportante = _reportanteService.ObtenerOCrear(reportante);

            incidente.IdReportante = idReportante;
            incidente.Estado = "Registrado";
            incidente.FechaHoraRegistro = DateTime.Now;

            return _incidenteRepository.Registrar(incidente);
        }

        public List<Incidente> ObtenerTodos() => _incidenteRepository.Listar();

        public Incidente? ObtenerPorId(int idIncidente) => _incidenteRepository.ObtenerPorId(idIncidente);
    }
}
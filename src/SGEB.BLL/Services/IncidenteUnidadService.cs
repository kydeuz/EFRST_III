using SGEB.DAL.Entities;
using SGEB.DAL.Repositories;

namespace SGEB.BLL.Services
{
    // Solo deja pasar las llamadas: el SP ya valida las reglas.
    public class IncidenteUnidadService : IIncidenteUnidadService
    {
        private readonly IIncidenteUnidadRepository _repository;

        public IncidenteUnidadService(IIncidenteUnidadRepository repository)
        {
            _repository = repository;
        }

        public int Asignar(int idIncidente, int idUnidad) => _repository.Asignar(idIncidente, idUnidad);

        public void Liberar(int idIncidenteUnidad) => _repository.Liberar(idIncidenteUnidad);

        public List<IncidenteUnidad> ListarPorIncidente(int idIncidente) => _repository.ListarPorIncidente(idIncidente);

        public List<UnidadDisponible> ListarUnidadesDisponibles() => _repository.ListarUnidadesDisponibles();
    }
}
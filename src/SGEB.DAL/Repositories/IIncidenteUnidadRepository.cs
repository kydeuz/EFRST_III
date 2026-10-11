using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public interface IIncidenteUnidadRepository
    {
        int Asignar(int idIncidente, int idUnidad);
        void Liberar(int idIncidenteUnidad);
        List<IncidenteUnidad> ListarPorIncidente(int idIncidente);
        List<UnidadDisponible> ListarUnidadesDisponibles();
    }
}
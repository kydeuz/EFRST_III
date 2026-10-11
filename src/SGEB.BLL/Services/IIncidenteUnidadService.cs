using SGEB.DAL.Entities;

namespace SGEB.BLL.Services
{
    public interface IIncidenteUnidadService
    {
        int Asignar(int idIncidente, int idUnidad);
        void Liberar(int idIncidenteUnidad);
        List<IncidenteUnidad> ListarPorIncidente(int idIncidente);
        List<UnidadDisponible> ListarUnidadesDisponibles();
    }
}
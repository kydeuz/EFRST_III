using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public interface IIncidenteRepository
    {
        int Registrar(Incidente incidente);
        List<Incidente> Listar();
        Incidente? ObtenerPorId(int idIncidente);
    }
}
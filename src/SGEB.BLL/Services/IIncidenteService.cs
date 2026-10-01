using SGEB.DAL.Entities;

namespace SGEB.BLL.Services
{
    public interface IIncidenteService
    {
        /// Registra el incidente. Internamente resuelve el reportante
        /// (lo busca por documento o lo crea si es nuevo) antes de insertar.
   
        int RegistrarIncidente(Incidente incidente, Reportante reportante);

        List<Incidente> ObtenerTodos();
        Incidente? ObtenerPorId(int idIncidente);
    }
}
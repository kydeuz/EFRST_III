using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public interface IUnidadRepository
    {
        int Registrar(Unidad unidad);
        List<Unidad> Listar();
        List<Unidad> ListarDisponibles();
        Unidad? ObtenerPorId(int idUnidad);
        void ActualizarEstado(int idUnidad, string nuevoEstado);
    }
}

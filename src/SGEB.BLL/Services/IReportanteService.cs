using SGEB.DAL.Entities;

namespace SGEB.BLL.Services
{
    public interface IReportanteService
    {
        /// <summary>
        /// Busca al reportante por documento; si no existe, lo crea.
        /// Devuelve el IdReportante en cualquiera de los dos casos.
        /// </summary>
        int ObtenerOCrear(Reportante reportante);

        List<Reportante> ObtenerTodos();
        Reportante? ObtenerPorId(int idReportante);
    }
}
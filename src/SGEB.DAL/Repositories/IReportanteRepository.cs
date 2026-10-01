using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public interface IReportanteRepository
    {
        int Registrar(Reportante reportante);
        Reportante? ObtenerPorDocumento(string tipoDocumento, string numeroDocumento);
        Reportante? ObtenerPorId(int idReportante);
        List<Reportante> Listar();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGEB.DAL.Entities;

namespace SGEB.DAL.Repositories
{
    public interface IAtencionCierreRepository
    {
        int Registrar(AtencionCierre cierre);

        AtencionCierre? ObtenerPorIncidente(int idIncidente);

        void MarcarAtendido(int idIncidente);
    }
}

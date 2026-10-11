using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGEB.DAL.Entities;

namespace SGEB.BLL.Services
{
    public interface IAtencionCierreService
    {
        void MarcarAtendido(int idIncidente);

        int RegistrarCierre(AtencionCierre cierre);

        AtencionCierre? ObtenerPorIncidente(int idIncidente);
    }
}

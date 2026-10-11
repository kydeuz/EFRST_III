using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGEB.DAL.Entities;
using SGEB.DAL.Repositories;

namespace SGEB.BLL.Services
{
    public class AtencionCierreService : IAtencionCierreService
    {
        private readonly IAtencionCierreRepository _cierreRepository;
        private readonly IIncidenteRepository _incidenteRepository;

        public AtencionCierreService(
            IAtencionCierreRepository cierreRepository,
            IIncidenteRepository incidenteRepository)
        {
            _cierreRepository = cierreRepository;
            _incidenteRepository = incidenteRepository;
        }

        public void MarcarAtendido(int idIncidente)
        {
            if (idIncidente <= 0)
                throw new ArgumentException(
                    "El identificador del incidente no es válido.");

            var incidente = _incidenteRepository.ObtenerPorId(idIncidente);

            if (incidente == null)
                throw new ArgumentException(
                    "El incidente seleccionado no existe.");

            if (incidente.Estado == "Cerrado")
                throw new InvalidOperationException(
                    "El incidente ya está cerrado.");

            if (incidente.Estado == "Atendido")
                throw new InvalidOperationException(
                    "El incidente ya fue marcado como atendido.");

            if (incidente.Estado != "Asignado")
                throw new InvalidOperationException(
                    "El incidente debe estar asignado antes de marcarlo como atendido.");

            _cierreRepository.MarcarAtendido(idIncidente);
        }

        public int RegistrarCierre(AtencionCierre cierre)
        {
            if (cierre.IdIncidente <= 0)
                throw new ArgumentException(
                    "El identificador del incidente no es válido.");

            var incidente =
                _incidenteRepository.ObtenerPorId(cierre.IdIncidente);

            if (incidente == null)
                throw new ArgumentException(
                    "El incidente seleccionado no existe.");

            if (incidente.Estado == "Cerrado")
                throw new InvalidOperationException(
                    "El incidente ya está cerrado.");

            if (incidente.Estado != "Atendido")
                throw new InvalidOperationException(
                    "El incidente debe estar atendido antes de cerrarlo.");

            var cierreExistente =
                _cierreRepository.ObtenerPorIncidente(cierre.IdIncidente);

            if (cierreExistente != null)
                throw new InvalidOperationException(
                    "El incidente ya tiene un cierre registrado.");

            var resultadosValidos = new[]
            {
                "Controlado",
                "Sin novedad",
                "Con pérdidas materiales",
                "Con víctimas",
                "Falsa alarma"
            };

            if (string.IsNullOrWhiteSpace(cierre.ResultadoCierre) ||
                !resultadosValidos.Contains(cierre.ResultadoCierre))
            {
                throw new ArgumentException(
                    "Seleccione un resultado de cierre válido.");
            }

            if (cierre.Observaciones?.Length > 500)
                throw new ArgumentException(
                    "Las observaciones no pueden superar los 500 caracteres.");

            if (cierre.PersonalInvolucrado.HasValue &&
                cierre.PersonalInvolucrado.Value < 0)
            {
                throw new ArgumentException(
                    "El personal involucrado no puede ser negativo.");
            }

            return _cierreRepository.Registrar(cierre);
        }

        public AtencionCierre? ObtenerPorIncidente(int idIncidente)
        {
            if (idIncidente <= 0)
                return null;

            return _cierreRepository.ObtenerPorIncidente(idIncidente);
        }
    }
}

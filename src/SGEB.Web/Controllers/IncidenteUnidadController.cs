using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using SGEB.BLL.Services;
using SGEB.Web.Models;

namespace SGEB.Web.Controllers
{
    public class IncidenteUnidadController : Controller
    {
        private readonly IIncidenteUnidadService _service;
        private readonly IIncidenteService _incidenteService;

        public IncidenteUnidadController(
            IIncidenteUnidadService service,
            IIncidenteService incidenteService)
        {
            _service = service;
            _incidenteService = incidenteService;
        }

        // GET: /IncidenteUnidad/Index/5
        [HttpGet]
        public IActionResult Index(int id)
        {
            var incidente = _incidenteService.ObtenerPorId(id);
            if (incidente == null) return NotFound();

            var modelo = new AsignarUnidadViewModel
            {
                IdIncidente = incidente.IdIncidente,
                TipoIncidente = incidente.TipoIncidente,
                Direccion = incidente.Direccion,
                EstadoIncidente = incidente.Estado,
                Asignadas = _service.ListarPorIncidente(id)
                    .Select(a => new UnidadAsignadaItemViewModel
                    {
                        IdIncidenteUnidad = a.IdIncidenteUnidad,
                        CodigoUnidad = a.CodigoUnidad ?? string.Empty,
                        TipoUnidad = a.TipoUnidad ?? string.Empty,
                        Estado = a.Estado,
                        FechaHoraAsignacion = a.FechaHoraAsignacion
                    }).ToList(),
                UnidadesDisponibles = _service.ListarUnidadesDisponibles()
                    .Select(u => new SelectListItem
                    {
                        Value = u.IdUnidad.ToString(),
                        Text = $"{u.CodigoUnidad} - {u.TipoUnidad}"
                    }).ToList()
            };

            return View(modelo);
        }

        // POST: /IncidenteUnidad/Asignar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Asignar(int id, int? idUnidad)
        {
            if (idUnidad == null)
            {
                TempData["Error"] = "Seleccione una unidad.";
                return RedirectToAction(nameof(Index), new { id });
            }

            try
            {
                _service.Asignar(id, idUnidad.Value);
                TempData["Mensaje"] = "Unidad asignada correctamente.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = TraducirError(ex);
            }

            return RedirectToAction(nameof(Index), new { id });
        }

        // POST: /IncidenteUnidad/Liberar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Liberar(int id, int idIncidenteUnidad)
        {
            try
            {
                _service.Liberar(idIncidenteUnidad);
                TempData["Mensaje"] = "Unidad liberada correctamente.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = TraducirError(ex);
            }

            return RedirectToAction(nameof(Index), new { id });
        }

        private static string TraducirError(SqlException ex) => ex.Number switch
        {
            50001 => "La unidad no está disponible (otra persona pudo asignarla antes).",
            50002 => "El incidente ya está cerrado: no se pueden asignar unidades.",
            50003 => "La asignación no existe o ya fue liberada.",
            _ => "Ocurrió un error al acceder a la base de datos."
        };
    }
}
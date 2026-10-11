using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGEB.BLL.Services;
using SGEB.DAL.Entities;
using SGEB.Web.Models;

namespace SGEB.Web.Controllers
{
    public class AtencionCierreController : Controller
    {
        private readonly IAtencionCierreService _atencionCierreService;
        private readonly IIncidenteService _incidenteService;

        public AtencionCierreController(
            IAtencionCierreService atencionCierreService,
            IIncidenteService incidenteService)
        {
            _atencionCierreService = atencionCierreService;
            _incidenteService = incidenteService;
        }

        // GET: /AtencionCierre
        [HttpGet]
        public IActionResult Index()
        {
            var incidentes = _incidenteService.ObtenerTodos();

            var modelo = incidentes
                .Select(i => new AtencionIncidenteViewModel
                {
                    IdIncidente = i.IdIncidente,
                    TipoIncidente = i.TipoIncidente,
                    Descripcion = i.Descripcion,
                    Direccion = i.Direccion,
                    Zona = i.Zona,
                    Prioridad = i.Prioridad,
                    Estado = i.Estado,
                    FechaHoraRegistro = i.FechaHoraRegistro,
                    NombreReportante = i.NombreReportante ?? string.Empty
                })
                .ToList();

            return View(modelo);
        }

        // POST: /AtencionCierre/MarcarAtendido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarAtendido(int idIncidente)
        {
            try
            {
                _atencionCierreService.MarcarAtendido(idIncidente);

                TempData["Mensaje"] =
                    $"El incidente #{idIncidente} fue marcado como atendido.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (SqlException ex)
            {
                TempData["Error"] =
                    $"No se pudo actualizar el incidente: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /AtencionCierre/Cerrar/1
        [HttpGet]
        public IActionResult Cerrar(int id)
        {
            var incidente = _incidenteService.ObtenerPorId(id);

            if (incidente == null)
                return NotFound();

            if (incidente.Estado == "Cerrado")
            {
                TempData["Error"] = "El incidente ya está cerrado.";
                return RedirectToAction(nameof(Index));
            }

            if (incidente.Estado != "Atendido")
            {
                TempData["Error"] =
                    "El incidente debe estar atendido antes de cerrarlo.";

                return RedirectToAction(nameof(Index));
            }

            var modelo = new RegistrarCierreViewModel
            {
                IdIncidente = incidente.IdIncidente,
                TipoIncidente = incidente.TipoIncidente,
                Direccion = incidente.Direccion,
                Zona = incidente.Zona,
                Estado = incidente.Estado
            };

            return View(modelo);
        }

        // POST: /AtencionCierre/Cerrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cerrar(RegistrarCierreViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                CargarDatosIncidente(modelo);
                return View(modelo);
            }

            var cierre = new AtencionCierre
            {
                IdIncidente = modelo.IdIncidente,
                ResultadoCierre = modelo.ResultadoCierre,
                Observaciones = modelo.Observaciones,
                PersonalInvolucrado = modelo.PersonalInvolucrado
            };

            try
            {
                var idCierre =
                    _atencionCierreService.RegistrarCierre(cierre);

                TempData["Mensaje"] =
                    $"Incidente #{modelo.IdIncidente} cerrado correctamente. " +
                    $"Código de cierre: {idCierre}.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"No se pudo cerrar el incidente: {ex.Message}");
            }

            CargarDatosIncidente(modelo);
            return View(modelo);
        }

        private void CargarDatosIncidente(RegistrarCierreViewModel modelo)
        {
            var incidente =
                _incidenteService.ObtenerPorId(modelo.IdIncidente);

            if (incidente == null)
                return;

            modelo.TipoIncidente = incidente.TipoIncidente;
            modelo.Direccion = incidente.Direccion;
            modelo.Zona = incidente.Zona;
            modelo.Estado = incidente.Estado;
        }
    }
}
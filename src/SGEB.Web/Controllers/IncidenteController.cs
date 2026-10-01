using Microsoft.AspNetCore.Mvc;
using SGEB.BLL.Services;
using SGEB.DAL.Entities;
using SGEB.Web.Models;

namespace SGEB.Web.Controllers
{
    public class IncidenteController : Controller
    {
        private readonly IIncidenteService _incidenteService;

        public IncidenteController(IIncidenteService incidenteService)
        {
            _incidenteService = incidenteService;
        }

        // GET: /Incidente
        public IActionResult Index()
        {
            var incidentes = _incidenteService.ObtenerTodos();

            var modelo = incidentes.Select(i => new IncidenteListItemViewModel
            {
                IdIncidente = i.IdIncidente,
                TipoIncidente = i.TipoIncidente,
                Direccion = i.Direccion,
                Zona = i.Zona,
                Prioridad = i.Prioridad,
                Estado = i.Estado,
                FechaHoraRegistro = i.FechaHoraRegistro,
                NombreReportante = i.NombreReportante ?? string.Empty
            }).ToList();

            return View(modelo);
        }

        // GET: /Incidente/Registrar
        [HttpGet]
        public IActionResult Registrar()
        {
            return View(new RegistrarIncidenteViewModel());
        }

        // POST: /Incidente/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(RegistrarIncidenteViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var incidente = new Incidente
            {
                TipoIncidente = modelo.TipoIncidente,
                Descripcion = modelo.Descripcion,
                Direccion = modelo.Direccion,
                Zona = modelo.Zona,
                Latitud = modelo.Latitud,
                Longitud = modelo.Longitud,
                Prioridad = modelo.Prioridad
            };

            var reportante = new Reportante
            {
                TipoDocumento = modelo.TipoDocumento,
                NumeroDocumento = modelo.NumeroDocumento,
                Nombre = modelo.NombreReportante,
                Telefono = modelo.TelefonoReportante,
                Correo = modelo.CorreoReportante
            };

            try
            {
                var idNuevo = _incidenteService.RegistrarIncidente(incidente, reportante);
                TempData["Mensaje"] = $"Incidente #{idNuevo} registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(modelo);
            }
        }
    }
}
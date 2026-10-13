using Microsoft.AspNetCore.Mvc;
using SGEB.BLL.Services;
using SGEB.DAL.Entities;
using SGEB.Web.Models;

namespace SGEB.Web.Controllers
{
    public class UnidadController : Controller
    {
        private static readonly string[] Estados =
        {
            "Disponible", "Mantenimiento", "Fuera de servicio"
        };

        private readonly IUnidadService _unidadService;

        public UnidadController(IUnidadService unidadService)
        {
            _unidadService = unidadService;
        }

        // GET: /Unidad
        public IActionResult Index()
        {
            var unidades = _unidadService.ObtenerTodas();

            var modelo = unidades.Select(u => new UnidadListItemViewModel
            {
                IdUnidad = u.IdUnidad,
                CodigoUnidad = u.CodigoUnidad,
                TipoUnidad = u.TipoUnidad,
                Placa = u.Placa ?? string.Empty,
                Capacidad = u.Capacidad,
                Estacion = u.Estacion ?? string.Empty,
                Estado = u.Estado,
                FechaRegistro = u.FechaRegistro
            }).ToList();

            // Opciones de la lista desplegable para cambiar el estado
            ViewBag.Estados = Estados;

            return View(modelo);
        }

        // GET: /Unidad/Registrar
        [HttpGet]
        public IActionResult Registrar()
        {
            return View(new RegistrarUnidadViewModel());
        }

        // POST: /Unidad/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(RegistrarUnidadViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var unidad = new Unidad
            {
                CodigoUnidad = modelo.CodigoUnidad,
                TipoUnidad = modelo.TipoUnidad,
                Placa = modelo.Placa,
                Capacidad = modelo.Capacidad,
                Estacion = modelo.Estacion
            };

            try
            {
                _unidadService.RegistrarUnidad(unidad);
                TempData["Mensaje"] = $"Unidad {unidad.CodigoUnidad} registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // Código de unidad repetido: se muestra junto al campo
                ModelState.AddModelError(nameof(modelo.CodigoUnidad), ex.Message);
                return View(modelo);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(modelo);
            }
        }

        // POST: /Unidad/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int idUnidad, string nuevoEstado)
        {
            try
            {
                _unidadService.CambiarEstado(idUnidad, nuevoEstado);
                TempData["Mensaje"] = "Estado actualizado correctamente.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SGEB.BLL.Exporters;

namespace SGEB.Web.Controllers
{
    public class ReporteController : Controller
    {
        private readonly IEnumerable<IReportExporter> _exporters;

        public ReporteController(IEnumerable<IReportExporter> exporters)
        {
            _exporters = exporters;
        }

        // GET: /Reporte/Exportar?formato=pdf|excel|csv
        public IActionResult Exportar(string formato)
        {
            var exporter = _exporters.FirstOrDefault(e => e.Formato == formato);
            if (exporter == null) return BadRequest("Formato no soportado.");

            // TODO (quien haga Reportes): reemplazar estos datos de prueba
            // por el resultado del service de estadísticas.
            var columnas = new[] { "Tipo", "Total", "Cerrados" };
            var filas = new List<string[]>
            {
                new[] { "Incendio", "10", "8" },
                new[] { "Rescate", "5", "5" }
            };

            var bytes = exporter.Exportar("Estadísticas por tipo", columnas, filas);
            return File(bytes, exporter.ContentType, $"reporte.{exporter.Extension}");
        }
    }
}
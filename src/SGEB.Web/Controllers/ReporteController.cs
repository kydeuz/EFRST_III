using Microsoft.AspNetCore.Mvc;
using SGEB.BLL.Exporters;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SGEB.BLL.Exporters;
using SGEB.BLL.Services;
using SGEB.Web.Models;

namespace SGEB.Web.Controllers
{
    public class ReporteController : Controller
    {
        private readonly IEnumerable<IReportExporter> _exporters;
        private readonly IReporteService _reporteService;

        public ReporteController(IEnumerable<IReportExporter> exporters, IReporteService reporteService)
        {
            _exporters = exporters;
            _reporteService = reporteService;
        }

        public IActionResult Index() => RedirectToAction(nameof(PorTipo));

        public IActionResult PorTipo() => View(ObtenerPorTipo());

        public IActionResult PorZona() => View(ObtenerPorZona());

        // GET: /Reporte/Exportar?reporte=tipo|zona&formato=pdf|excel|csv
        public IActionResult Exportar(string reporte, string formato)
        {
            var exporter = _exporters.FirstOrDefault(e => e.Formato == formato);
            if (exporter == null) return BadRequest("Formato no soportado.");

            string titulo;
            string[] columnas;
            List<string[]> filas;

            switch (reporte)
            {
                case "tipo":
                    var porTipo = ObtenerPorTipo();
                    titulo = "Estadísticas por tipo de incidente";
                    columnas = new[] { "Tipo", "Total", "Cerrados", "Sin asignar", "Min. promedio", "% cierre" };
                    filas = porTipo.Select(d => new[]
                    {
                        d.TipoIncidente,
                        d.TotalIncidentes.ToString(),
                        d.Cerrados.ToString(),
                        d.SinAsignar.ToString(),
                        d.MinutosPromedioAtencion?.ToString() ?? "-",
                        Pct(d.PorcentajeCierre)
                    }).ToList();
                    filas.Add(new[]
                    {
                        "TOTAL",
                        porTipo.Sum(d => d.TotalIncidentes).ToString(),
                        porTipo.Sum(d => d.Cerrados).ToString(),
                        porTipo.Sum(d => d.SinAsignar).ToString(),
                        "-",
                        Pct(PctGlobal(porTipo.Sum(d => d.Cerrados), porTipo.Sum(d => d.TotalIncidentes)))
                    });
                    break;

                case "zona":
                    var porZona = ObtenerPorZona();
                    titulo = "Estadísticas por zona";
                    columnas = new[] { "Zona", "Total", "Cerrados", "% cierre" };
                    filas = porZona.Select(d => new[]
                    {
                        d.Zona,
                        d.TotalIncidentes.ToString(),
                        d.Cerrados.ToString(),
                        Pct(d.PorcentajeCierre)
                    }).ToList();
                    filas.Add(new[]
                    {
                        "TOTAL",
                        porZona.Sum(d => d.TotalIncidentes).ToString(),
                        porZona.Sum(d => d.Cerrados).ToString(),
                        Pct(PctGlobal(porZona.Sum(d => d.Cerrados), porZona.Sum(d => d.TotalIncidentes)))
                    });
                    break;

                default:
                    return BadRequest("Reporte no soportado.");
            }

            var bytes = exporter.Exportar(titulo, columnas, filas);
            var nombre = $"reporte-{reporte}-{DateTime.Now:yyyyMMdd-HHmm}.{exporter.Extension}";
            return File(bytes, exporter.ContentType, nombre);
        }

        private List<EstadisticaTipoViewModel> ObtenerPorTipo() =>
            _reporteService.ObtenerEstadisticasPorTipo().Select(e => new EstadisticaTipoViewModel
            {
                TipoIncidente = e.TipoIncidente,
                TotalIncidentes = e.TotalIncidentes,
                Cerrados = e.Cerrados,
                SinAsignar = e.SinAsignar,
                MinutosPromedioAtencion = e.MinutosPromedioAtencion
            }).ToList();

        private List<EstadisticaZonaViewModel> ObtenerPorZona() =>
            _reporteService.ObtenerEstadisticasPorZona().Select(e => new EstadisticaZonaViewModel
            {
                Zona = e.Zona,
                TotalIncidentes = e.TotalIncidentes,
                Cerrados = e.Cerrados
            }).ToList();

        private static double PctGlobal(int cerrados, int total) => total == 0 ? 0 : cerrados * 100.0 / total;
        private static string Pct(double v) => v.ToString("0.0", CultureInfo.InvariantCulture) + "%";
    }
}
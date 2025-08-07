using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;

namespace MecaFlow2025.Controllers
{
    public class ReportesFinancierosController : Controller
    {
        private readonly MecaFlowContext _ctx;
        public ReportesFinancierosController(MecaFlowContext ctx) => _ctx = ctx;

        public async Task<IActionResult> Index(string scope = "day")
        {
            var registros = await _ctx.ReportesFinancieros
                                      .AsNoTracking()
                                      .OrderBy(r => r.Fecha)
                                      .ToListAsync();

            // Tabla de meses en español
            string[] mesesEs = new[]
            { "Enero","Febrero","Marzo","Abril","Mayo","Junio",
      "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre" };

            IEnumerable<ResumenVM> data;
            switch (scope)
            {
                case "year":
                    ViewBag.TituloScope = "AÑO";
                    data = registros
                           .GroupBy(r => r.Fecha.Year)
                           .Select(g => new ResumenVM
                           {
                               Periodo = g.Key.ToString(),          // 2025
                               TotalIngresos = g.Sum(x => x.TotalIngresos ?? 0),
                               TotalGastos = g.Sum(x => x.TotalGastos ?? 0),
                               ReporteId = null
                           });
                    break;

                case "month":
                    ViewBag.TituloScope = "MES";
                    data = registros
                           .GroupBy(r => new { r.Fecha.Year, r.Fecha.Month })
                           .Select(g => new ResumenVM
                           {
                               // «agosto 2025»
                               Periodo = $"{mesesEs[g.Key.Month - 1]} {g.Key.Year}",
                               TotalIngresos = g.Sum(x => x.TotalIngresos ?? 0),
                               TotalGastos = g.Sum(x => x.TotalGastos ?? 0),
                               ReporteId = null
                           });
                    break;

                default: // "day"
                    ViewBag.TituloScope = "DÍA";
                    data = registros.Select(r => new ResumenVM
                    {
                        Periodo = r.Fecha.ToString("dd-MM-yyyy"),
                        TotalIngresos = r.TotalIngresos ?? 0,
                        TotalGastos = r.TotalGastos ?? 0,
                        ReporteId = r.ReporteId
                    });
                    break;
            }

            ViewBag.Scope = scope;
            return View(data.OrderBy(d => d.Periodo));
        }


        // GET
        public IActionResult Create() => View();

        // POST
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string FechaString,
                                                decimal? TotalIngresos,
                                                decimal? TotalGastos,
                                                string? Observaciones)
        {
            if (!DateOnly.TryParseExact(FechaString, "yyyy-MM-dd", null,
                                        DateTimeStyles.None, out var fecha))
            {
                ModelState.AddModelError("FechaString", "Fecha inválida");
                return View();
            }

            var registro = new ReportesFinanciero
            {
                Fecha = fecha,
                TotalIngresos = TotalIngresos,
                TotalGastos = TotalGastos,
                Observaciones = Observaciones
            };

            _ctx.Add(registro);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var reg = await _ctx.ReportesFinancieros.FindAsync(id);
            return reg is null ? NotFound() : View(reg);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
                                              string FechaString,
                                              decimal? TotalIngresos,
                                              decimal? TotalGastos,
                                              string? Observaciones)
        {
            var reg = await _ctx.ReportesFinancieros.FindAsync(id);
            if (reg is null) return NotFound();

            if (!DateOnly.TryParseExact(FechaString, "yyyy-MM-dd", null,
                                        DateTimeStyles.None, out var fecha))
            {
                ModelState.AddModelError("FechaString", "Fecha inválida");
                return View(reg);
            }

            reg.Fecha = fecha;
            reg.TotalIngresos = TotalIngresos;
            reg.TotalGastos = TotalGastos;
            reg.Observaciones = Observaciones;

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ReportesFinancieros/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var reg = await _ctx.ReportesFinancieros
                                .AsNoTracking()
                                .FirstOrDefaultAsync(r => r.ReporteId == id);

            return reg is null ? NotFound() : View(reg);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var reg = await _ctx.ReportesFinancieros.FindAsync(id);
            return reg is null ? NotFound() : View(reg);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reg = await _ctx.ReportesFinancieros.FindAsync(id);
            if (reg is not null)
            {
                _ctx.Remove(reg);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        /*--------------------- View-Model -----------------------------*/
        public class ResumenVM
        {
            public string Periodo { get; set; } = "";
            public decimal TotalIngresos { get; set; }
            public decimal TotalGastos { get; set; }
            public decimal Utilidad => TotalIngresos - TotalGastos;
            public int? ReporteId { get; set; } 
        }
    }
}

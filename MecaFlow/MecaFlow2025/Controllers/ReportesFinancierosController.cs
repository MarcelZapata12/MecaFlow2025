using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using System.Threading.Tasks;

namespace MecaFlow2025.Controllers
{
    public class ReportesFinancierosController : Controller
    {
        private readonly MecaFlowContext _context;
        public ReportesFinancierosController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: ReportesFinancieros
        public async Task<IActionResult> Index()
        {
            var lista = await _context.ReportesFinancieros
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
            return View(lista);
        }

        // GET: ReportesFinancieros/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var rpt = await _context.ReportesFinancieros
                .FirstOrDefaultAsync(r => r.ReporteId == id);
            if (rpt == null) return NotFound();
            return View(rpt);
        }

        // GET: ReportesFinancieros/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ReportesFinancieros/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Fecha,TotalIngresos,TotalGastos,Observaciones")] ReportesFinanciero rpt)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rpt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rpt);
        }

        // GET: ReportesFinancieros/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var rpt = await _context.ReportesFinancieros.FindAsync(id);
            if (rpt == null) return NotFound();
            return View(rpt);
        }

        // POST: ReportesFinancieros/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReporteId,Fecha,TotalIngresos,TotalGastos,Observaciones")] ReportesFinanciero rpt)
        {
            if (id != rpt.ReporteId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rpt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.ReportesFinancieros.AnyAsync(r => r.ReporteId == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(rpt);
        }

        // GET: ReportesFinancieros/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var rpt = await _context.ReportesFinancieros
                .FirstOrDefaultAsync(r => r.ReporteId == id);
            if (rpt == null) return NotFound();
            return View(rpt);
        }

        // POST: ReportesFinancieros/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rpt = await _context.ReportesFinancieros.FindAsync(id);
            if (rpt != null)
            {
                _context.ReportesFinancieros.Remove(rpt);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

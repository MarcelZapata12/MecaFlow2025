using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MecaFlow2025.Controllers
{
    public class PagosController : Controller
    {
        private readonly MecaFlowContext _context;
        public PagosController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            var pagos = await _context.Pagos
                .Include(p => p.Factura)
                .ThenInclude(f => f.Cliente)
                .Include(p => p.Factura)
                .ThenInclude(f => f.Vehiculo)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
            return View(pagos);
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var pago = await _context.Pagos
                .Include(p => p.Factura)
                .ThenInclude(f => f.Cliente)
                .Include(p => p.Factura)
                .ThenInclude(f => f.Vehiculo)
                .FirstOrDefaultAsync(p => p.PagoId == id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // GET: Pagos/Create
        public IActionResult Create()
        {
            // Dropdown de facturas mostrando número y monto
            ViewBag.Facturas = new SelectList(
                _context.Facturas
                    .Include(f => f.Cliente)
                    .OrderBy(f => f.FacturaId)
                    .Select(f => new {
                        f.FacturaId,
                        Text = $"#{f.FacturaId} – {f.Cliente.Nombre} – {f.MontoTotal:C}"
                    }),
                "FacturaId", "Text"
            );
            // Opciones de método de pago
            ViewBag.Metodos = new SelectList(
                new[] { "Efectivo", "Tarjeta" }
            );
            return View();
        }

        // POST: Pagos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FacturaId,FechaPago,MetodoPago")] Pago model)
        {
            if (!ModelState.IsValid)
            {
                // recargar dropdowns
                ViewBag.Facturas = new SelectList(
                    _context.Facturas
                        .Include(f => f.Cliente)
                        .Select(f => new {
                            f.FacturaId,
                            Text = $"#{f.FacturaId} – {f.Cliente.Nombre} – {f.MontoTotal:C}"
                        }),
                    "FacturaId", "Text", model.FacturaId
                );
                ViewBag.Metodos = new SelectList(
                    new[] { "Efectivo", "Tarjeta" },
                    model.MetodoPago
                );
                return View(model);
            }

            _context.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null) return NotFound();

            ViewBag.Facturas = new SelectList(
                _context.Facturas
                    .Include(f => f.Cliente)
                    .Select(f => new {
                        f.FacturaId,
                        Text = $"#{f.FacturaId} – {f.Cliente.Nombre} – {f.MontoTotal:C}"
                    }),
                "FacturaId", "Text", pago.FacturaId
            );
            ViewBag.Metodos = new SelectList(
                new[] { "Efectivo", "Tarjeta" },
                pago.MetodoPago
            );
            return View(pago);
        }

        // POST: Pagos/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PagoId,FacturaId,FechaPago,MetodoPago")] Pago model)
        {
            if (id != model.PagoId) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Facturas = new SelectList(
                    _context.Facturas
                        .Include(f => f.Cliente)
                        .Select(f => new {
                            f.FacturaId,
                            Text = $"#{f.FacturaId} – {f.Cliente.Nombre} – {f.MontoTotal:C}"
                        }),
                    "FacturaId", "Text", model.FacturaId
                );
                ViewBag.Metodos = new SelectList(
                    new[] { "Efectivo", "Tarjeta" },
                    model.MetodoPago
                );
                return View(model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pagos.Any(p => p.PagoId == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var pago = await _context.Pagos
                .Include(p => p.Factura)
                .ThenInclude(f => f.Cliente)
                .Include(p => p.Factura)
                .ThenInclude(f => f.Vehiculo)
                .FirstOrDefaultAsync(p => p.PagoId == id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // POST: Pagos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

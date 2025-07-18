// DiagnosticosController.cs (Completamente funcional con filtro de búsqueda)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using System.Linq;
using System.Threading.Tasks;
using MecaFlow2025.Attributes;

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador", "Empleado", "Cliente")]
    public class DiagnosticosController : Controller
    {
        private readonly MecaFlowContext _context;
        public DiagnosticosController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: Diagnosticos
        public async Task<IActionResult> Index(string search)
        {
            var diagnosticos = _context.Diagnosticos
                .Include(d => d.Vehiculo)
                .Include(d => d.Empleado)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                diagnosticos = diagnosticos.Where(d =>
                    d.Detalle.Contains(search) ||
                    d.Vehiculo.Placa.Contains(search) ||
                    d.Empleado.Nombre.Contains(search));
            }

            ViewBag.CurrentFilter = search;
            var lista = await diagnosticos.OrderByDescending(d => d.Fecha).ToListAsync();
            return View(lista);
        }

        // GET: Diagnosticos/Create
        public IActionResult Create()
        {
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos.OrderBy(v => v.Placa), "VehiculoId", "Placa");
            ViewBag.Empleados = new SelectList(_context.Empleados.OrderBy(e => e.Nombre), "EmpleadoId", "Nombre");
            return View();
        }

        // POST: Diagnosticos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehiculoId,Fecha,Detalle,EmpleadoId")] Diagnostico model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Vehiculos = new SelectList(_context.Vehiculos.OrderBy(v => v.Placa), "VehiculoId", "Placa", model.VehiculoId);
                ViewBag.Empleados = new SelectList(_context.Empleados.OrderBy(e => e.Nombre), "EmpleadoId", "Nombre", model.EmpleadoId);
                return View(model);
            }

            _context.Diagnosticos.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Diagnosticos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var diag = await _context.Diagnosticos.FindAsync(id);
            if (diag == null) return NotFound();

            ViewBag.Vehiculos = new SelectList(_context.Vehiculos.OrderBy(v => v.Placa), "VehiculoId", "Placa", diag.VehiculoId);
            ViewBag.Empleados = new SelectList(_context.Empleados.OrderBy(e => e.Nombre), "EmpleadoId", "Nombre", diag.EmpleadoId);
            return View(diag);
        }

        // POST: Diagnosticos/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DiagnosticoId,VehiculoId,Fecha,Detalle,EmpleadoId")] Diagnostico model)
        {
            if (id != model.DiagnosticoId) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Vehiculos = new SelectList(_context.Vehiculos.OrderBy(v => v.Placa), "VehiculoId", "Placa", model.VehiculoId);
                ViewBag.Empleados = new SelectList(_context.Empleados.OrderBy(e => e.Nombre), "EmpleadoId", "Nombre", model.EmpleadoId);
                return View(model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Diagnosticos.Any(d => d.DiagnosticoId == id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Diagnosticos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var diag = await _context.Diagnosticos
                .Include(d => d.Vehiculo)
                .Include(d => d.Empleado)
                .FirstOrDefaultAsync(d => d.DiagnosticoId == id);

            if (diag == null) return NotFound();

            return View(diag);
        }

        // POST: Diagnosticos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var diag = await _context.Diagnosticos.FindAsync(id);
            if (diag != null)
            {
                _context.Diagnosticos.Remove(diag);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

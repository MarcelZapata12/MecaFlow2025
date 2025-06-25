using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MecaFlow2025.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly MecaFlowContext _context;
        public VehiculosController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: Vehiculos
        public async Task<IActionResult> Index()
        {
            var lista = await _context.Vehiculos
                .Include(v => v.Cliente)
                .ToListAsync();
            return View(lista);
        }

        // GET: Vehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var veh = await _context.Vehiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.VehiculoId == id);
            if (veh == null) return NotFound();
            return View(veh);
        }

        // GET: Vehiculos/Create
        public IActionResult Create()
        {
            // Construimos el dropdown de clientes
            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre");
            return View();
        }

        // POST: Vehiculos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Placa,Marca,Modelo,Anio,ClienteId")] Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre",
                vehiculo.ClienteId);
            return View(vehiculo);
        }

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var veh = await _context.Vehiculos.FindAsync(id);
            if (veh == null) return NotFound();

            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre",
                veh.ClienteId);
            return View(veh);
        }

        // POST: Vehiculos/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("VehiculoId,Placa,Marca,Modelo,Anio,ClienteId")] Vehiculo vehiculo)
        {
            if (id != vehiculo.VehiculoId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehiculos.Any(e => e.VehiculoId == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre",
                vehiculo.ClienteId);
            return View(vehiculo);
        }

        // GET: Vehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var veh = await _context.Vehiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.VehiculoId == id);
            if (veh == null) return NotFound();
            return View(veh);
        }

        // POST: Vehiculos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var veh = await _context.Vehiculos.FindAsync(id);
            if (veh != null)
            {
                _context.Vehiculos.Remove(veh);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

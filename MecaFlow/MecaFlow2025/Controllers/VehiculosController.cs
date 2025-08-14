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
    public class VehiculosController : Controller
    {
        private readonly MecaFlowContext _context;
        public VehiculosController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: Vehiculos
        public async Task<IActionResult> Index(string filtroPlaca)
        {
            var lista = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .ToListAsync();
            if (!string.IsNullOrWhiteSpace(filtroPlaca))
            {
                lista = lista.Where(v => v.Placa.Contains(filtroPlaca, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.FiltroPlaca = filtroPlaca;

            return View(lista);
        }


        // GET: Vehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var veh = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .FirstOrDefaultAsync(v => v.VehiculoId == id);
            if (veh == null) return NotFound();
            return View(veh);
        }
        [AuthorizeRole("Administrador", "Empleado")]
        // GET: Vehiculos/Create
        public IActionResult Create()
        {
            // Construimos el dropdown de clientes
            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre");
            // Construimos el dropdown de marcas
            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nombre),
                "MarcaId", "Nombre");
            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nombre).ToList(),
                "MarcaId",   // propiedad que se usará como value
                "Nombre"     // propiedad que se usará como texto visible
    );

            return View();
        }

        // POST: Vehiculos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Placa,Anio,ClienteId,MarcaId,ModeloId")] Vehiculo vehiculo)
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

            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nombre),
                "MarcaId", "Nombre",
                vehiculo.MarcaId);

            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nombre).ToList(),
                "MarcaId",
                "Nombre",
                vehiculo.MarcaId);


            return View(vehiculo);
        }

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehiculo = await _context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.VehiculoId == id);

            if (vehiculo == null) return NotFound();

            // Dropdown de clientes (con selección actual)
            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre", vehiculo.ClienteId);

            // Dropdown de marcas (con selección actual)
            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nombre),
                "MarcaId", "Nombre", vehiculo.MarcaId);

            // Dropdown de modelos filtrados por marca seleccionada (con selección actual)
            ViewBag.Modelos = new SelectList(
                _context.Modelos
                    .Where(m => m.MarcaId == vehiculo.MarcaId)
                    .OrderBy(m => m.Nombre),
                "ModeloId", "Nombre", vehiculo.ModeloId);

            return View(vehiculo);
        }

        // POST: Vehiculos/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("VehiculoId,Placa,Anio,ClienteId,MarcaId,ModeloId")] Vehiculo vehiculo)
        {
            if (id != vehiculo.VehiculoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehiculos.Any(e => e.VehiculoId == id))
                        return NotFound();
                    throw;
                }
            }

            // ⚠️ Si ModelState no es válido, volver a cargar los dropdowns con selección actual

            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre",
                vehiculo.ClienteId);

            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nombre),
                "MarcaId", "Nombre",
                vehiculo.MarcaId);

            ViewBag.Modelos = new SelectList(
                _context.Modelos
                    .Where(m => m.MarcaId == vehiculo.MarcaId)
                    .OrderBy(m => m.Nombre),
                "ModeloId", "Nombre",
                vehiculo.ModeloId);

            return View(vehiculo);
        }

        // GET: Vehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var veh = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
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

        [HttpGet]
        public JsonResult GetModelosPorMarca(int marcaId)
        {
            var modelos = _context.Modelos
                .Where(m => m.MarcaId == marcaId)
                .OrderBy(m => m.Nombre)
                .Select(m => new {
                    modeloId = m.ModeloId,
                    nombre = m.Nombre
                })
                .ToList();

            return Json(modelos);
        }

    }
}

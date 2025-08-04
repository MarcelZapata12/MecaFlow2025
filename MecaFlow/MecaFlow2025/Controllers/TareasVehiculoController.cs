using MecaFlow2025.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MecaFlow2025.Controllers
{
    public class TareasVehiculoController : Controller
    {

        private readonly MecaFlowContext _context;

        public TareasVehiculoController(MecaFlowContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tareas = await _context.TareasVehiculos
                .Include(t => t.Vehiculo)
                .ToListAsync();

            return View(tareas);
        }

        public IActionResult Create()
        {
            ViewBag.Vehiculos = _context.Vehiculos
    .Include(v => v.Cliente)
    .ToList()
    .Select(v => new SelectListItem
    {
        Value = v.VehiculoId.ToString(),
        Text = v.Placa + " - " + v.Cliente.Nombre
    }).ToList();


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TareasVehiculo tarea)
        {
            Console.WriteLine($"VehiculoId recibido: {tarea.VehiculoId}");
            if (ModelState.IsValid)
            {
                _context.TareasVehiculos.Add(tarea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehiculos = _context.Vehiculos
    .Include(v => v.Cliente)
    .ToList()
    .Select(v => new SelectListItem
    {
        Value = v.VehiculoId.ToString(),
        Text = v.Placa + " - " + v.Cliente.Nombre
    }).ToList();

            return View(tarea);
        }

    }
}

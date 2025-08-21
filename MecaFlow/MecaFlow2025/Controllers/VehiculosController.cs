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
            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            IQueryable<Vehiculo> query = _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.Marca)
                .Include(v => v.Modelo);

            // Si es cliente, solo mostrar sus vehículos
            if (userRole == "Cliente")
            {
                query = query.Where(v => v.Cliente.Correo == userEmail);
            }

            var lista = await query.ToListAsync();

            if (!string.IsNullOrWhiteSpace(filtroPlaca))
            {
                lista = lista
                    .Where(v => v.Placa.Contains(filtroPlaca, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.FiltroPlaca = filtroPlaca;
            ViewBag.UserRole = userRole; // Pasar el rol a la vista
            return View(lista);
        }

        // GET: Vehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            var vehiculoQuery = _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Where(v => v.VehiculoId == id);

            // Si es cliente, verificar que el vehículo le pertenezca
            if (userRole == "Cliente")
            {
                vehiculoQuery = vehiculoQuery.Where(v => v.Cliente.Correo == userEmail);
            }

            var veh = await vehiculoQuery.FirstOrDefaultAsync();

            if (veh == null) return NotFound();
            return View(veh);
        }

        [AuthorizeRole("Administrador", "Empleado")]
        // GET: Vehiculos/Create
        public IActionResult Create()
        {
            // Dropdown de clientes
            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre");

            // Dropdown de marcas (sin duplicar asignación)
            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nombre).ToList(),
                "MarcaId",   // value
                "Nombre"     // texto
            );

            return View();
        }

        // POST: Vehiculos/Create
        [HttpPost, ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> Create(
            [Bind("Placa,Anio,ClienteId,MarcaId,ModeloId")] Vehiculo vehiculo)
        {
            // Validación de placa duplicada (case-insensitive)
            bool placaExiste = await _context.Vehiculos
                .AnyAsync(v => v.Placa.ToLower() == vehiculo.Placa.ToLower());

            if (placaExiste)
                ModelState.AddModelError(nameof(Vehiculo.Placa), "Ya existe un vehículo registrado con esta placa.");

            if (!ModelState.IsValid)
            {
                // Recargar dropdowns
                ViewBag.Clientes = new SelectList(
                    _context.Clientes.OrderBy(c => c.Nombre),
                    "ClienteId", "Nombre", vehiculo.ClienteId);

                ViewBag.Marcas = new SelectList(
                    _context.Marcas.OrderBy(m => m.Nombre),
                    "MarcaId", "Nombre", vehiculo.MarcaId);

                // Si vino por AJAX, devolver el parcial para re-renderizar dentro del modal
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("Create", vehiculo);

                // Fallback no-AJAX
                return View(vehiculo);
            }

            _context.Add(vehiculo);
            await _context.SaveChangesAsync();

            // Respuesta AJAX: JSON para cerrar modal y recargar lista
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            // Fallback no-AJAX
            return RedirectToAction(nameof(Index));
        }

        // GET: Vehiculos/Edit/5
        [AuthorizeRole("Administrador", "Empleado")]
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
        [AuthorizeRole("Administrador", "Empleado")]
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

            // Si ModelState no es válido, volver a cargar dropdowns
            ViewBag.Clientes = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "ClienteId", "Nombre", vehiculo.ClienteId);

            ViewBag.Marcas = new SelectList(
                _context.Marcas.OrderBy(m => m.Nombre),
                "MarcaId", "Nombre", vehiculo.MarcaId);

            ViewBag.Modelos = new SelectList(
                _context.Modelos
                    .Where(m => m.MarcaId == vehiculo.MarcaId)
                    .OrderBy(m => m.Nombre),
                "ModeloId", "Nombre", vehiculo.ModeloId);

            return View(vehiculo);
        }

        // GET: Vehiculos/Delete/5
        [AuthorizeRole("Administrador", "Empleado")]
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
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var veh = await _context.Vehiculos.FirstOrDefaultAsync(v => v.VehiculoId == id);
            if (veh == null)
            {
                TempData["Error"] = "El vehículo no existe o ya fue eliminado.";
                return RedirectToAction(nameof(Index));
            }

            // Contar registros asociados
            var diagnosticosCount = await _context.Diagnosticos.CountAsync(d => d.VehiculoId == id);
            var facturasCount = await _context.Facturas.CountAsync(f => f.VehiculoId == id);
            var ingresosCount = await _context.IngresosVehiculos.CountAsync(i => i.VehiculoId == id);
            var tareasCount = await _context.TareasVehiculos.CountAsync(t => t.VehiculoId == id);

            if (diagnosticosCount > 0 || facturasCount > 0 || ingresosCount > 0 || tareasCount > 0)
            {
                var errorMessages = new List<string>();
                if (diagnosticosCount > 0)
                {
                    errorMessages.Add($"{diagnosticosCount} diagnóstico(s)");
                }
                if (facturasCount > 0)
                {
                    errorMessages.Add($"{facturasCount} factura(s)");
                }
                if (ingresosCount > 0)
                {
                    errorMessages.Add($"{ingresosCount} ingreso(s)");
                }
                if (tareasCount > 0)
                {
                    errorMessages.Add($"{tareasCount} tarea(s)");
                }

                TempData["Error"] = $"No se puede eliminar el vehículo porque tiene información asociada: " +
                                   $"{string.Join(" y ", errorMessages)}. " +
                                   $"Elimine primero esos registros desde sus respectivos módulos.";
                return RedirectToAction(nameof(Index));
            }

            _context.Vehiculos.Remove(veh);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehículo eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se pudo eliminar el vehículo por referencias existentes.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ========= NUEVO: Marca -> Modelo (JSON) =========
        // GET: Vehiculos/GetModelosPorMarca?marcaId=123
        [HttpGet]
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> GetModelosPorMarca(int marcaId)
        {
            if (marcaId <= 0)
                return BadRequest("marcaId inválido.");

            var modelos = await _context.Modelos
                .Where(m => m.MarcaId == marcaId)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    modeloId = m.ModeloId,
                    nombre = m.Nombre
                })
                .ToListAsync();

            return Ok(modelos); // 200 JSON
        }
    }
}
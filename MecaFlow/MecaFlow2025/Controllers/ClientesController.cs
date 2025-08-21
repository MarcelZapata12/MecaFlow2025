using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Attributes;
using System; // por DateTime

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador","Empleado")]
    public class ClientesController : Controller
    {
        private readonly MecaFlowContext _context;

        private readonly string[] Provincias = new[]
        {
            "San José", "Alajuela", "Cartago", "Heredia",
            "Guanacaste", "Puntarenas", "Limón"
        };

        public ClientesController(MecaFlowContext context)
        {
            _context = context;
        }

        private void PoblarProvincias(string? seleccionada = null)
        {
            ViewBag.Provincias = new SelectList(Provincias, seleccionada);
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = userRole;

            var clientes = await _context.Clientes.ToListAsync();
            return View(clientes);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ClienteId == id);
            if (cliente == null) return NotFound();
            return View(cliente); // La vista Details ya tiene Layout = null para modal
        }

        // === CREATE (GET) ===
        [HttpGet]
        public IActionResult Create()
        {
            PoblarProvincias();
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax) return PartialView("Create");
            return View();
        }

        // === CREATE (POST) con soporte AJAX ===
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,Nombre,Correo,Telefono,Direccion")] Cliente cliente)
        {
            // Reglas de negocio
            if (!string.IsNullOrWhiteSpace(cliente.Telefono) &&
                !Regex.IsMatch(cliente.Telefono, @"^\d+$"))
            {
                ModelState.AddModelError(nameof(cliente.Telefono),
                    "El teléfono debe contener solo números.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Correo) ||
                !new EmailAddressAttribute().IsValid(cliente.Correo))
            {
                ModelState.AddModelError(nameof(cliente.Correo),
                    "Formato de correo inválido.");
            }

            bool nombreRepetido = await _context.Clientes
                .AnyAsync(c => c.Nombre == cliente.Nombre);
            if (nombreRepetido)
            {
                ModelState.AddModelError(nameof(cliente.Nombre),
                    "Ya existe un cliente con ese nombre.");
            }

            bool correoRepetido = await _context.Clientes
                .AnyAsync(c => c.Correo == cliente.Correo);
            if (correoRepetido)
            {
                ModelState.AddModelError(nameof(cliente.Correo),
                    "Ese correo ya está registrado.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Direccion) ||
                !Provincias.Contains(cliente.Direccion))
            {
                ModelState.AddModelError(nameof(cliente.Direccion),
                    "Debes seleccionar una provincia válida.");
            }

            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                PoblarProvincias(cliente.Direccion);
                // Si es AJAX, devolvemos el markup de la vista para re-render en el modal
                if (isAjax) return PartialView("Create", cliente);
                return View(cliente);
            }

            // Éxito
            cliente.FechaRegistro = DateTime.Now;
            _context.Add(cliente);
            await _context.SaveChangesAsync();

            if (isAjax)
                return Json(new { ok = true, id = cliente.ClienteId, nombre = cliente.Nombre });

            return RedirectToAction(nameof(Index));
        }

        // === EDIT (GET) ===
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            PoblarProvincias(cliente.Direccion);

            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax) return PartialView("Edit", cliente);
            return View(cliente);
        }

        // === EDIT (POST) con soporte AJAX ===
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClienteId,Nombre,Correo,Telefono,Direccion")] Cliente form)
        {
            if (id != form.ClienteId) return NotFound();

            // Reglas de negocio
            if (!string.IsNullOrWhiteSpace(form.Telefono) &&
                !Regex.IsMatch(form.Telefono, @"^\d+$"))
            {
                ModelState.AddModelError(nameof(form.Telefono),
                    "El teléfono debe contener solo números.");
            }

            if (string.IsNullOrWhiteSpace(form.Correo) ||
                !new EmailAddressAttribute().IsValid(form.Correo))
            {
                ModelState.AddModelError(nameof(form.Correo),
                    "Formato de correo inválido.");
            }

            bool nombreRepetido = await _context.Clientes
                .AnyAsync(c => c.ClienteId != id && c.Nombre == form.Nombre);
            if (nombreRepetido)
            {
                ModelState.AddModelError(nameof(form.Nombre),
                    "Ya existe un cliente con ese nombre.");
            }

            bool correoRepetido = await _context.Clientes
                .AnyAsync(c => c.ClienteId != id && c.Correo == form.Correo);
            if (correoRepetido)
            {
                ModelState.AddModelError(nameof(form.Correo),
                    "Ese correo ya está registrado.");
            }

            if (string.IsNullOrWhiteSpace(form.Direccion) ||
                !Provincias.Contains(form.Direccion))
            {
                ModelState.AddModelError(nameof(form.Direccion),
                    "Debes seleccionar una provincia válida.");
            }

            // No bindeamos/validamos FechaRegistro
            ModelState.Remove(nameof(Cliente.FechaRegistro));

            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                PoblarProvincias(form.Direccion);
                if (isAjax) return PartialView("Edit", form);
                return View(form);
            }

            var entity = await _context.Clientes.FindAsync(id);
            if (entity == null) return NotFound();

            try
            {
                entity.Nombre = form.Nombre;
                entity.Correo = form.Correo;
                entity.Telefono = form.Telefono;
                entity.Direccion = form.Direccion;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(form.ClienteId)) return NotFound();
                throw;
            }

            if (isAjax) return Json(new { ok = true, id = id });
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ClienteId == id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // === Delete con validación de dependencias (Tareas, Diagnósticos) ===
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Solicitud de eliminación no válida.";
                return RedirectToAction(nameof(Index));
            }

            // IDs de vehículos del cliente
            var vehiculoIds = await _context.Vehiculos
                .Where(v => v.ClienteId == id)
                .Select(v => v.VehiculoId)
                .ToListAsync();

            var tareasCount = vehiculoIds.Count > 0
                ? await _context.TareasVehiculos.CountAsync(t => vehiculoIds.Contains(t.VehiculoId))
                : 0;

            var diagCount = vehiculoIds.Count > 0
                ? await _context.Diagnosticos.CountAsync(d => vehiculoIds.Contains(d.VehiculoId))
                : 0;

            if (tareasCount > 0 || diagCount > 0)
            {
                TempData["Error"] =
                    $"No se puede eliminar el cliente porque tiene información pendiente en sus vehículos: " +
                    $"{tareasCount} tarea(s) y {diagCount} diagnóstico(s). " +
                    $"Elimine primero esas tareas y diagnósticos desde sus respectivos módulos.";
                return RedirectToAction(nameof(Index));
            }

            var cliente = await _context.Clientes
                .Include(c => c.Vehiculos)
                .FirstOrDefaultAsync(c => c.ClienteId == id);

            if (cliente == null)
            {
                TempData["Error"] = "El cliente no existe o ya fue eliminado.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (cliente.Vehiculos != null && cliente.Vehiculos.Any())
                    _context.Vehiculos.RemoveRange(cliente.Vehiculos);

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cliente eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar el cliente: el registro tiene información asociada.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
            => _context.Clientes.Any(e => e.ClienteId == id);
    }
}

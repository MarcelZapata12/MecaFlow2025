using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Attributes;

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador")]
    public class ClientesController : Controller
    {
        private readonly MecaFlowContext _context;

        // Lista fija de provincias de Costa Rica
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

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ClienteId == id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            PoblarProvincias();
            return View();
        }

        // POST: Clientes/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ClienteId,Nombre,Correo,Telefono,Direccion,FechaRegistro")] Cliente cliente)
        {
            // Validaciones varias...
            if (!string.IsNullOrWhiteSpace(cliente.Telefono) &&
                !Regex.IsMatch(cliente.Telefono, @"^\d+$"))
                ModelState.AddModelError(nameof(cliente.Telefono),
                    "El teléfono debe contener solo números.");

            if (string.IsNullOrWhiteSpace(cliente.Correo) ||
                !new EmailAddressAttribute().IsValid(cliente.Correo))
                ModelState.AddModelError(nameof(cliente.Correo),
                    "Formato de correo inválido.");

            bool nombreRepetido = await _context.Clientes
                .AnyAsync(c => c.Nombre == cliente.Nombre);
            if (nombreRepetido)
                ModelState.AddModelError(nameof(cliente.Nombre),
                    "Ya existe un cliente con ese nombre.");

            bool correoRepetido = await _context.Clientes
                .AnyAsync(c => c.Correo == cliente.Correo);
            if (correoRepetido)
                ModelState.AddModelError(nameof(cliente.Correo),
                    "Ese correo ya está registrado.");

            // Validar que Dirección venga de la lista
            if (string.IsNullOrWhiteSpace(cliente.Direccion) ||
                !Provincias.Contains(cliente.Direccion))
                ModelState.AddModelError(nameof(cliente.Direccion),
                    "Debes seleccionar una provincia válida.");

            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si falla, repoblar el dropdown
            PoblarProvincias(cliente.Direccion);
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            PoblarProvincias(cliente.Direccion);
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ClienteId,Nombre,Correo,Telefono,Direccion,FechaRegistro")] Cliente cliente)
        {
            if (id != cliente.ClienteId) return NotFound();

            // Validaciones idénticas a Create
            if (!string.IsNullOrWhiteSpace(cliente.Telefono) &&
                !Regex.IsMatch(cliente.Telefono, @"^\d+$"))
                ModelState.AddModelError(nameof(cliente.Telefono),
                    "El teléfono debe contener solo números.");

            if (string.IsNullOrWhiteSpace(cliente.Correo) ||
                !new EmailAddressAttribute().IsValid(cliente.Correo))
                ModelState.AddModelError(nameof(cliente.Correo),
                    "Formato de correo inválido.");

            bool nombreRepetido = await _context.Clientes
                .AnyAsync(c => c.ClienteId != id && c.Nombre == cliente.Nombre);
            if (nombreRepetido)
                ModelState.AddModelError(nameof(cliente.Nombre),
                    "Ya existe un cliente con ese nombre.");

            bool correoRepetido = await _context.Clientes
                .AnyAsync(c => c.ClienteId != id && c.Correo == cliente.Correo);
            if (correoRepetido)
                ModelState.AddModelError(nameof(cliente.Correo),
                    "Ese correo ya está registrado.");

            if (string.IsNullOrWhiteSpace(cliente.Direccion) ||
                !Provincias.Contains(cliente.Direccion))
                ModelState.AddModelError(nameof(cliente.Direccion),
                    "Debes seleccionar una provincia válida.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.ClienteId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PoblarProvincias(cliente.Direccion);
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ClienteId == id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
            => _context.Clientes.Any(e => e.ClienteId == id);
    }
}

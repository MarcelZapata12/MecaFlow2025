using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Attributes;
using MecaFlow2025.Models;

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador")]
    public class EmpleadosController : Controller
    {
        private readonly MecaFlowContext _context;
        private bool IsAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        public EmpleadosController(MecaFlowContext context)
        {
            _context = context;
        }

        // INDEX: vista normal con tabla
        public async Task<IActionResult> Index()
        {
            var empleados = await _context.Empleados
                .OrderBy(e => e.Nombre)
                .ToListAsync();
            return View(empleados);
        }

        // DETAILS (GET) -> pensado para mostrarse en modal (Layout = null en la vista)
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(e => e.EmpleadoId == id);

            if (empleado == null) return NotFound();
            return View(empleado);
        }

        // ===== CREATE =====
        [HttpGet]
        public IActionResult Create()
        {
            if (IsAjax) return PartialView("Create");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Cedula,Correo,Puesto,Activo")] Empleado empleado)
        {
            // --- Reglas de negocio (igual estilo que Clientes) ---
            if (!string.IsNullOrWhiteSpace(empleado.Cedula) &&
                !Regex.IsMatch(empleado.Cedula, @"^\d+$"))
            {
                ModelState.AddModelError(nameof(empleado.Cedula),
                    "La cédula debe contener solo números.");
            }

            if (string.IsNullOrWhiteSpace(empleado.Correo) ||
                !new EmailAddressAttribute().IsValid(empleado.Correo))
            {
                ModelState.AddModelError(nameof(empleado.Correo),
                    "Formato de correo inválido.");
            }

            // Unicidad
            if (!string.IsNullOrWhiteSpace(empleado.Cedula))
            {
                bool cedulaExiste = await _context.Empleados.AnyAsync(e => e.Cedula == empleado.Cedula);
                if (cedulaExiste)
                    ModelState.AddModelError(nameof(empleado.Cedula), "Ya existe un empleado con esta cédula.");
            }

            if (!string.IsNullOrWhiteSpace(empleado.Correo))
            {
                bool correoExiste = await _context.Empleados.AnyAsync(e => e.Correo == empleado.Correo);
                if (correoExiste)
                    ModelState.AddModelError(nameof(empleado.Correo), "Ese correo ya está registrado.");
            }

            // Campos que NO vienen del form: setear en servidor
            // (tu modelo usa DateOnly? y DateTime?) 
            // Quitar validación para que no estorben:
            ModelState.Remove(nameof(Empleado.FechaIngreso));
            ModelState.Remove(nameof(Empleado.FechaRegistro));

            if (!ModelState.IsValid)
            {
                if (IsAjax) return PartialView("Create", empleado);
                return View(empleado);
            }

            // Defaults de servidor
            empleado.FechaIngreso ??= DateOnly.FromDateTime(DateTime.Today);
            empleado.FechaRegistro ??= DateTime.Now;

            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();

            if (IsAjax) return Json(new { ok = true, id = empleado.EmpleadoId, nombre = empleado.Nombre });
            return RedirectToAction(nameof(Index));
        }

        // ===== EDIT =====
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            if (IsAjax) return PartialView("Edit", empleado);
            return View(empleado);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmpleadoId,Nombre,Cedula,Correo,Puesto,Activo")] Empleado form)
        {
            if (id != form.EmpleadoId) return NotFound();

            // Reglas de negocio (mismo estilo)
            if (!string.IsNullOrWhiteSpace(form.Cedula) &&
                !Regex.IsMatch(form.Cedula, @"^\d+$"))
            {
                ModelState.AddModelError(nameof(form.Cedula),
                    "La cédula debe contener solo números.");
            }

            if (string.IsNullOrWhiteSpace(form.Correo) ||
                !new EmailAddressAttribute().IsValid(form.Correo))
            {
                ModelState.AddModelError(nameof(form.Correo),
                    "Formato de correo inválido.");
            }

            // Unicidad excluyendo el propio registro
            if (!string.IsNullOrWhiteSpace(form.Cedula))
            {
                bool cedulaRepetida = await _context.Empleados
                    .AnyAsync(e => e.EmpleadoId != id && e.Cedula == form.Cedula);
                if (cedulaRepetida)
                    ModelState.AddModelError(nameof(form.Cedula), "Ya existe un empleado con esta cédula.");
            }

            if (!string.IsNullOrWhiteSpace(form.Correo))
            {
                bool correoRepetido = await _context.Empleados
                    .AnyAsync(e => e.EmpleadoId != id && e.Correo == form.Correo);
                if (correoRepetido)
                    ModelState.AddModelError(nameof(form.Correo), "Ese correo ya está registrado.");
            }

            // Quitar validación de campos no bindeados
            ModelState.Remove(nameof(Empleado.FechaIngreso));
            ModelState.Remove(nameof(Empleado.FechaRegistro));

            if (!ModelState.IsValid)
            {
                if (IsAjax) return PartialView("Edit", form);
                return View(form);
            }

            var entity = await _context.Empleados.FirstOrDefaultAsync(e => e.EmpleadoId == id);
            if (entity == null) return NotFound();

            try
            {
                // Mapear SOLO campos editables (no tocar fechas)
                entity.Nombre = form.Nombre;
                entity.Cedula = form.Cedula;
                entity.Correo = form.Correo;
                entity.Puesto = form.Puesto;
                entity.Activo = form.Activo;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmpleadoExists(form.EmpleadoId)) return NotFound();
                throw;
            }

            if (IsAjax) return Json(new { ok = true, id = id });
            return RedirectToAction(nameof(Index));
        }

        // ===== DELETE =====
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(e => e.EmpleadoId == id);

            if (empleado == null) return NotFound();
            return View(empleado); // pensado para modal (Layout = null)
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                var msg = "Solicitud de eliminación no válida.";
                if (IsAjax) return Json(new { ok = false, error = msg });
                TempData["Error"] = msg;
                return RedirectToAction(nameof(Index));
            }

            // Validación de dependencias mínimas (diagnósticos asignados)
            var diagCount = await _context.Diagnosticos.CountAsync(d => d.EmpleadoId == id);
            if (diagCount > 0)
            {
                var msg = $"No se puede eliminar el empleado: tiene {diagCount} diagnóstico(s) asociado(s). " +
                          $"Elimine o reasigne esos registros primero.";
                if (IsAjax) return Json(new { ok = false, error = msg });
                TempData["Error"] = msg;
                return RedirectToAction(nameof(Index));
            }

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                var msg = "El empleado no existe o ya fue eliminado.";
                if (IsAjax) return Json(new { ok = false, error = msg });
                TempData["Error"] = msg;
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Empleados.Remove(empleado);
                await _context.SaveChangesAsync();
                if (!IsAjax) TempData["Success"] = "Empleado eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                var msg = "No se puede eliminar el empleado: el registro tiene información asociada.";
                if (IsAjax) return Json(new { ok = false, error = msg });
                TempData["Error"] = msg;
            }

            if (IsAjax) return Json(new { ok = true });
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
            => _context.Empleados.Any(e => e.EmpleadoId == id);
    }
}

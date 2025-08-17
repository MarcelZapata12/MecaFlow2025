using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Attributes;

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador")]
    public class EmpleadosController : Controller
    {
        private readonly MecaFlowContext _context;

        public EmpleadosController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: Empleados
        public async Task<IActionResult> Index()
        {
            return View(await _context.Empleados.ToListAsync());
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.EmpleadoId == id);

            if (empleado == null) return NotFound();

            return View(empleado);
        }

        // GET: Empleados/Create
        public IActionResult Create() => View();

        // POST: Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            // Bind SOLO campos editables (sin fechas)
            [Bind("Nombre,Cedula,Correo,Puesto,Activo")] Empleado empleado)
        {
            // Defaults en servidor (si Activo es bool?, conserva este set)
            if (empleado.Activo == null) empleado.Activo = true;

            // >>>>>>> FECHAS (no vienen del form) <<<<<<<
            // FechaIngreso: tu modelo da error DateTime -> DateOnly?, así que convertimos:
            empleado.FechaIngreso = DateOnly.FromDateTime(DateTime.Today);

            // FechaRegistro: si es DateTime? (como Clientes)
            empleado.FechaRegistro ??= DateTime.Now;
            // Si en tu modelo FechaRegistro es DateOnly?, usa esta en su lugar:
            // empleado.FechaRegistro ??= DateOnly.FromDateTime(DateTime.Now);

            // Evitar validación de fechas (por si tienen [Required])
            ModelState.Remove(nameof(Empleado.FechaIngreso));
            ModelState.Remove(nameof(Empleado.FechaRegistro));

            if (!ModelState.IsValid) return View(empleado);

            _context.Add(empleado);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            // Si usas AJAX para el modal parcial:
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("EditPartial", empleado);

            return View(empleado);
        }

        // POST: Empleados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            // Bind SOLO campos editables (sin fechas)
            [Bind("EmpleadoId,Nombre,Cedula,Correo,Puesto,Activo")] Empleado form)
        {
            if (id != form.EmpleadoId) return NotFound();

            // No validar fechas que NO vienen del form
            ModelState.Remove(nameof(Empleado.FechaIngreso));
            ModelState.Remove(nameof(Empleado.FechaRegistro));

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("EditPartial", form);
                return View(form);
            }

            var empleadoDb = await _context.Empleados
                                           .FirstOrDefaultAsync(e => e.EmpleadoId == id);
            if (empleadoDb == null) return NotFound();

            // Mapear SOLO campos editables — NO tocar fechas
            empleadoDb.Nombre = form.Nombre;
            empleadoDb.Cedula = form.Cedula;
            empleadoDb.Correo = form.Correo;
            empleadoDb.Puesto = form.Puesto;
            empleadoDb.Activo = form.Activo;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmpleadoExists(form.EmpleadoId)) return NotFound();
                throw;
            }
        }

        // GET: Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.EmpleadoId == id);

            if (empleado == null) return NotFound();

            return View(empleado);
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
                _context.Empleados.Remove(empleado);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
            => _context.Empleados.Any(e => e.EmpleadoId == id);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Attributes;
using Microsoft.AspNetCore.Http; // Necesario para HttpContext.Session

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador", "Empleado")]
    public class AsistenciasController : Controller
    {
        private readonly MecaFlowContext _context;

        public AsistenciasController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: Asistencias
        public async Task<IActionResult> Index(string? nombre, DateOnly? fechaInicio, DateOnly? fechaFin)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            IQueryable<Asistencia> asistencias = _context.Asistencias.Include(a => a.Empleado);

            // Los administradores verán todas las asistencias y podrán aplicar filtros
            if (userRole == "Administrador")
            {
                ViewBag.IsAdminReadOnly = true;
                ViewBag.CurrentNombre = nombre;
                ViewBag.CurrentFechaInicio = fechaInicio;
                ViewBag.CurrentFechaFin = fechaFin;

                if (!string.IsNullOrEmpty(nombre))
                {
                    asistencias = asistencias.Where(a => a.Empleado.Nombre.Contains(nombre));
                }

                if (fechaInicio.HasValue)
                {
                    asistencias = asistencias.Where(a => a.Fecha >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    asistencias = asistencias.Where(a => a.Fecha <= fechaFin.Value);
                }
            }
            // Si el usuario es un Empleado, solo puede ver sus propias asistencias y no se aplican filtros
            else if (userRole == "Empleado" && !string.IsNullOrEmpty(userEmail))
            {
                var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Correo == userEmail);
                if (empleado != null)
                {
                    asistencias = asistencias.Where(a => a.EmpleadoId == empleado.EmpleadoId);
                }
                else
                {
                    // En caso de que el empleado no se encuentre, se devuelve una lista vacía.
                    asistencias = asistencias.Where(a => false);
                }
                ViewBag.IsAdminReadOnly = false;
            }

            return View(await asistencias.ToListAsync());
        }

        // GET: Asistencias/RegistroPersonal
        [HttpGet]
        public async Task<IActionResult> RegistroPersonal()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Correo == userEmail);

            if (empleado == null)
            {
                TempData["Error"] = "No se pudo encontrar la información del empleado.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Obtener la fecha local de Costa Rica para evitar el desfase
            var zonaHorariaCR = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            var fechaLocalCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaCR).Date;

            // ✅ Usar la fecha local para buscar la asistencia
            var asistenciaAbierta = await _context.Asistencias
                .FirstOrDefaultAsync(a => a.EmpleadoId == empleado.EmpleadoId && a.Fecha == DateOnly.FromDateTime(fechaLocalCR) && !a.HoraSalida.HasValue);

            ViewBag.AsistenciaAbierta = asistenciaAbierta;
            ViewBag.EmpleadoNombre = empleado.Nombre;

            return View();
        }

        // POST: Asistencias/RegistrarEntrada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarEntrada()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Correo == userEmail);

            if (empleado == null)
            {
                TempData["Error"] = "No se pudo encontrar la información del empleado.";
                return RedirectToAction(nameof(RegistroPersonal));
            }

            // ✅ Obtener la fecha y hora locales de Costa Rica
            var zonaHorariaCR = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            var fechaHoraLocalCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaCR);

            // ✅ Usar la fecha local de Costa Rica para la verificación
            var asistenciaHoy = await _context.Asistencias
                .FirstOrDefaultAsync(a => a.EmpleadoId == empleado.EmpleadoId && a.Fecha == DateOnly.FromDateTime(fechaHoraLocalCR.Date));

            if (asistenciaHoy != null)
            {
                TempData["Error"] = "Ya se ha registrado una entrada para el día de hoy.";
                return RedirectToAction(nameof(RegistroPersonal));
            }

            var nuevaAsistencia = new Asistencia
            {
                EmpleadoId = empleado.EmpleadoId,
                // ✅ Guardar la fecha y la hora local de Costa Rica
                Fecha = DateOnly.FromDateTime(fechaHoraLocalCR.Date),
                HoraEntrada = TimeOnly.FromTimeSpan(fechaHoraLocalCR.TimeOfDay)
            };

            _context.Asistencias.Add(nuevaAsistencia);
            await _context.SaveChangesAsync();

            TempData["Success"] = "¡Entrada registrada con éxito!";
            return RedirectToAction(nameof(RegistroPersonal));
        }

        // POST: Asistencias/RegistrarSalida
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSalida()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Correo == userEmail);

            if (empleado == null)
            {
                TempData["Error"] = "No se pudo encontrar la información del empleado.";
                return RedirectToAction(nameof(RegistroPersonal));
            }

            // ✅ Obtener la fecha y hora locales de Costa Rica
            var zonaHorariaCR = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            var fechaHoraLocalCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaCR);

            // ✅ Usar la fecha local de Costa Rica para buscar la asistencia abierta del empleado para hoy
            var asistencia = await _context.Asistencias
                .FirstOrDefaultAsync(a => a.EmpleadoId == empleado.EmpleadoId && a.Fecha == DateOnly.FromDateTime(fechaHoraLocalCR.Date) && !a.HoraSalida.HasValue);

            if (asistencia == null)
            {
                TempData["Error"] = "No hay una entrada de hoy sin registrar una salida.";
                return RedirectToAction(nameof(RegistroPersonal));
            }

            // ✅ Guardar la hora de salida local de Costa Rica
            asistencia.HoraSalida = TimeOnly.FromTimeSpan(fechaHoraLocalCR.TimeOfDay);
            _context.Asistencias.Update(asistencia);
            await _context.SaveChangesAsync();

            TempData["Success"] = "¡Salida registrada con éxito!";
            return RedirectToAction(nameof(RegistroPersonal));
        }

        // GET: Asistencias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var asistencia = await _context.Asistencias.Include(a => a.Empleado).FirstOrDefaultAsync(m => m.AsistenciaId == id);
            if (asistencia == null) return NotFound();
            return View(asistencia);
        }

        // GET: Asistencias/Create
        public IActionResult Create()
        {
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "EmpleadoId", "Cedula");
            return View();
        }

        // POST: Asistencias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AsistenciaId,EmpleadoId,Fecha,HoraLlegada,HoraSalida,FechaRegistro")] Asistencia asistencia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asistencia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "EmpleadoId", "Cedula", asistencia.EmpleadoId);
            return View(asistencia);
        }

        // GET: Asistencias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var asistencia = await _context.Asistencias.FindAsync(id);
            if (asistencia == null) return NotFound();
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "EmpleadoId", "Cedula", asistencia.EmpleadoId);
            return View(asistencia);
        }

        // POST: Asistencias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AsistenciaId,EmpleadoId,Fecha,HoraLlegada,HoraSalida,FechaRegistro")] Asistencia asistencia)
        {
            if (id != asistencia.AsistenciaId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asistencia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsistenciaExists(asistencia.AsistenciaId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "EmpleadoId", "Cedula", asistencia.EmpleadoId);
            return View(asistencia);
        }

        // GET: Asistencias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var asistencia = await _context.Asistencias.Include(a => a.Empleado).FirstOrDefaultAsync(m => m.AsistenciaId == id);
            if (asistencia == null) return NotFound();
            return View(asistencia);
        }

        // POST: Asistencias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asistencia = await _context.Asistencias.FindAsync(id);
            if (asistencia != null) _context.Asistencias.Remove(asistencia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsistenciaExists(int id) => _context.Asistencias.Any(e => e.AsistenciaId == id);
    }
}
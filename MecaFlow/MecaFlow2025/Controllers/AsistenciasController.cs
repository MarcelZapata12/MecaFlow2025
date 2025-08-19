using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Attributes;

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

        // GET: Asistencias (solo listado general; útil para admin)
        public async Task<IActionResult> Index()
        {
            var asistencias = await _context.Asistencias
                .Include(a => a.Empleado)
                .OrderByDescending(a => a.Fecha)
                .ThenByDescending(a => a.HoraEntrada)
                .ToListAsync();

            return View(asistencias);
        }

        // -------- REGISTRO PERSONAL (solo el usuario logueado) --------

        // GET: Asistencias/RegistroPersonal
        public async Task<IActionResult> RegistroPersonal()
        {
            try
            {
                var empleadoActual = await GetEmpleadoActualAsync();
                if (empleadoActual == null)
                {
                    TempData["Error"] = "No se pudo identificar al usuario en sesión o no está activo.";
                    ViewBag.AsistenciasHoy = new List<Asistencia>();
                    ViewBag.EmpleadoActual = null;
                    return View();
                }

                ViewBag.EmpleadoActual = new
                {
                    empleadoActual.EmpleadoId,
                    empleadoActual.Nombre,
                    empleadoActual.Puesto
                };

                var hoy = DateOnly.FromDateTime(DateTime.Today);

                // Asistencias de HOY solo del empleado actual
                var asistenciasHoy = await _context.Asistencias
                    .Include(a => a.Empleado)
                    .Where(a => a.Fecha == hoy && a.EmpleadoId == empleadoActual.EmpleadoId)
                    .OrderBy(a => a.Empleado.Nombre)
                    .ToListAsync();

                ViewBag.AsistenciasHoy = asistenciasHoy;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los datos: " + ex.Message;
                ViewBag.AsistenciasHoy = new List<Asistencia>();
                ViewBag.EmpleadoActual = null;
            }

            return View();
        }

        // POST: Registrar Entrada (sin parámetros; usa el usuario en sesión)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarEntrada()
        {
            try
            {
                var empleado = await GetEmpleadoActualAsync();
                if (empleado == null)
                {
                    TempData["Error"] = "Sesión inválida o empleado inactivo.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                var hoy = DateOnly.FromDateTime(DateTime.Today);
                var horaActual = TimeOnly.FromDateTime(DateTime.Now);

                var asistencia = await _context.Asistencias
                    .FirstOrDefaultAsync(a => a.EmpleadoId == empleado.EmpleadoId && a.Fecha == hoy);

                if (asistencia != null)
                {
                    if (asistencia.HoraEntrada.HasValue)
                    {
                        TempData["Error"] = $"{empleado.Nombre} ya registró su entrada hoy a las {asistencia.HoraEntrada.Value:HH:mm}.";
                    }
                    else
                    {
                        asistencia.HoraEntrada = horaActual;
                        _context.Update(asistencia);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = $"Entrada registrada: {empleado.Nombre} {horaActual:HH:mm}.";
                    }
                }
                else
                {
                    _context.Add(new Asistencia
                    {
                        EmpleadoId = empleado.EmpleadoId,
                        Fecha = hoy,
                        HoraEntrada = horaActual
                    });
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Entrada registrada: {empleado.Nombre} {horaActual:HH:mm}.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar la entrada: " + ex.Message;
            }

            return RedirectToAction(nameof(RegistroPersonal));
        }

        // POST: Registrar Salida (sin parámetros; usa el usuario en sesión)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSalida()
        {
            try
            {
                var empleado = await GetEmpleadoActualAsync();
                if (empleado == null)
                {
                    TempData["Error"] = "Sesión inválida o empleado inactivo.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                var hoy = DateOnly.FromDateTime(DateTime.Today);
                var horaActual = TimeOnly.FromDateTime(DateTime.Now);

                var asistencia = await _context.Asistencias
                    .FirstOrDefaultAsync(a => a.EmpleadoId == empleado.EmpleadoId && a.Fecha == hoy);

                if (asistencia == null)
                {
                    TempData["Error"] = $"No hay entrada registrada para {empleado.Nombre} el día de hoy.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                if (!asistencia.HoraEntrada.HasValue)
                {
                    TempData["Error"] = $"Primero registra la entrada.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                if (asistencia.HoraSalida.HasValue)
                {
                    TempData["Error"] = $"Ya registraste salida hoy a las {asistencia.HoraSalida.Value:HH:mm}.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                if (horaActual <= asistencia.HoraEntrada.Value)
                {
                    TempData["Error"] = "La hora de salida debe ser posterior a la de entrada.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                asistencia.HoraSalida = horaActual;
                _context.Update(asistencia);
                await _context.SaveChangesAsync();

                var diff = horaActual.ToTimeSpan() - asistencia.HoraEntrada.Value.ToTimeSpan();
                TempData["Success"] = $"Salida registrada: {empleado.Nombre} {horaActual:HH:mm}. Tiempo: {(int)diff.TotalHours}h {diff.Minutes}m.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar la salida: " + ex.Message;
            }

            return RedirectToAction(nameof(RegistroPersonal));
        }

        // GET: Asistencias por empleado
        // - Empleado: siempre ve SOLO las suyas (ignora empleadoId externo)
        // - Admin: puede consultar a quien guste (empleadoId requerido)
        public async Task<IActionResult> MisAsistencias(int empleadoId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            Empleado? empleadoConsulta;
            if (userRole == "Empleado")
            {
                empleadoConsulta = await GetEmpleadoActualAsync();
                if (empleadoConsulta == null)
                {
                    TempData["Error"] = "No se pudo identificar al usuario.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }
            }
            else // Administrador
            {
                empleadoConsulta = await _context.Empleados.FindAsync(empleadoId);
                if (empleadoConsulta == null)
                {
                    return NotFound();
                }
            }

            // Rango por defecto: mes actual
            if (!fechaInicio.HasValue)
                fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (!fechaFin.HasValue)
                fechaFin = fechaInicio.Value.AddMonths(1).AddDays(-1);

            var fechaInicioOnly = DateOnly.FromDateTime(fechaInicio.Value);
            var fechaFinOnly = DateOnly.FromDateTime(fechaFin.Value);

            var asistencias = await _context.Asistencias
                .Include(a => a.Empleado)
                .Where(a => a.EmpleadoId == empleadoConsulta.EmpleadoId &&
                            a.Fecha >= fechaInicioOnly &&
                            a.Fecha <= fechaFinOnly)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            ViewBag.Empleado = empleadoConsulta;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(asistencias);
        }

        // -------- Helpers --------

        private async Task<Empleado?> GetEmpleadoActualAsync()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            // Si manejas "Usuario" o "Correo" en Empleado, puedes ampliar la condición aquí.
            return await _context.Empleados
                .FirstOrDefaultAsync(e => e.Activo && e.Nombre == userName);
        }

        private TimeSpan? CalcularHorasTrabajadas(TimeOnly? entrada, TimeOnly? salida)
        {
            if (entrada.HasValue && salida.HasValue)
            {
                return salida.Value.ToTimeSpan() - entrada.Value.ToTimeSpan();
            }
            return null;
        }
    }
}
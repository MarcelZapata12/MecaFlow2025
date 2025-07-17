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

        // GET: Asistencias
        public async Task<IActionResult> Index()
        {
            var asistencias = await _context.Asistencias
                .Include(a => a.Empleado)
                .OrderByDescending(a => a.Fecha)
                .ThenByDescending(a => a.HoraEntrada)
                .ToListAsync();

            return View(asistencias);
        }

        // GET: Asistencias/RegistroPersonal
        public async Task<IActionResult> RegistroPersonal()
        {
            try
            {
                // Obtener el rol del usuario actual
                var userRole = HttpContext.Session.GetString("UserRole");
                var userName = HttpContext.Session.GetString("UserName");

                List<object> empleadosParaDropdown = new List<object>();

                if (userRole == "Administrador")
                {
                    // Si es administrador, mostrar todos los empleados activos
                    var empleadosActivos = await _context.Empleados
                        .Where(e => e.Activo)
                        .OrderBy(e => e.Nombre)
                        .Select(e => new { e.EmpleadoId, e.Nombre })
                        .ToListAsync();

                    empleadosParaDropdown = empleadosActivos.Cast<object>().ToList();
                }
                else if (userRole == "Empleado")
                {
                    // Si es empleado, mostrar solo su propio registro
                    // Buscar el empleado por el nombre de usuario (asumiendo que el UserName corresponde al nombre del empleado)
                    var empleadoActual = await _context.Empleados
                        .Where(e => e.Activo && e.Nombre == userName)
                        .Select(e => new { e.EmpleadoId, e.Nombre })
                        .FirstOrDefaultAsync();

                    if (empleadoActual != null)
                    {
                        empleadosParaDropdown.Add(empleadoActual);
                    }
                }

                ViewBag.Empleados = new SelectList(empleadosParaDropdown, "EmpleadoId", "Nombre");
                ViewBag.UserRole = userRole;

                // Obtener asistencias del día actual
                var hoy = DateOnly.FromDateTime(DateTime.Today);
                var asistenciasHoy = await _context.Asistencias
                    .Include(a => a.Empleado)
                    .Where(a => a.Fecha == hoy)
                    .OrderBy(a => a.Empleado.Nombre)
                    .ToListAsync();

                // Si es empleado, filtrar solo sus asistencias
                if (userRole == "Empleado")
                {
                    asistenciasHoy = asistenciasHoy.Where(a => a.Empleado.Nombre == userName).ToList();
                }

                ViewBag.AsistenciasHoy = asistenciasHoy;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los datos: " + ex.Message;
                ViewBag.Empleados = new SelectList(new List<object>(), "EmpleadoId", "Nombre");
                ViewBag.AsistenciasHoy = new List<Asistencia>();
                ViewBag.UserRole = "Empleado";
            }

            return View();
        }

        // POST: Registrar Entrada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarEntrada(int empleadoId)
        {
            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado válido.";
                return RedirectToAction(nameof(RegistroPersonal));
            }

            try
            {
                // Validar permisos: si es empleado, solo puede registrar su propia asistencia
                var userRole = HttpContext.Session.GetString("UserRole");
                var userName = HttpContext.Session.GetString("UserName");

                var empleado = await _context.Empleados
                    .FirstOrDefaultAsync(e => e.EmpleadoId == empleadoId && e.Activo);

                if (empleado == null)
                {
                    TempData["Error"] = "El empleado seleccionado no existe o no está activo.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                // Validar que el empleado solo pueda registrar su propia asistencia
                if (userRole == "Empleado" && empleado.Nombre != userName)
                {
                    TempData["Error"] = "Solo puedes registrar tu propia asistencia.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                var hoy = DateOnly.FromDateTime(DateTime.Today);
                var horaActual = TimeOnly.FromDateTime(DateTime.Now);

                // Verificar si ya existe un registro de asistencia para hoy
                var asistenciaExistente = await _context.Asistencias
                    .FirstOrDefaultAsync(a => a.EmpleadoId == empleadoId && a.Fecha == hoy);

                if (asistenciaExistente != null)
                {
                    if (asistenciaExistente.HoraEntrada.HasValue)
                    {
                        TempData["Error"] = $"{empleado.Nombre} ya ha registrado su entrada el día de hoy a las {asistenciaExistente.HoraEntrada.Value:HH:mm}.";
                        return RedirectToAction(nameof(RegistroPersonal));
                    }
                    else
                    {
                        // Actualizar la hora de entrada si no estaba registrada
                        asistenciaExistente.HoraEntrada = horaActual;
                        _context.Update(asistenciaExistente);
                    }
                }
                else
                {
                    // Crear nuevo registro de asistencia
                    var nuevaAsistencia = new Asistencia
                    {
                        EmpleadoId = empleadoId,
                        Fecha = hoy,
                        HoraEntrada = horaActual
                    };
                    _context.Add(nuevaAsistencia);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Entrada registrada correctamente para {empleado.Nombre} a las {horaActual:HH:mm}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar la entrada: " + ex.Message;
            }

            return RedirectToAction(nameof(RegistroPersonal));
        }

        // POST: Registrar Salida
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSalida(int empleadoId)
        {
            if (empleadoId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un empleado válido.";
                return RedirectToAction(nameof(RegistroPersonal));
            }

            try
            {
                // Validar permisos: si es empleado, solo puede registrar su propia asistencia
                var userRole = HttpContext.Session.GetString("UserRole");
                var userName = HttpContext.Session.GetString("UserName");

                var empleado = await _context.Empleados
                    .FirstOrDefaultAsync(e => e.EmpleadoId == empleadoId && e.Activo);

                if (empleado == null)
                {
                    TempData["Error"] = "El empleado seleccionado no existe o no está activo.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                // Validar que el empleado solo pueda registrar su propia asistencia
                if (userRole == "Empleado" && empleado.Nombre != userName)
                {
                    TempData["Error"] = "Solo puedes registrar tu propia asistencia.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                var hoy = DateOnly.FromDateTime(DateTime.Today);
                var horaActual = TimeOnly.FromDateTime(DateTime.Now);

                // Buscar el registro de asistencia del día
                var asistencia = await _context.Asistencias
                    .FirstOrDefaultAsync(a => a.EmpleadoId == empleadoId && a.Fecha == hoy);

                if (asistencia == null)
                {
                    TempData["Error"] = $"No se encontró registro de entrada para {empleado.Nombre} el día de hoy.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                if (!asistencia.HoraEntrada.HasValue)
                {
                    TempData["Error"] = $"{empleado.Nombre} debe registrar su entrada antes de registrar la salida.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                if (asistencia.HoraSalida.HasValue)
                {
                    TempData["Error"] = $"{empleado.Nombre} ya ha registrado su salida el día de hoy a las {asistencia.HoraSalida.Value:HH:mm}.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                // Validar que la hora de salida sea posterior a la de entrada
                if (horaActual <= asistencia.HoraEntrada.Value)
                {
                    TempData["Error"] = "La hora de salida debe ser posterior a la hora de entrada.";
                    return RedirectToAction(nameof(RegistroPersonal));
                }

                // Registrar la hora de salida
                asistencia.HoraSalida = horaActual;
                _context.Update(asistencia);
                await _context.SaveChangesAsync();

                // Calcular horas trabajadas
                var horasTrabajadas = horaActual.ToTimeSpan() - asistencia.HoraEntrada.Value.ToTimeSpan();
                var horas = (int)horasTrabajadas.TotalHours;
                var minutos = horasTrabajadas.Minutes;

                TempData["Success"] = $"Salida registrada correctamente para {empleado.Nombre} a las {horaActual:HH:mm}. Tiempo trabajado: {horas}h {minutos}m.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar la salida: " + ex.Message;
            }

            return RedirectToAction(nameof(RegistroPersonal));
        }

        // GET: Asistencias por empleado
        public async Task<IActionResult> MisAsistencias(int empleadoId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Validar permisos: si es empleado, solo puede ver sus propias asistencias
            var userRole = HttpContext.Session.GetString("UserRole");
            var userName = HttpContext.Session.GetString("UserName");

            var empleado = await _context.Empleados.FindAsync(empleadoId);
            if (empleado == null)
            {
                return NotFound();
            }

            // Validar que el empleado solo pueda ver sus propias asistencias
            if (userRole == "Empleado" && empleado.Nombre != userName)
            {
                TempData["Error"] = "Solo puedes ver tus propias asistencias.";
                return RedirectToAction(nameof(RegistroPersonal));
            }

            // Si no se especifican fechas, mostrar las asistencias del mes actual
            if (!fechaInicio.HasValue)
                fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (!fechaFin.HasValue)
                fechaFin = fechaInicio.Value.AddMonths(1).AddDays(-1);

            var fechaInicioOnly = DateOnly.FromDateTime(fechaInicio.Value);
            var fechaFinOnly = DateOnly.FromDateTime(fechaFin.Value);

            var asistencias = await _context.Asistencias
                .Where(a => a.EmpleadoId == empleadoId &&
                           a.Fecha >= fechaInicioOnly &&
                           a.Fecha <= fechaFinOnly)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            ViewBag.Empleado = empleado;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(asistencias);
        }

        // Método auxiliar para calcular horas trabajadas
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
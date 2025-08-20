using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Attributes;
using MecaFlow2025.Models;
using System.Security.Cryptography;
using System.Text;

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
        public async Task<IActionResult> Create(CreateEmpleadoViewModel model)
        {
            // --- Reglas de negocio ---
            if (!string.IsNullOrWhiteSpace(model.Cedula) &&
                !Regex.IsMatch(model.Cedula, @"^\d+$"))
            {
                ModelState.AddModelError(nameof(model.Cedula),
                    "La cédula debe contener solo números.");
            }

            if (string.IsNullOrWhiteSpace(model.Correo) ||
                !new EmailAddressAttribute().IsValid(model.Correo))
            {
                ModelState.AddModelError(nameof(model.Correo),
                    "Formato de correo inválido.");
            }

            // Unicidad de cédula
            if (!string.IsNullOrWhiteSpace(model.Cedula))
            {
                bool cedulaExiste = await _context.Empleados.AnyAsync(e => e.Cedula == model.Cedula);
                if (cedulaExiste)
                    ModelState.AddModelError(nameof(model.Cedula), "Ya existe un empleado con esta cédula.");
            }

            // Unicidad de correo en Empleados
            if (!string.IsNullOrWhiteSpace(model.Correo))
            {
                bool correoExiste = await _context.Empleados.AnyAsync(e => e.Correo == model.Correo);
                if (correoExiste)
                    ModelState.AddModelError(nameof(model.Correo), "Ese correo ya está registrado en empleados.");

                // También verificar que no exista en Usuarios
                bool correoEnUsuarios = await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo);
                if (correoEnUsuarios)
                    ModelState.AddModelError(nameof(model.Correo), "Ya existe un usuario con este correo electrónico.");
            }

            if (!ModelState.IsValid)
            {
                if (IsAjax) return PartialView("Create", model);
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Crear el empleado
                var empleado = new Empleado
                {
                    Nombre = model.Nombre,
                    Cedula = model.Cedula,
                    Correo = model.Correo,
                    Puesto = model.Puesto,
                    Activo = model.Activo,
                    FechaIngreso = DateOnly.FromDateTime(DateTime.Today),
                    FechaRegistro = DateTime.Now
                };

                _context.Empleados.Add(empleado);
                await _context.SaveChangesAsync();

                // 2. Crear usuario para el empleado
                var usuario = new Usuario
                {
                    Username = model.Correo,
                    Correo = model.Correo,
                    PasswordHash = HashPassword(model.Contrasena),
                    FechaCreacion = DateTime.Now
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // 3. Asignar rol según el puesto seleccionado
                string rolNombre = model.Puesto == "Administrador" ? "Administrador" : "Empleado";
                var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == rolNombre);
                if (rol == null)
                {
                    rol = new Role { Nombre = rolNombre };
                    _context.Roles.Add(rol);
                    await _context.SaveChangesAsync();
                }

                usuario.Rols ??= new List<Role>();
                usuario.Rols.Add(rol);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                if (IsAjax) return Json(new { ok = true, id = empleado.EmpleadoId, nombre = empleado.Nombre });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error al crear el empleado y su usuario. Inténtelo de nuevo.");

                if (IsAjax) return PartialView("Create", model);
                return View(model);
            }
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Obtener el correo original antes de actualizar
                string correoOriginal = entity.Correo;

                // Mapear SOLO campos editables (no tocar fechas)
                entity.Nombre = form.Nombre;
                entity.Cedula = form.Cedula;
                entity.Correo = form.Correo;
                entity.Puesto = form.Puesto;
                entity.Activo = form.Activo;

                await _context.SaveChangesAsync();

                // Actualizar el rol del usuario correspondiente
                var usuario = await _context.Usuarios
                    .Include(u => u.Rols)
                    .FirstOrDefaultAsync(u => u.Correo == correoOriginal || u.Correo == form.Correo);

                bool rolActualizado = false;
                var usuarioLogueadoId = HttpContext.Session.GetString("UserId");

                if (usuario != null)
                {
                    // Actualizar el correo del usuario si cambió
                    if (usuario.Correo != form.Correo)
                    {
                        usuario.Correo = form.Correo;
                        usuario.Username = form.Correo;
                    }

                    // Determinar el nuevo rol según el puesto
                    string nuevoRolNombre = form.Puesto == "Administrador" ? "Administrador" : "Empleado";

                    // Verificar si ya tiene el rol correcto
                    var rolActual = usuario.Rols.FirstOrDefault();
                    if (rolActual == null || rolActual.Nombre != nuevoRolNombre)
                    {
                        // Remover todos los roles actuales
                        usuario.Rols.Clear();

                        // Buscar o crear el nuevo rol
                        var nuevoRol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nuevoRolNombre);
                        if (nuevoRol == null)
                        {
                            nuevoRol = new Role { Nombre = nuevoRolNombre };
                            _context.Roles.Add(nuevoRol);
                            await _context.SaveChangesAsync();
                        }

                        // Asignar el nuevo rol
                        usuario.Rols.Add(nuevoRol);
                        await _context.SaveChangesAsync();

                        // *** ACTUALIZAR SESIÓN SI ES EL USUARIO ACTUAL ***
                        if (!string.IsNullOrEmpty(usuarioLogueadoId) &&
                            int.TryParse(usuarioLogueadoId, out int loggedUserId) &&
                            usuario.UsuarioId == loggedUserId)
                        {
                            // Actualizar el rol en la sesión
                            HttpContext.Session.SetString("UserRole", nuevoRolNombre);
                            // Actualizar el nombre en la sesión también
                            HttpContext.Session.SetString("UserName", form.Nombre);
                            rolActualizado = true;
                        }
                    }
                }

                await transaction.CommitAsync();

                if (IsAjax)
                {
                    if (rolActualizado)
                    {
                        return Json(new { ok = true, id = id, roleUpdated = true, newRole = form.Puesto });
                    }
                    return Json(new { ok = true, id = id });
                }

                if (rolActualizado)
                {
                    TempData["RoleUpdated"] = "Tu rol ha sido actualizado. Los cambios se han aplicado inmediatamente.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                if (!EmpleadoExists(form.EmpleadoId)) return NotFound();
                throw;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error al actualizar el empleado y su usuario. Inténtelo de nuevo.");

                if (IsAjax) return PartialView("Edit", form);
                return View(form);
            }
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
                // También eliminar el usuario asociado si existe
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == empleado.Correo);
                if (usuario != null)
                {
                    _context.Usuarios.Remove(usuario);
                }

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

        // Método para hashear contraseñas (igual que en AuthController)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
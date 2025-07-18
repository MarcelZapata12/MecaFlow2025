using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using System.Security.Cryptography;
using System.Text;

namespace MecaFlow2025.Controllers
{
    public class AuthController : Controller
    {
        private readonly MecaFlowContext _context;

        public AuthController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el usuario ya existe por correo
                var existingUserByEmail = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Correo == model.Correo);

                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Correo", "Ya existe un usuario con este correo electrónico");
                    return View(model);
                }

                var existingEmpleado = await _context.Empleados
                    .FirstOrDefaultAsync(e => e.Cedula == model.Cedula);

                if (existingEmpleado != null)
                {
                    ModelState.AddModelError("Cedula", "Ya existe un empleado registrado con esta cédula");
                    return View(model);
                }

                // Crear nuevo usuario
                var usuario = new Usuario
                {
                    Username = model.Correo, // Usar el correo como username
                    Correo = model.Correo,
                    PasswordHash = HashPassword(model.Contrasena),
                    FechaCreacion = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Buscar o crear el rol
                var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == model.Rol);
                if (rol == null)
                {
                    rol = new Role { Nombre = model.Rol };
                    _context.Roles.Add(rol);
                    await _context.SaveChangesAsync();
                }

                // Asignar rol al usuario
                usuario.Rols.Add(rol);
                await _context.SaveChangesAsync();

                // Crear registro adicional según el rol
                switch (model.Rol)
                {
                    case "Empleado":
                        var empleado = new Empleado
                        {
                            Nombre = $"{model.Nombre} {model.Apellido}",
                            Cedula = model.Cedula,
                            Correo = model.Correo,
                            Puesto = "Por definir",
                            FechaIngreso = DateOnly.FromDateTime(DateTime.Now),
                            Activo = true,
                            FechaRegistro = DateTime.Now
                        };
                        _context.Empleados.Add(empleado);
                        break;

                    case "Cliente":
                        var cliente = new Cliente
                        {
                            Nombre = $"{model.Nombre} {model.Apellido}",
                            Telefono = model.Telefono,
                            Correo = model.Correo,
                            FechaRegistro = DateTime.Now
                        };
                        _context.Clientes.Add(cliente);
                        break;

                    case "Administrador":
                        // Para administrador, crear también un registro en Empleados
                        var administrador = new Empleado
                        {
                            Nombre = $"{model.Nombre} {model.Apellido}",
                            Cedula = model.Cedula,
                            Correo = model.Correo,
                            Puesto = "Administrador",
                            FechaIngreso = DateOnly.FromDateTime(DateTime.Now),
                            Activo = true,
                            FechaRegistro = DateTime.Now
                        };
                        _context.Empleados.Add(administrador);
                        break;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registro exitoso. Por favor, inicia sesión.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Rols)
                    .FirstOrDefaultAsync(u => u.Correo == model.Correo);

                if (usuario != null && VerifyPassword(model.Contrasena, usuario.PasswordHash))
                {
                    // Configurar sesión
                    HttpContext.Session.SetString("UserId", usuario.UsuarioId.ToString());
                    HttpContext.Session.SetString("UserEmail", usuario.Correo);

                    var userRole = usuario.Rols.FirstOrDefault()?.Nombre ?? "Usuario";
                    HttpContext.Session.SetString("UserRole", userRole);

                    // Obtener el nombre completo dependiendo del rol
                    string nombreCompleto = "Usuario";
                    if (userRole == "Cliente")
                    {
                        var cliente = await _context.Clientes
                            .FirstOrDefaultAsync(c => c.Correo == usuario.Correo);
                        if (cliente != null)
                        {
                            nombreCompleto = cliente.Nombre;
                        }
                    }
                    else if (userRole == "Empleado" || userRole == "Administrador")
                    {
                        var empleado = await _context.Empleados
                            .FirstOrDefaultAsync(e => e.Correo == usuario.Correo);
                        if (empleado != null)
                        {
                            nombreCompleto = empleado.Nombre;
                        }
                    }

                    HttpContext.Session.SetString("UserName", nombreCompleto);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Correo o contraseña incorrectos");
                }
            }

            return View(model);
        }

        // GET: Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Método para hashear contraseñas
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Método para verificar contraseñas
        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }

        public IActionResult AccessDenied()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Services;
using System.Security.Cryptography;
using System.Text;

namespace MecaFlow2025.Controllers
{
    public class AuthController : Controller
    {
        private readonly MecaFlowContext _context;
        private readonly IEmailService _emailService;

        public AuthController(MecaFlowContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
            if (!ModelState.IsValid)
                return View(model);

            // 1) ¿Correo ya existe?
            var existingUserByEmail = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == model.Correo);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario con este correo electrónico");
                return View(model);
            }

            // 2) Evita colisión con Empleados solo si vino cédula
            if (!string.IsNullOrWhiteSpace(model.Cedula))
            {
                var existingEmpleado = await _context.Empleados
                    .FirstOrDefaultAsync(e => e.Cedula == model.Cedula);
                if (existingEmpleado != null)
                {
                    ModelState.AddModelError("Cedula", "Ya existe un empleado registrado con esta cédula");
                    return View(model);
                }
            }

            // 3) Crear usuario
            var usuario = new Usuario
            {
                Username = model.Correo,
                Correo = model.Correo,
                PasswordHash = HashPassword(model.Contrasena),
                FechaCreacion = DateTime.Now
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // 4) Asignar SIEMPRE rol "Cliente" (hardcoded server-side)
            const string rolPorDefecto = "Cliente";
            var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == rolPorDefecto);
            if (rolCliente == null)
            {
                rolCliente = new Role { Nombre = rolPorDefecto };
                _context.Roles.Add(rolCliente);
                await _context.SaveChangesAsync();
            }

            usuario.Rols ??= new List<Role>();
            usuario.Rols.Add(rolCliente);

            // 5) Crear registro en Clientes
            var cliente = new Cliente
            {
                Nombre = $"{model.Nombre} {model.Apellido}".Trim(),
                Telefono = model.Telefono,
                Correo = model.Correo,
                FechaRegistro = DateTime.Now
            };
            _context.Clientes.Add(cliente);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registro exitoso. Por favor, inicia sesión.";
            return RedirectToAction("Login");
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

        // GET: Auth/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Correo == model.Email);

                if (usuario != null)
                {
                    // Invalidar todos los tokens anteriores para este usuario
                    var tokensAnteriores = await _context.PasswordResetTokens
                        .Where(t => t.UsuarioId == usuario.UsuarioId && !t.Usado)
                        .ToListAsync();

                    foreach (var token in tokensAnteriores)
                    {
                        token.Usado = true;
                    }

                    // Generar nuevo token
                    var resetToken = GenerateResetToken();
                    var passwordResetToken = new PasswordResetToken
                    {
                        UsuarioId = usuario.UsuarioId,
                        Token = resetToken,
                        FechaCreacion = DateTime.Now,
                        FechaExpiracion = DateTime.Now.AddHours(1), // Token válido por 1 hora
                        Usado = false
                    };

                    _context.PasswordResetTokens.Add(passwordResetToken);
                    await _context.SaveChangesAsync();

                    // Generar enlace de restablecimiento
                    var resetLink = Url.Action("ResetPassword", "Auth",
                        new { token = resetToken }, Request.Scheme);

                    try
                    {
                        // Enviar correo
                        await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink);

                        TempData["SuccessMessage"] = "Se ha enviado un correo con las instrucciones para restablecer tu contraseña. Revisa tu bandeja de entrada.";
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Hubo un error al enviar el correo. Por favor, inténtalo de nuevo más tarde.";
                        // Log del error (opcional)
                        // _logger.LogError(ex, "Error al enviar correo de restablecimiento");
                    }
                }
                else
                {
                    // Por seguridad, no revelar si el correo existe o no
                    TempData["SuccessMessage"] = "Si el correo electrónico está registrado, recibirás las instrucciones para restablecer tu contraseña.";
                }
            }

            return View(model);
        }

        // GET: Auth/ResetPassword
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var passwordResetToken = await _context.PasswordResetTokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (passwordResetToken == null || !passwordResetToken.IsValid())
            {
                ViewBag.IsExpiredToken = true;
                return View();
            }

            var model = new ResetPasswordViewModel
            {
                Token = token
            };

            return View(model);
        }

        // POST: Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var passwordResetToken = await _context.PasswordResetTokens
                    .Include(t => t.Usuario)
                    .FirstOrDefaultAsync(t => t.Token == model.Token);

                if (passwordResetToken == null || !passwordResetToken.IsValid())
                {
                    ViewBag.IsExpiredToken = true;
                    return View(model);
                }

                // Actualizar la contraseña del usuario
                passwordResetToken.Usuario.PasswordHash = HashPassword(model.NewPassword);

                // Marcar el token como usado
                passwordResetToken.Usado = true;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tu contraseña ha sido restablecida exitosamente. Ya puedes iniciar sesión con tu nueva contraseña.";
                return RedirectToAction("Login");
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

        // Método para generar token de restablecimiento
        private string GenerateResetToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }

        public IActionResult AccessDenied()
        {
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }
    }
}
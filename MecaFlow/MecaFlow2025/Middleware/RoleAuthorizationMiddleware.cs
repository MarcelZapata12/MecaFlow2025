using Microsoft.AspNetCore.Authorization;

namespace MecaFlow2025.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleAuthorizationMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // 0) Respetar [AllowAnonymous]
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var isApi = path.StartsWith("/api/");
            var userId = context.Session.GetString("UserId");
            var userRole = context.Session.GetString("UserRole");

            // 1) Rutas públicas y estáticos
            if (path == "/"
                || path.StartsWith("/auth")
                || path.StartsWith("/home")
                || path.StartsWith("/acercanosotros")
                || path.StartsWith("/css/")
                || path.StartsWith("/js/")
                || path.StartsWith("/lib/")
                || path.StartsWith("/images/")
                || path.StartsWith("/favicon")
                || path == "/robots.txt"
                || path == "/sitemap.xml")
            {
                await _next(context);
                return;
            }

            // --- ALLOWLIST: Chat para cualquier usuario autenticado (incluye /api/chat y /api/chat/reset) ---
            if (path.StartsWith("/api/chat"))
            {
                if (string.IsNullOrEmpty(userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "No autenticado" });
                    return;
                }
                await _next(context);
                return;
            }
            // -------------------------------------------------------------------------------------------------

            // 2) Sin sesión
            if (string.IsNullOrEmpty(userId))
            {
                if (isApi)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "No autenticado" });
                }
                else
                {
                    context.Response.Redirect("/Auth/Login");
                }
                return;
            }

            // 3) Chequeo de permisos por rol (no-API o APIs distintas a /api/chat)
            if (!HasPermission(path, userRole))
            {
                if (isApi)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { error = "Acceso denegado" });
                }
                else
                {
                    context.Response.Redirect("/Auth/AccessDenied");
                }
                return;
            }

            await _next(context);
        }

        private bool HasPermission(string? path, string? userRole)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(userRole))
                return false;

            // Admin: acceso total
            if (userRole == "Administrador") return true;

            // Empleado
            if (userRole == "Empleado")
            {
                return path.Contains("/asistencias")
                    || path.Contains("/diagnosticos")
                    || path.Contains("/vehiculos")
                    || path.Contains("/pagos")
                    || path.Contains("/facturas")
                    || path.Contains("/clientes") 
                    || path.Contains("/tareas")    
                    || path.Contains("/home");
            }

            // Cliente
            if (userRole == "Cliente")
            {
                return path.Contains("/diagnosticos")
                    || path.Contains("/vehiculos")
                    || path.Contains("/facturas")
                    || path.Contains("/home");
            }

            return false;
        }
    }
}

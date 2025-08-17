using Microsoft.AspNetCore.Authorization;

namespace MecaFlow2025.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

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
            var userId = context.Session.GetString("UserId");
            var userRole = context.Session.GetString("UserRole");

            // 1) Rutas públicas y estáticos (SIN /home)
            if (path == "/" ||
                path.StartsWith("/auth") ||
                path.StartsWith("/acercanosotros") ||
                path.StartsWith("/css/") ||
                path.StartsWith("/js/") ||
                path.StartsWith("/lib/") ||
                path.StartsWith("/images/") ||
                path.StartsWith("/favicon") ||
                path == "/robots.txt" ||
                path == "/sitemap.xml")
            {
                await _next(context);
                return;
            }

            // 2) Si no hay sesión, enviar a Login
            if (string.IsNullOrEmpty(userId))
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            // 3) Chequeo de permisos por rol
            if (!HasPermission(path, userRole))
            {
                context.Response.Redirect("/Auth/AccessDenied");
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
                return path.Contains("/asistencias") ||
                       path.Contains("/diagnosticos") ||
                       path.Contains("/vehiculos") ||
                       path.Contains("/pagos");
            }

            // Cliente
            if (userRole == "Cliente")
            {
                return path.Contains("/diagnosticos") ||
                       path.Contains("/vehiculos");
            }

            return false;
        }
    }
}

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
            var path = context.Request.Path.Value?.ToLower();
            var userRole = context.Session.GetString("UserRole");
            var userId = context.Session.GetString("UserId");

            // Permitir acceso a Auth y Home sin verificación
            if (path?.StartsWith("/auth") == true || path?.StartsWith("/home") == true || path == "/")
            {
                await _next(context);
                return;
            }

            // Si no está autenticado, redirigir al login
            if (string.IsNullOrEmpty(userId))
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            // Verificar permisos según el rol
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

            // Administradores tienen acceso a todo
            if (userRole == "Administrador")
                return true;

            // Empleados: Asistencias, Diagnosticos, Vehiculos, Pagos
            if (userRole == "Empleado")
            {
                return path.Contains("/asistencias") ||
                       path.Contains("/diagnosticos") ||
                       path.Contains("/vehiculos") ||
                       path.Contains("/pagos");
            }

            // Clientes: solo Diagnosticos y Vehiculos
            if (userRole == "Cliente")
            {
                return path.Contains("/diagnosticos") ||
                       path.Contains("/vehiculos");
            }

            return false;
        }
    }
}

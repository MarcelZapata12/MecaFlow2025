using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MecaFlow2025.Filters
{
    public class RoleAuthorizationFilter : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizationFilter(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            var userId = context.HttpContext.Session.GetString("UserId");

            // Verificar si el usuario está autenticado
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Verificar si el rol del usuario está permitido
            if (!_allowedRoles.Contains(userRole))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

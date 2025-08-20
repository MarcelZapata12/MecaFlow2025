using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MecaFlow2025.Helpers;

namespace MecaFlow2025.Attributes
{
    public class AuthorizeActionAttribute : ActionFilterAttribute
    {
        private readonly string _module;
        private readonly string _action;

        public AuthorizeActionAttribute(string module, string action)
        {
            _module = module;
            _action = action;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetString("UserId");

            // Verificar si el usuario está autenticado
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Verificar si tiene permisos para esta acción específica
            if (!RoleHelper.CanPerformAction(context.HttpContext, _module, _action))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}


using Microsoft.AspNetCore.Mvc;

namespace MecaFlow2025.Attributes
{
    public class AuthorizeRoleAttribute : TypeFilterAttribute
    {
        public AuthorizeRoleAttribute(params string[] roles) : base(typeof(Filters.RoleAuthorizationFilter))
        {
            Arguments = new object[] { roles };
        }
    }
}
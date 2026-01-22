using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERPApi.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission)
        {
            _permission = permission;
            // Just use basic authentication requirement
            Roles = "Admin"; // Admin can access everything
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // First check if authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // If user is admin, allow access
            //if (user.IsInRole("Admin"))
            //{
            //    return;
            //}

            // For non-admin users, check specific permission
            var hasPermission = user.Claims.Any(c =>
                c.Type == "permissions" &&
                c.Value.Equals(_permission, StringComparison.OrdinalIgnoreCase));

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
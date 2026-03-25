using System.Security.Claims;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManageAccountWebAPI.Infrastructure.Filters
{
    public class FunctionAuthorizationFilter : IAuthorizationFilter
    {
        private readonly string _permissionCode;
        private readonly IAuthService _authService;

        public FunctionAuthorizationFilter(string permissionCode, IAuthService authService)
        {
            _permissionCode = permissionCode;
            _authService = authService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            bool hasPermission = _authService.UserHasPermission(userId, _permissionCode);
            if (!hasPermission)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}
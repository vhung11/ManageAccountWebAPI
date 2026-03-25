using Microsoft.AspNetCore.Mvc;

namespace ManageAccountWebAPI.Controllers.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizeFunctionAttribute : TypeFilterAttribute
    {
        public AuthorizeFunctionAttribute(string permissionCode) : base(typeof(FunctionAuthorizationFilter))
        {
            Arguments = new object[] { permissionCode };
        }
    }
}
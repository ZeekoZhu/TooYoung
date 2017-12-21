using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TooYoung.Web.Jwt;
using TooYoung.Web.Models;

namespace TooYoung.Web.Filters
{
    /// <summary>
    /// 权限校验，满足参数中的任意一个权限即可
    /// </summary>
    public class RequiredPermissions : TypeFilterAttribute
    {
        public RequiredPermissions(params Permission[] permissions) : base(typeof(PermissionFilterAttribute))
        {
            Arguments = new[] { new PermissionRequirement(permissions) };
        }
    }
    public class PermissionFilterAttribute : IAuthorizationFilter
    {
        private readonly PermissionRequirement _requirement;

        public PermissionFilterAttribute(PermissionRequirement req)
        {
            _requirement = req;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var permissionClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "per")?.Value ?? "";
            var permissions = permissionClaim
                .Split(',')
                .Select(p =>
                {
                    if (int.TryParse(p, out int permission))
                    {
                        return new Nullable<int>(permission);
                    }
                    return null;
                })
                .Where(p => p != null)
                .Select(p => (Permission)p);

            if (permissions.Any(p => _requirement.Permissions.Contains(p)))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}

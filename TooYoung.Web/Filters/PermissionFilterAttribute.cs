using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TooYoung.Core.Models;
using TooYoung.Web.Jwt;

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
                        return new int?(permission);
                    }
                    return null;
                })
                .Where(p => p != null && Enum.IsDefined(typeof(Permission), p.Value))
                .Select(p => (Permission)p);
            if (permissions.Any(p => _requirement.Permissions.Contains(p)) == false)
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new JsonResult(new { Required = _requirement });
            }
        }
    }
}

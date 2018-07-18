using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TooYoung.Core.Models;
using TooYoung.Core.Permissions;
using TooYoung.Web.Jwt;

namespace TooYoung.Web.Filters
{
    /// <summary>
    /// 权限校验，满足参数中的任意一个权限即可
    /// </summary>
    public class RequiredPermissions : TypeFilterAttribute
    {
        public RequiredPermissions(string permissionStr) : base(typeof(PermissionFilterAttribute))
        {
            var permission = new Permission(permissionStr);
            Arguments = new object[] { new PermissionRequirement(permission) };
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
            try
            {
                var userPermission = new Permission(permissionClaim);
                if (userPermission.Contains(_requirement.Permissions) == false)
                {
                    AuthFailed(context);
                }
            }
            catch (Exception)
            {
                AuthFailed(context);
            }
            
        }

        private void AuthFailed(AuthorizationFilterContext context)
        {
            context.HttpContext.Response.StatusCode = 403;
            context.Result = new JsonResult(new { Required = _requirement.Permissions });
        }
    }
}

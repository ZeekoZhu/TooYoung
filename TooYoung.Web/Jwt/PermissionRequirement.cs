using Microsoft.AspNetCore.Authorization;
using TooYoung.Core.Models;

namespace TooYoung.Web.Jwt
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public Permission[] Permissions { get; set; }
        public PermissionRequirement(Permission[] permissions)
        {
            this.Permissions = permissions;
        }
    }
}

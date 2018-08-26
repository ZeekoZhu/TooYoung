using Microsoft.AspNetCore.Authorization;
using TooYoung.Core.Models;
using TooYoung.Core.Permissions;

namespace TooYoung.Web.Jwt
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public Permission Permissions { get; set; }
        public PermissionRequirement(Permission permissions)
        {
            Permissions = permissions;
        }
    }
}

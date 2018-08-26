using System.Linq;
using System.Security.Claims;

namespace TooYoung.Web.Utils
{
    public static class IdentityExt
    {
        public static string Id(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
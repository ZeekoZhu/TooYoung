using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace TooYoung.Web.Filters
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        public JwtAuthorizeAttribute()
        {
            AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," +
                                    JwtBearerDefaults.AuthenticationScheme;
        }
    }
}

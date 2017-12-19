using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AgileObjects.AgileMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TooYoung.Filters;
using TooYoung.Models;
using TooYoung.Services;
using TooYoung.Web.ViewModels;
using ZeekoUtilsPack.AspNetCore.Jwt;

namespace TooYoung.Controllers
{
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ValidateModel]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly JwtOptions _jwtOptions;
        public AccountController(AccountService accountService, JwtOptions jwtOptions)
        {
            _jwtOptions = jwtOptions;
            _accountService = accountService;
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await _accountService.Create(Mapper.Map(model).ToANew<User>());
            return Json(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _accountService.FindByUserName(model.UserName);
            if (user != null && user.Password == model.Password)
            {
                var token = CreateToken(model.UserName, DateTime.Now.AddDays(7), "tooyoung");
                return Json(new { token });
            }
            return Unauthorized();
        }

        /// <summary>
        /// 生成一个新的 Token
        /// </summary>
        /// <param name="user">用户信息实体</param>
        /// <param name="expire">token 过期时间</param>
        /// <param name="audience">Token 接收者</param>
        /// <returns></returns>
        private string CreateToken(string user, DateTime expire, string audience)
        {
            var handler = new JwtSecurityTokenHandler();
            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(user, "TokenAuth"));
            var token = handler.CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Issuer = _jwtOptions.Issuer,
                Audience = audience,
                SigningCredentials = _jwtOptions.Credentials,
                Subject = identity,
                Expires = expire
            });
            return token;
        }
    }
}

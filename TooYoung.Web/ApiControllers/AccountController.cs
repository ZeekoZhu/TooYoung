using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgileObjects.AgileMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using TooYoung.Core.Models;
using TooYoung.Core.Repository;
using TooYoung.Web.Filters;
using TooYoung.Web.ViewModels;
using ZeekoUtilsPack.AspNetCore.Jwt;
using ZeekoUtilsPack.BCLExt;

namespace TooYoung.Web.ApiControllers
{
    /// <summary>
    /// 账户相关 API
    /// </summary>
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ValidateModel]
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly EasyJwt _jwt;

        public AccountController(IAccountRepository accountRepository, EasyJwt jwt)
        {
            _accountRepository = accountRepository;
            _jwt = jwt;
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            model.Password = (model.Password + model.UserName).GetMd5();
            var result = await _accountRepository.Create(Mapper.Map(model).ToANew<User>());
            return Json(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _accountRepository.FindByUserName(model.UserName);
            if (user != null && user.Password == (model.Password + model.UserName).GetMd5())
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id, ClaimValueTypes.String),
                    // TODO: new Claim("jti", jti, ClaimValueTypes.String),
                    new Claim("per",string.Join(",", user.Permissions.Select(p=>(int)p)), ClaimValueTypes.String)
                };
                var token = _jwt.GenerateToken(user.UserName, claims, DateTime.Now.AddDays(7));
                var (principal, authProps) = _jwt.GenerateAuthTicket(user.UserName, claims, DateTime.Now.AddDays(7));
                await HttpContext.SignInAsync(principal, authProps);
                return Json(new { token });
            }
            return Unauthorized();
        }
    }
}

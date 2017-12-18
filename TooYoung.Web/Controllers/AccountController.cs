using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AgileObjects.AgileMapper;
using Microsoft.AspNetCore.Mvc;
using TooYoung.Models;
using TooYoung.Services;

namespace TooYoung.Controllers
{
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        public AccountService AccountService { get; set; }
        
        public AccountController(AccountService accountService)
        {
            AccountService = accountService;
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await AccountService.Create(Mapper.Map(model).ToANew<User>());
            return Json(result);
        }
        
        public class RegisterModel
        {
            [Required]
            public string UserName { get; set; }
            [Required]
            public string Password { get; set; }
            [Required]
            public string DisplayName { get; set; }
            [Required]
            public List<Permission> Permissions { get; set; }
        }
    }
}
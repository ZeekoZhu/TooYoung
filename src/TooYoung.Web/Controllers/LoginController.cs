using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TooYoung.Provider.MongoDB.Repositories;
using TooYoung.Web.Filters;
using TooYoung.Web.ViewModels;

namespace TooYoung.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger _logger;
        private readonly AccountRepository _accountRepository;

        public LoginController(ILogger<LoginController> logger, AccountRepository accountRepository)
        {
            _logger = logger;
            _accountRepository = accountRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData.TryGetValue("Error", out object value))
            {
                ViewBag.Error = value;
            }
            return View();
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Index([FromForm] LoginModel model)
        {
            var user = await _accountRepository.FindByUserName(model.UserName);
            if (user != null && user.Password == model.Password)
            {
                return Redirect("/");
            }
            Response.StatusCode = 401;
            TempData["Error"] = "用户名或密码错误";
            return RedirectToAction("Index");
        }
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TooYoung.Filters;
using TooYoung.Web.ViewModels;
using ZeekoUtilsPack.BCLExt;

namespace TooYoung.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
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
        public IActionResult Index([FromForm] LoginModel model)
        {
            if (model.UserName == "12345" && model.Password == "123".GetMd5())
            {
                return Redirect("/");
            }
            Response.StatusCode = 401;
            TempData["Error"] = "用户名或密码错误";
            return RedirectToAction("Index");
        }
    }
}

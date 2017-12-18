using Microsoft.AspNetCore.Mvc;

namespace TooYoung.Controllers
{
    public class LoginController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return
            View();
        }
    }
}
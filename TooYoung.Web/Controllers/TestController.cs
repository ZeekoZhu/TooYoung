using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.NodeServices;

namespace TooYoung.Web.Controllers
{
    public class TestController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromServices] INodeServices nodeServices)
        {
            var result = await nodeServices.InvokeAsync<int>("./addNumbers", 1, 2);
            return Content("1 + 2 = " + result);
        }
    }
}

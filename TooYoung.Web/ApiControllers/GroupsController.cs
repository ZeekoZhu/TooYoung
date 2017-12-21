using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TooYoung.Filters;
using TooYoung.Web.Filters;
using TooYoung.Web.Models;
using TooYoung.Web.Services;

namespace TooYoung.Web.ApiControllers
{
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ValidateModel]
    [JwtAuthorize]
    public class GroupsController : Controller
    {
        private readonly ImageManageService _imageManageService;
        public GroupsController(ImageManageService imageManageService)
        {
            this._imageManageService = imageManageService;
        }

        [HttpPost]
        [RequiredPermissions(Permission.ManageGroup, Permission.AdminAll)]
        public IActionResult Post()
        {
            return Ok("Success");
        }
    }
}

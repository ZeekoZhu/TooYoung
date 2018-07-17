using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileObjects.AgileMapper;
using Microsoft.AspNetCore.Mvc;
using TooYoung.Core.Models;
using TooYoung.Core.Repository;
using TooYoung.Web.Filters;
using TooYoung.Web.Utils;
using TooYoung.Web.ViewModels;
using ZeekoUtilsPack.AspNetCore.Jwt;

namespace TooYoung.Web.ApiControllers
{
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [EasyJwtAuthorize]
    public class GroupsController : Controller
    {
        private readonly IImageRepository _imageRepository;
        public GroupsController(IImageRepository imageRepository)
        {
            this._imageRepository = imageRepository;
        }

        [HttpPost]
        [RequiredPermissions(Permission.ManageGroup, Permission.AdminAll)]
        public async Task<IActionResult> Post([FromBody] GroupPostModel model)
        {
            do
            {
                if (await _imageRepository.HasGroupNameAsync(model.Name, User.Id()))
                {
                    ModelState.AddModelError(nameof(model.Name), "输入的组名已经存在，请输入一个新的组名");
                    break;
                }
            } while (false);
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }
            var group = Mapper.Map(model).ToANew<Group>();
            if (group.ACL == null || (group.ACL.Any() == false))
            {
                group.ACL = new List<string> { "*" };
            }
            group.OwnerId = User.Id();
            var result = await _imageRepository.AddNewGroup(group);
            return Json(result);
        }

        [HttpGet]
        [RequiredPermissions(Permission.ManageGroup, Permission.AdminAll)]
        public async Task<IActionResult> Get()
        {
            var userId = User.Id();
            var result = await _imageRepository.GetGroupsAsync(userId);
            return Json(result);
        }
    }
}

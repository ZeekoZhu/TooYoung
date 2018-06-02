using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using TooYoung.Web.Filters;
using TooYoung.Web.Models;
using TooYoung.Web.Services;
using TooYoung.Web.ViewModels;

namespace TooYoung.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorize]
    public class ImagesController : Controller
    {
        private readonly ImageManageService _imgService;

        public ImagesController(ImageManageService imgService)
        {
            _imgService = imgService;
        }

        /// <summary>
        /// 上传图片第一步，创建图片信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [RequiredPermissions(Permission.ManageImage, Permission.AdminAll)]
        public async Task<ActionResult<ImageInfo>> Post([FromBody] ImagePostModel model)
        {
            var result = await _imgService.SaveImageInfo(model.Name, model.GroupId);
            if (result == null)
            {
                return NotFound();
            }
            return result;
        }

        [RequiredPermissions(Permission.ManageImage, Permission.AdminAll)]
        [HttpPut("{infoId}")]
        public async Task<ActionResult<ImageInfo>> Put([FromRoute]string infoId)
        {
            var imageFile = Request.Form?.Files?.GetFile("image");
            if (imageFile == null) return BadRequest();
            using (var ms = new MemoryStream())
            {
                await imageFile.CopyToAsync(ms);
                var result = await _imgService.UpdateImage(ms, infoId);
                if (result == null)
                {
                    return NotFound();
                }
                return result;
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Get([FromRoute]string name)
        {
            // get imageinfo
            var info = await _imgService.GetImageInfoByName(name);
            if (info == null)
            {
                return NotFound();
            }
            // check group ACL
            var group = await _imgService.GetGroupByImageInfo(info.Id);
            var hasReferer = Request.Headers.TryGetValue("Referer", out var refererValue);
            var referer = hasReferer ? refererValue.ToString() : "";
            var accessible = group.IsAccessible(referer);
            if (accessible == false) return Forbid(JwtBearerDefaults.AuthenticationScheme);
            // get image binary
            var img = await _imgService.GetImageByImageInfo(info.Id);
            return File(img.Binary, info.GetMime(), name);
        }
    }
}

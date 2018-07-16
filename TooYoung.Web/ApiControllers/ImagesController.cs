using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TooYoung.Core.Exceptions;
using TooYoung.Core.Models;
using TooYoung.Core.Repository;
using TooYoung.Core.Services;
using TooYoung.Web.Filters;
using TooYoung.Web.Utils;
using TooYoung.Web.ViewModels;

namespace TooYoung.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorize]
    public class ImagesController : Controller
    {
        private readonly ImageManageService _imgService;
        private readonly IImageRepository _imgRepo;

        public ImagesController(ImageManageService imgService, IImageRepository imgRepo)
        {
            _imgService = imgService;
            _imgRepo = imgRepo;
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
            var result = await _imgService.SaveImageInfo(model.Name, model.GroupId, User.Id());
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
                return result;
            }
        }

        ///// <summary>
        ///// 下载图片
        ///// </summary>
        ///// <param name="user">用户名</param>
        ///// <param name="imgName">图片名</param>
        ///// <returns></returns>
        //[AllowAnonymous]
        //[HttpGet("{user}/{imgName}")]
        //public async Task<IActionResult> Get([FromRoute]string user, [FromRoute]string imgName)
        //{
        //    // get imageinfo
        //    try
        //    {
        //        var info = await _imgRepo.GetImageInfoByName(user, imgName);
        //        // check group ACL
        //        var group = await _imgRepo.GetGroupByImageInfo(info.Id);
        //        var hasReferer = Request.Headers.TryGetValue("Referer", out var refererValue);
        //        var referer = hasReferer ? refererValue.ToString() : "";
        //        var accessible = group.IsAccessible(referer);
        //        if (accessible == false) return Forbid(JwtBearerDefaults.AuthenticationScheme);
        //        // get image binary
        //        var img = await _imgRepo.GetImageByImageInfo(info.Id);
        //        return File(img.Binary, info.GetMime());
        //    }
        //    catch (BlogAppException e)
        //    {
        //        return NotFound(new ErrorMsg(e.Message));
        //    }
        //}

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="user">用户名或用户 Id</param>
        /// <param name="groupName">分组名</param>
        /// <param name="imgName">图片名</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{user}/{groupName}/{imgName}")]
        public async Task<IActionResult> Get([FromRoute] string user, [FromRoute] string groupName, [FromRoute] string imgName)
        {
            try
            {
                var hasReferer = Request.Headers.TryGetValue("Referer", out var refererValue);
                var referer = hasReferer ? refererValue.ToString() : "";
                var (image, info) = await _imgService.GetImage(user, groupName, imgName, referer);
                return File(image.Binary, info.GetMime());
            }
            catch (AppException e)
            {
                Response.StatusCode = e.Code;
                return Json(new ErrorMsg(e.Message));
            }
        }
    }
}

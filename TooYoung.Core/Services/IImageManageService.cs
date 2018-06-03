using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TooYoung.Core.Models;

namespace TooYoung.Core.Services
{
    public interface IImageManageService
    {
        /// <summary>
        /// 添加新的分组
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        Task<Group> AddNewGroup(Group group);

        /// <summary>
        /// 判断分组是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> HasGroupName(string name, string userId);

        /// <summary>
        /// 获取指定用户的全部分组
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Group>> GetGroups(string userId);

        /// <summary>
        /// 创建一个空的图片信息，创建后该图片仍然无法获取，需要使用 <see cref="ImageManageService.UpdateImage"></see> 存储图片内容。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        Task<ImageInfo> SaveImageInfo(string name, string groupId);

        Task<Result<ImageInfo>> UpdateImage(MemoryStream bin, string infoId);
        Task<ImageInfo> GetImageInfo(string infoId);

        Task<Result<ImageInfo>> GetImageInfoByName(string userName, string name);
        Task<Group> GetGroupByImageInfo(string infoId);
        Task<Image> GetImageByImageInfo(string infoId);
    }
}
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TooYoung.Core.Models;

namespace TooYoung.Core.Repository
{
    public interface IImageRepository
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
        Task<bool> HasGroupNameAsync(string name, string userId);

        /// <summary>
        /// 根据分组名称获取分组信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Group> GetGroupByName(string name, string userId);

        /// <summary>
        /// 获取指定用户的全部分组
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Group>> GetGroupsAsync(string userId);

        /// <summary>
        /// 创建一个空的图片信息，创建后该图片仍然无法获取，需要使用 <see cref="UpdateImage"></see> 存储图片内容。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        Task<ImageInfo> SaveImageInfo(string name, string groupId);

        /// <summary>
        /// 更新图片内容
        /// </summary>
        /// <param name="bin">图片内容流</param>
        /// <param name="infoId">图片信息 Id</param>
        /// <returns></returns>
        Task<ImageInfo> UpdateImage(MemoryStream bin, string infoId);
        Task<ImageInfo> GetImageInfo(string infoId);

        /// <summary>
        /// 检查指定图片是否存在
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        Task<bool> HasImageInGroup(string groupId, string imageName);

        /// <summary>
        /// 查找指定用户的指定图片的信息
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="imageName">图片名称</param>
        /// <returns></returns>
        Task<ImageInfo> GetImageInfoByName(string userName, string imageName);
        Task<Group> GetGroupByImageInfo(string infoId);
        Task<Image> GetImageByImageInfo(string infoId);
    }
}
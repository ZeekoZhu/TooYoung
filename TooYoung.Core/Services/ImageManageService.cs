using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TooYoung.Core.Exceptions;
using TooYoung.Core.Models;
using TooYoung.Core.Repository;

namespace TooYoung.Core.Services
{
    public class ImageManageService
    {
        private readonly IImageRepository _imgRepo;

        public ImageManageService(IImageRepository imgRepo)
        {
            _imgRepo = imgRepo;
        }


        public async Task<(Image, ImageInfo)> GetImage(string user, string groupName, string imageName, string referer)
        {
            // find group
            var group = await _imgRepo.GetGroupByName(groupName, user);
            if (group == null)
            {
                throw new AppException($"Group {groupName} not found", 404);
            }
            var isAllowed = group.IsAccessible(referer);
            if (isAllowed == false)
            {
                throw new AppException("Access denied", 403);
            }
            // find image info
            var imageInfo = await _imgRepo.GetImageInfoByName(user, groupName, imageName);
            // find image
            return (await _imgRepo.GetImageByImageInfo(imageInfo.Id), imageInfo);
        }

        /// <summary>
        /// 判断分组是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<bool> HasGroupName(string name, string userId)
        {
            return _imgRepo.HasGroupNameAsync(name, userId);
        }

        /// <summary>
        /// 获取指定用户的全部分组
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Group>> GetGroups(string userId)
        {
            return _imgRepo.GetGroupsAsync(userId);
        }

        /// <summary>
        /// 创建一个空的图片信息，创建后该图片仍然无法获取，需要使用 <see cref="UpdateImage"></see> 存储图片内容。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ImageInfo> SaveImageInfo(string name, string groupName, string userId)
        {
            // group should exits
            var group = await _imgRepo.GetGroupByName(groupName, userId);
            if (group == null)
            {
                throw new AppException($"{groupName} not found");
            }

            // find images with same imageName in same group
            var imgExists = await _imgRepo.HasImageInGroup(groupName, name);

            if (imgExists)
            {
                throw new AppException($"{groupName} already contains {name}");
            }

            return await _imgRepo.SaveImageInfo(name, group.Id);

        }

        /// <summary>
        /// 更新一张图片的内容
        /// </summary>
        /// <param name="bin">图片内容</param>
        /// <param name="infoId">图片信息 Id</param>
        /// <returns></returns>
        public async Task<ImageInfo> UpdateImage(MemoryStream bin, string infoId)
        {
            return await _imgRepo.UpdateImage(bin, infoId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        /// <summary>
        /// 添加新的分组
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public Task<Group> AddNewGroup(Group group)
        {
            return _imgRepo.AddNewGroup(group);
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
                throw new BlogAppException($"{groupName} not found");
            }

            // find images with same imageName in same group
            var imgExists = await _imgRepo.HasImageInGroup(groupName, name);

            if (imgExists)
            {
                throw new BlogAppException($"{groupName} already contains {name}");
            }

            return await _imgRepo.SaveImageInfo(name, group.Id);

        }
    }
}

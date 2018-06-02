using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SixLabors.ImageSharp;
using TooYoung.Web.Models;
using TooYoung.Web.Utils;
using Image = TooYoung.Web.Models.Image;

namespace TooYoung.Web.Services
{
    public class ImageManageService
    {

        public IMongoCollection<User> Users { get; set; }
        public IMongoCollection<Image> Images { get; set; }
        public IMongoCollection<ImageInfo> ImageInfos { get; set; }

        public ImageManageService(IMongoDatabase db)
        {
            Users = db.GetCollection<User>(nameof(User));
            Images = db.GetCollection<Image>(nameof(Image));
            ImageInfos = db.GetCollection<ImageInfo>(nameof(ImageInfo));
        }

        /// <summary>
        /// 添加新的分组
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task<Group> AddNewGroup(Group group)
        {
            group.Id = ObjectId.GenerateNewId().ToString();
            var update = Builders<User>.Update.Push(u => u.Groups, group);
            var filter = Builders<User>.Filter.Eq(u => u.Id, group.OwnerId);
            var result = await Users.FindOneAndUpdateAsync(filter, update);
            return group;
        }

        /// <summary>
        /// 判断分组是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> HasGroupName(string name, string userId)
        {
            var groupQuery = Builders<Group>.Filter.Eq(g => g.Name, name);
            var userQuery = Builders<User>.Filter.ElemMatch(u => u.Groups, groupQuery);
            var matchIdQuery = Builders<User>.Filter.Eq(u => u.Id, userId);
            var result = await Users.Find(Builders<User>.Filter.And(matchIdQuery, userQuery)).CountAsync();
            return result > 0;
        }

        /// <summary>
        /// 获取指定用户的全部分组
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<Group>> GetGroups(string userId)
        {
            var user = await Users.AsQueryable().FirstAsync(u => u.Id == userId);
            return user.Groups;
        }

        /// <summary>
        /// 创建一个空的图片信息，创建后该图片仍然无法获取，需要使用 <see cref="UpdateImage"></see> 存储图片内容。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<ImageInfo> SaveImageInfo(string name, string groupId)
        {
            // save imageinfo
            var info = new ImageInfo
            {
                Name = name,
                GroupId = groupId
            };
            await ImageInfos.InsertOneAsync(info);

            // Add image info to group
            var groupIdFilter = Builders<Group>.Filter.Eq(g => g.Id, groupId);
            var userFilter = Builders<User>.Filter.ElemMatch(u => u.Groups, groupIdFilter);
            var update = Builders<User>.Update.AddToSet(u => u.Groups[Constant.FirstMatch].ImageInfos, info.Id);
            await Users.UpdateOneAsync(userFilter, update);

            // return info
            return info;
        }

        public async Task<ImageInfo> UpdateImage(MemoryStream bin, string infoId)
        {
            // find imageinfo
            var targetInfo = await ImageInfos.AsQueryable().FirstOrDefaultAsync(i => i.Id == infoId);
            if (targetInfo == null)
            {
                return null;
            }
            // Save image binary
            var bytes = bin.ToArray();
            var img = new Image
            {
                Binary = bytes
            };
            await Images.InsertOneAsync(img);
            bin.Seek(0, SeekOrigin.Begin);
            // update imageinfo
            IImageInfo imageInfo = SixLabors.ImageSharp.Image.Identify(bin);
            targetInfo.Image = img.Id;
            targetInfo.SizeOfBytes = bytes.Length;
            targetInfo.Width = imageInfo.Width;
            targetInfo.Height = imageInfo.Height;
            var result = await ImageInfos.ReplaceOneAsync(i => i.Id == infoId, targetInfo);
            if (result.ModifiedCount > 0)
            {
                return targetInfo;
            }
            return null;
        }

        public async Task<ImageInfo> GetImageInfo(string infoId)
        {
            var result = await ImageInfos.AsQueryable().FirstOrDefaultAsync(i => i.Id == infoId);
            return result;
        }

        public async Task<ImageInfo> GetImageInfoByName(string name)
        {
            var result = await ImageInfos.AsQueryable().FirstOrDefaultAsync(i => i.Name == name);
            return result;
        }

        public async Task<Group> GetGroupByImageInfo(string infoId)
        {
            var result = await Users
                .AsQueryable()
                .SelectMany(u => u.Groups)
                .FirstOrDefaultAsync(g => g.ImageInfos.Contains(infoId));
            return result;
        }

        public async Task<Image> GetImageByImageInfo(string infoId)
        {
            var info = await ImageInfos.AsQueryable()
                .FirstOrDefaultAsync(i => i.Id == infoId);
            if (info == null) return null;
            var image = await Images.AsQueryable().FirstOrDefaultAsync(img => img.Id == info.Image);
            return image;
        }

    }
}

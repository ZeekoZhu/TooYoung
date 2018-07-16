using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TooYoung.Core.Exceptions;
using TooYoung.Core.Models;
using TooYoung.Core.Repository;
using TooYoung.Core.Services;
using Image = TooYoung.Core.Models.Image;

namespace TooYoung.Provider.MongoDB.Services
{
    /// <inheritdoc />
    public class ImageRepository : IImageRepository
    {
        private readonly IImageProcessService _imgProcessor;

        private IMongoCollection<User> Users { get; }
        private IMongoCollection<Image> Images { get; }
        private IMongoCollection<ImageInfo> ImageInfos { get; }

        public ImageRepository(IMongoDatabase db, IImageProcessService imgProcessor)
        {
            _imgProcessor = imgProcessor;
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
            await Users.FindOneAndUpdateAsync(filter, update);
            return group;
        }

        /// <summary>
        /// 判断分组是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> HasGroupNameAsync(string name, string userId)
        {
            var groupQuery = Builders<Group>.Filter.Eq(g => g.Name, name);
            var userQuery = Builders<User>.Filter.ElemMatch(u => u.Groups, groupQuery);
            var matchIdQuery = Builders<User>.Filter.Eq(u => u.Id, userId);
            var result = await Users.Find(Builders<User>.Filter.And(matchIdQuery, userQuery)).CountDocumentsAsync();
            return result > 0;
        }

        /// <summary>
        /// 根据分组名称获取分组信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Group> GetGroupByName(string name, string userId)
        {
            var groupQuery = Builders<Group>.Filter.Eq(g => g.Name, name);
            var userQuery = Builders<User>.Filter.ElemMatch(u => u.Groups, groupQuery);
            var matchIdQuery = Builders<User>.Filter.Eq(u => u.Id, userId);
            var result = await Users.Find(Builders<User>.Filter.And(matchIdQuery, userQuery))
                .Project(Builders<User>.Projection.ElemMatch(u => u.Groups, groupQuery)).As<Group>().FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 获取指定用户的全部分组
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<Group>> GetGroupsAsync(string userId)
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
            var info = await ImageInfos.AsQueryable().FirstOrDefaultAsync(i => i.Id == infoId);
            if (info == null)
            {
                throw new BlogAppException("Image not found");
            }

            // Save image binary
            var bytes = bin.ToArray();
            bin.Seek(0, SeekOrigin.Begin);
            var img = new Image
            {
                Binary = bytes
            };
            try
            {
                await Images.InsertOneAsync(img);
            }
            catch (Exception ex)
            {
                throw new BlogAppException("Can not save image", ex);
            }

            // update imageinfo
            var (width, height) = _imgProcessor.GetBound(bin);
            info.Image = img.Id;
            info.SizeOfBytes = img.Binary.Length;
            info.Width = width;
            info.Height = height;
            var updateInfoResult = await ImageInfos.ReplaceOneAsync(i => i.Id == infoId, info);
            if (updateInfoResult.ModifiedCount <= 0)
            {
                throw new BlogAppException("Can not update image information");
            }
            return info;
        }

        public async Task<ImageInfo> GetImageInfo(string infoId)
        {
            var result = await ImageInfos.AsQueryable().FirstOrDefaultAsync(i => i.Id == infoId);
            return result;
        }

        /// <summary>
        /// 检查指定分组中是否包含指定名字的图片，不检查分组是否存在
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public async Task<bool> HasImageInGroup(string groupId, string imageName)
        {
            var result = await ImageInfos.AsQueryable()
                .AnyAsync(info => info.GroupId == groupId && info.Name == imageName);

            return result;
        }

        public async Task<ImageInfo> GetImageInfoByName(string userName, string imageName)
        {
            var user = await Users.AsQueryable().FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                throw new BlogAppException($"{userName} not found");
            }

            var infoIds = user.Groups.SelectMany(g => g.ImageInfos);
            var infoIdFilter = Builders<ImageInfo>.Filter.In(i => i.Id, infoIds);
            var infoNameFilter = Builders<ImageInfo>.Filter.Eq(i => i.Name, imageName);
            var infoFilter = Builders<ImageInfo>.Filter.And(infoNameFilter, infoIdFilter);
            var result = await ImageInfos.Find(infoFilter).FirstOrDefaultAsync();
            if (result == null)
            {
                throw new BlogAppException($"{imageName} not found");
            }

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

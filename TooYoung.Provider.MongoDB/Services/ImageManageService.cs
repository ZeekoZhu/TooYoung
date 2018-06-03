using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TooYoung.Core.Helpers;
using TooYoung.Core.Models;
using TooYoung.Core.Services;
using Image = TooYoung.Core.Models.Image;

namespace TooYoung.Provider.MongoDB.Services
{
    public class ImageManageService : IImageManageService
    {
        private readonly IImageProcessService _imgProcessor;

        public IMongoCollection<User> Users { get; set; }
        public IMongoCollection<Image> Images { get; set; }
        public IMongoCollection<ImageInfo> ImageInfos { get; set; }

        public ImageManageService(IMongoDatabase db, IImageProcessService imgProcessor)
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

        public async Task<Result<ImageInfo>> UpdateImage(MemoryStream bin, string infoId)
        {
            // find imageinfo
            var saveImageResult = await Maybe<ImageInfo>.From(await ImageInfos
                    .AsQueryable()
                    .FirstOrDefaultAsync(i => i.Id == infoId))
                .ToResult("Image not found")
                // Save image binary
                .Flatten(async info =>
                {
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
                    catch
                    {
                        return Result.Fail<(Image, ImageInfo)>("Can not save image");
                    }

                    return Result.Ok((img, info));
                });
            var boundResult = saveImageResult
                .Flatten(_ => _imgProcessor.GetBound(bin));
            return await Result.Combine(saveImageResult, boundResult)
                // update imageinfo
                .Flatten(async () =>
                {
                    var (img, targetInfo) = saveImageResult.Value;
                    var (width, height) = boundResult.Value;
                    targetInfo.Image = img.Id;
                    targetInfo.SizeOfBytes = img.Binary.Length;
                    targetInfo.Width = width;
                    targetInfo.Height = height;
                    var result = await ImageInfos.ReplaceOneAsync(i => i.Id == infoId, targetInfo);
                    if (result.ModifiedCount > 0)
                    {
                        return Result.Ok(targetInfo);
                    }

                    return Result.Fail<ImageInfo>("Can not update image information");
                });


        }

        public async Task<ImageInfo> GetImageInfo(string infoId)
        {
            var result = await ImageInfos.AsQueryable().FirstOrDefaultAsync(i => i.Id == infoId);
            return result;
        }

        public async Task<Result<ImageInfo>> GetImageInfoByName(string userName, string name)
        {
            var user = await Users.AsQueryable().FirstOrDefaultAsync(u => u.UserName == userName);
            return await Maybe<User>.From(user).ToResult($"{userName} not found")
                .Map(u => u.Groups.SelectMany(g => g.ImageInfos).ToList())
                .Flatten(async infosRange =>
                {
                    var result = await ImageInfos.AsQueryable()
                        .FirstOrDefaultAsync(i => i.Name == name && infosRange.Contains(i.Id));
                    return Maybe<ImageInfo>.From(result).ToResult($"{name} not found");
                });
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

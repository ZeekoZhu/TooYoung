using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TooYoung.Web.Models;
using TooYoung.Web.Utils;

namespace TooYoung.Web.Services
{
    public class ImageManageService
    {

        public IMongoCollection<User> Users { get; set; }

        public ImageManageService(IMongoDatabase db)
        {
            Users = db.GetCollection<User>(nameof(User));
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
            var result = await Users.CountAsync(userQuery);
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
    }
}

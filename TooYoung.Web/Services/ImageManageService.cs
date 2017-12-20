using System.Threading.Tasks;
using MongoDB.Driver;
using TooYoung.Web.Models;

namespace TooYoung.Web.Services
{
    public class ImageManageService
    {

        public IMongoCollection<Group> Groups { get; set; }

        public ImageManageService(IMongoDatabase db)
        {
            Groups = db.GetCollection<Group>(nameof(Group));
        }
        public async Task<Group> AddNewGroup(Group group)
        {
            await Groups.InsertOneAsync(group);
            return group;
        }
    }
}

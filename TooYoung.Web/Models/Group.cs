using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace TooYoung.Web.Models
{
    public class Group
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> ACL { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public string OwnerId { get; set; }

        /// <summary>
        /// 该组别下包含的全部图片
        /// </summary>
        /// <returns></returns>
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> ImageInfos { get; set; } = new List<string>();
    }

    public static class GroupFunc
    {
        public static bool IsAccessible(this Group g, string referer)
        {
            return g.ACL.Any(rule =>
            {
                if (rule == "*") return true;
                var regex = new Regex(rule);
                return regex.IsMatch(referer);
            });
        }
    }
}

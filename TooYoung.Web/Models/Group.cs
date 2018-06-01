using System.Collections.Generic;
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
}

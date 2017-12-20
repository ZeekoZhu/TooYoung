using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace TooYoung.Web.Models
{
    public class Group
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> ACL { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string OwnerId { get; set; }

        /// <summary>
        /// 该组别下包含的全部图片
        /// </summary>
        /// <returns></returns>

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Images { get; set; }
    }
}

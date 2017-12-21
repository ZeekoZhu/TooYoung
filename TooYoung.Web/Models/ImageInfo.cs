using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace TooYoung.Web.Models
{
    public class ImageInfo
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int height { get; set; }

        /// <summary>
        /// 图片大小，单位为字节
        /// </summary>
        /// <returns></returns>
        public int SizeOfBytes { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Image { get; set; }

        // TODO: EXIF, Thumbnail
    }
}

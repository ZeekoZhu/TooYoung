using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using TooYoung.Core.Models;
using MongoDB.Bson;

namespace TooYoung.Provider.MongoDB
{
    public class MongoDBProvider
    {
        public static void Init()
        {
            BsonClassMap.RegisterClassMap<Group>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(g => g.Id).MapStringAsId();
                cm.MapMember(g => g.OwnerId).RepresentAsObjectId();
                cm.MapMember(g => g.ImageInfos)
                    .SetSerializer(new EnumerableInterfaceImplementerSerializer<List<string>>(new StringSerializer(BsonType.ObjectId)));
            });

            BsonClassMap.RegisterClassMap<Image>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(i => i.Id).MapStringAsId();

            });

            BsonClassMap.RegisterClassMap<ImageInfo>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(i => i.Id).MapStringAsId();
                cm.MapMember(i => i.GroupId).RepresentAsObjectId();
                cm.MapMember(i => i.Image).RepresentAsObjectId();
            });

            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(u => u.Id).MapStringAsId();
            });
        }
    }


}

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace TooYoung.Provider.MongoDB
{
    internal static class MongoExt
    {
        public static string RenderToJson<T>(this FilterDefinition<T> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            return filter.Render(documentSerializer, serializerRegistry).ToJson();
        }

        public static string RenderToJson<T>(this UpdateDefinition<T> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            return filter.Render(documentSerializer, serializerRegistry).ToJson();
        }

        public static BsonMemberMap RepresentAsObjectId(this BsonMemberMap map)
        {
            return map.SetSerializer(new StringSerializer(BsonType.ObjectId));
        }

        public static BsonMemberMap MapStringAsId(this BsonMemberMap map)
        {
            return map.SetIdGenerator(StringObjectIdGenerator.Instance)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        }
    }
}

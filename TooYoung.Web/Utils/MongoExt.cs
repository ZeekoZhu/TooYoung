using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TooYoung.Web.Utils
{
    public static class MongoExt
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
    }
}

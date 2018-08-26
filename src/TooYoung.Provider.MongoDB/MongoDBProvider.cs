using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using TooYoung.Core.Models;
using MongoDB.Bson;
using TooYoung.Core.Helpers;
using System.Linq;
using MongoDB.Driver;
using TooYoung.Core.Repository;
using TooYoung.Core.Services;
using TooYoung.Provider.MongoDB.Services;

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

    public static class MongoDBProviderExt
    {
        public static IServiceCollection AddYoungMongo(this IServiceCollection services)
        {
            MongoDBProvider.Init();

            services.AddSingleton<IMongoClient>(provider =>
            {
                var config = provider.GetService<IConfiguration>();
                var connectStr = config.GetConnectionString("Mongo");
                connectStr.ParseEnvVarParams()
                    .Select(p => (Key: $"$({p})", Val: config.GetSection(p).Value))
                    .ToList()
                    .ForEach(p => { connectStr = connectStr.Replace(p.Key, p.Val); });
                return new MongoClient(connectStr);
            });
            services.AddScoped(provider =>
            {
                var config = provider.GetService<IConfiguration>();
                var dbName = config.GetSection("MONGO_DBNAME").Value;
                return provider.GetService<IMongoClient>().GetDatabase(dbName);
            });
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            return services;
        }
    }


}

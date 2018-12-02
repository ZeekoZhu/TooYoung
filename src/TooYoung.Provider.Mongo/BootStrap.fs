module TooYoung.Provider.Mongo.BootStrap
open System
open AutoMapper
open AutoMapperBuilder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.IdGenerators
open MongoDB.Driver
open TooYoung.Domain.Authorization.UserGroup
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Repositories
open TooYoung.Domain.User
open TooYoung.Provider.Mongo.Repositories
open TooYoung.Provider.Mongo.Enities

[<CLIMutable>]
type MongoDbConfig =
    { Address: string
      DatabaseName: string
      UserName: string
      Password: string
      Port: int
    }


let bindConfig (config: IConfiguration) =
    let configSec = config.GetSection("MongoDb")
    { Address = configSec.GetSection("Address").Value
      DatabaseName = configSec.GetSection("DatabaseName").Value
      UserName = configSec.GetSection("UserName").Value
      Password = configSec.GetSection("Password").Value
      Port = configSec.GetSection("Port").Value |> int
    }

let addMongoDbRepository (configuration: IConfiguration) (services: IServiceCollection) =
    let config = bindConfig configuration
    BsonClassMap.RegisterClassMap<User>(
        fun cm -> cm.AutoMap()
                  cm.MapIdMember(fun u -> u.Id).SetIdGenerator(GuidGenerator.Instance) |> ignore
    ) |> ignore
    services.AddScoped<IFileRepository, FileRepository>()
            .AddScoped<ISharingRepository, SharingRepository>()
            .AddScoped<IDirectoryRepository, DirectoryRepository>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddScoped<IUserGroupRepository, UserGroupRepository>()
            .AddSingleton<IMongoDatabase>(
                fun provider ->
                    // using settings object to avoid url encoding
                    let credential = MongoCredential.CreateCredential(config.DatabaseName, config.UserName, config.Password)
                    let settings = MongoClientSettings(Credential = credential)
                    settings.Server <- MongoServerAddress(config.Address, config.Port)
                    let client = MongoClient(settings) :> IMongoClient
                    client.GetDatabase(config.DatabaseName)
            )
module TooYoung.Provider.Mongo.BootStrap
open AutoMapper
open AutoMapperBuilder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open MongoDB.Driver
open TooYoung.Domain.Authorization.UserGroup
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Repositories
open TooYoung.Provider.Mongo.Repositories

let addMongoProviderMapping (cfg: IMapperConfigurationExpression) =
    let dirProfile =
        let dir = prepare<FileDirectory>
        let entity = prepare<FileDirectoryEntity>
        
        automapper {
            map dir entity
            resolve
                entity.FileChildren
                (fun (src: FileDirectory) _ ->
                    src.FileChildren |> List.map (fun x -> x.Id))
            reverseMap
            swap
            notMap dir.PendingOperations
            create
        }

    let accessDefProfile =
        let definition = prepare<AccessDefinition>
        let entity = prepare<AccessDefinitionEntity>
        automapper {
            map definition entity
            resolve
                entity.Constraint
                ( fun (src: AccessDefinition) _ ->
                    match src.Constraint with
                    | All -> "*"
                    | Instance x -> x
                )
            resolve
                entity.AccessOperation
                ( fun (src: AccessDefinition) _ ->
                    match src.AccessOperation with
                    | Any -> "*"
                    | Action x -> x
                )
            reverseMap
            swap
            resolve
                definition.Constraint
                ( fun (src: AccessDefinitionEntity) _ ->
                    match src.Constraint with
                    | "*" -> All
                    | x -> Instance x
                )
            resolve
                definition.AccessOperation
                ( fun (src: AccessDefinitionEntity) _ ->
                    match src.AccessOperation with
                    | "*" -> Any
                    | x -> Action x
                )
            reverseMap
            create
        }
    cfg.AddProfile accessDefProfile

    let userGroupProfile =
        let group = prepare<UserGroup>
        let entity = prepare<UserGroupEntity>
        automapper {
            map group entity
            resolve
                entity.Definitions
                ( fun (src: UserGroup) _ ->
                    src.AccessDefinitions |> List.map (Mapper.Map<AccessDefinition, AccessDefinitionEntity>)
                )
            reverseMap
            swap
            resolve
                group.AccessDefinitions
                ( fun (src: UserGroupEntity) _ ->
                    src.Definitions |> List.map (Mapper.Map<AccessDefinitionEntity, AccessDefinition>)
                )
            create
        }
    cfg.AddProfile userGroupProfile

    cfg.AddProfile(dirProfile)

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
    services.AddScoped<IFileRepository, FileRepository>()
            .AddScoped<ISharingRepository, SharingRepository>()
            .AddScoped<IDirectoryRepository, DirectoryRepository>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddSingleton<IMongoDatabase>(
                fun provider ->
                    // using settings object to avoid url encoding
                    let credential = MongoCredential.CreateCredential(config.DatabaseName, config.UserName, config.Password)
                    let settings = MongoClientSettings(Credential = credential)
                    settings.Server <- MongoServerAddress(config.Address, config.Port)
                    let client = MongoClient(settings) :> IMongoClient
                    client.GetDatabase(config.DatabaseName)
            )
module  TooYoung.Provider.Mongo.BootStrap
open AutoMapper
open AutoMapperBuilder
open Microsoft.Extensions.DependencyInjection
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

    cfg.AddProfile(dirProfile)

let AddMongoDbRepository (services: IServiceCollection) =
    services.AddTransient<IFileRepository, FileRepository>()
            .AddTransient<ISharingRepository, SharingRepository>()
            .AddTransient<IDirectoryRepository, DirectoryRepository>()
            .AddTransient<AccountRepository>()

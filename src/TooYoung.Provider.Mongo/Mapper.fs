module TooYoung.Provider.Mongo.Mapper

module FileDirectory =
    open TooYoung.Domain.FileDirectory
    open TooYoung.Provider.Mongo.Enities

    let toEntity (model: FileDirectory): FileDirectoryEntity =
        FileDirectoryEntity
         ( Id = model.Id,
           IsRoot = model.IsRoot,
           Name = model.Name,
           OwnerId = model.OwnerId,
           ParentId = model.ParentId,
           DirectoryChildren = model.DirectoryChildren,
           FileChildren = (model.FileChildren |> List.map (fun x -> x.Id))
         )

    let toModel (entity: FileDirectoryEntity): FileDirectory =
        FileDirectory
            ( entity.Id, entity.OwnerId, entity.IsRoot,
              Name = entity.Name, DirectoryChildren = entity.DirectoryChildren,
              ParentId = entity.ParentId, FileChildren = List.Empty
            )
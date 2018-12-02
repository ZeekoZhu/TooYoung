namespace TooYoung.Provider.Mongo.Repositories
open System
open System.Collections.Generic
open MongoDB.Driver
open TooYoung
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Repositories
open TooYoung.Provider.Mongo
open FsToolkit.ErrorHandling

open Utils
open FunxAlias
open TooYoung.Provider.Mongo.Enities
open WebCommon.Impure

type DirectoryRepository(db: IMongoDatabase, files: IFileRepository) =
    let mapDir = Mapper.FileDirectory.toEntity
    let dirs = db.GetCollection<FileDirectoryEntity>("FileDirectory")

    let setFileChildren (x: Async<FileDirectoryEntity option>) =
        x |>
        Async.bind (function
            | Some entity ->
              async {
                  let! children = files.ListByIdAsync (entity.FileChildren |> ofCsList)
                  let dir = Mapper.FileDirectory.toModel(entity)
                  dir.FileChildren <- children
                  return Some dir
              }
            | None -> Async.fromValue None
            )

    let getUserRootDir userId =
            dirs.Find(fun x -> x.IsRoot && x.OwnerId = userId)
                .FirstOrDefaultAsync()
            |> Async.AwaitTask
            |> Async.map Option.ofObj
            |> setFileChildren

    let getDir dirId userId =
        dirs.Find(fun x -> x.Id = dirId && x.OwnerId = userId)
            .FirstOrDefaultAsync()
        |> Async.AwaitTask
        |> Async.map Option.ofObj
        |> setFileChildren

    let saveNewNode (dir: FileDirectory) =
        dirs.InsertOneAsync(dir |> mapDir)
        |> Async.AwaitTask
        |> Async.map (just Ok dir)

    let updateNode (dir: FileDirectory) =
        dirs.FindOneAndReplaceAsync<FileDirectoryEntity,FileDirectoryEntity>
            ((fun x -> x.Id = dir.Id), dir |> mapDir)
        |> Async.AwaitTask
        |> Async.map (just Ok ())

    let containsName name (dir: FileDirectory) =
        dirs.Find(fun x -> x.ParentId = dir.Id && x.Name = name)
            .AnyAsync()
        |> Async.AwaitTask

    let updateById update id =
        dirs.FindOneAndUpdateAsync<FileDirectoryEntity,FileDirectoryEntity>
            ((fun x -> x.Id = id), update)
            |> Async.AwaitTask
             |> Async.map (just Ok ())

    let attachSubDirsAsync (dir: FileDirectory) (subDirIds: string list) =
        let update = Builders<FileDirectoryEntity>.Update.AddToSetEach((fun x -> x.DirectoryChildren :> IEnumerable<string>), subDirIds)
        updateById update dir.Id
    
    let attachFilesAsync (dir: FileDirectory) (fileIds: string list) =
        let update = Builders<FileDirectoryEntity>.Update.AddToSetEach((fun x -> x.FileChildren :> IEnumerable<string>), fileIds)
        updateById update dir.Id
             
    let deattachSubDirs (dir: FileDirectory) (subDirIds: string list) =
        let update = Builders<FileDirectoryEntity>.Update.PullAll((fun e -> e.DirectoryChildren :> IEnumerable<string>), subDirIds)
        updateById update dir.Id
    
    let deattachFiles (dir: FileDirectory) (fileIds: string list) =
        let update = Builders<FileDirectoryEntity>.Update.PullAll((fun x -> x.FileChildren :> IEnumerable<string>), fileIds)
        updateById update dir.Id

    let deleteDir (dir: FileDirectory) =
        dirs.DeleteOneAsync(fun x -> x.Id = dir.Id)
        |> Async.AwaitTask
        |> Async.map (just Ok ())
         
    interface IDirectoryRepository with
        member this.BeginTransaction() =
            new MongoUnitOfWork(db.Client) :> UnitOfWork

        member this.GetRootDir userId =
            try getUserRootDir userId
            with _ -> asyncNone()

        member this.GetDir dirId userId =
            try getDir dirId userId
            with _ -> asyncNone()

        member this.SaveNewNode dir =
            saveNewNode dir

        member this.UpdateNode dir =
            updateNode dir

        member this.ContainsName name dir =
            containsName name dir

        member this.AttachSubDirsAsync dir subDirs =
            attachSubDirsAsync dir subDirs

        member this.AttachFilesAsync dir files =
            attachFilesAsync dir files

        member this.DeattachFilesAsync dir files =
            (deattachFiles dir) files

        member this.DeattachSubDirsAsync dir subDirs =
            (deattachSubDirs dir) subDirs

        member this.DeleteDir dir =
            deleteDir dir
        
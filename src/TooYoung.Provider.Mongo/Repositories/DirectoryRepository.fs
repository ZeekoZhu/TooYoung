namespace TooYoung.Provider.Mongo.Repositories
open System
open System.Collections.Generic
open AutoMapper
open MongoDB.Driver
open TooYoung
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Repositories
open TooYoung.Provider.Mongo

open Utils
open Asyncx
open FunxAlias
open AutoMapperBuilder

/// 为了方便映射对目录下子文件的引用，所以为目录创建了一个用来表达存储关系的实体
[<AllowNullLiteral>]
type FileDirectoryEntity() =
    member val Id = String.Empty with get, set
    member val OwnerId = String.Empty with get, set
    member val IsRoot = false with get, set
    member val Name = String.Empty with get, set
    member val ParentId = String.Empty with get, set
    member val DirectoryChildren = List.empty<string> with get, set
    member val FileChildren = List.empty<string> with get, set

type DirectoryRepository(db: IMongoDatabase, mapper: IMapper, files: IFileRepository) =
    let mapDir = mapper.Map<FileDirectoryEntity>
    let dirs = db.GetCollection<FileDirectoryEntity>("FileDirectory")

    let setFileChildren (x: Async<FileDirectoryEntity option>) =
        x |>
        Async.bind (function
            | Some entity ->
              async {
                  let! children = files.ListByIdAsync entity.FileChildren
                  let dir = mapper.Map<FileDirectory>(entity)
                  dir.FileChildren <- children
                  return Some dir
              }
            | None -> Async.fromValue None
            )

    let getUserRootDir userId =
            dirs.Find(fun x -> x.IsRoot && x.OwnerId = userId)
                .FirstOrDefaultAsync()
            |> Async.AwaitTask
            <!> Option.ofObj
            |> setFileChildren

    let getDir dirId userId =
        dirs.Find(fun x -> x.Id = dirId && x.OwnerId = userId)
            .FirstOrDefaultAsync()
        |> Async.AwaitTask
        <!> Option.ofObj
        |> setFileChildren

    let saveNewNode (dir: FileDirectory) =
        dirs.InsertOneAsync(dir |> mapDir)
        |> Async.AwaitTask
        <!> just Ok dir

    let updateNode (dir: FileDirectory) =
        dirs.FindOneAndReplaceAsync<FileDirectoryEntity,FileDirectoryEntity>
            ((fun x -> x.Id = dir.Id), dir |> mapDir)
        |> Async.AwaitTask
        <!> just Ok ()

    let containsName name (dir: FileDirectory) =
        dirs.Find(fun x -> x.ParentId = dir.Id && x.Name = name)
            .AnyAsync()
        |> Async.AwaitTask

    let updateById update id =
        dirs.FindOneAndUpdateAsync<FileDirectoryEntity,FileDirectoryEntity>
            ((fun x -> x.Id = id), update)
            |> Async.AwaitTask
             <!> just Ok ()

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
        <!> just Ok ()
         
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
            onError None saveNewNode dir

        member this.UpdateNode dir =
            onError None updateNode dir

        member this.ContainsName name dir =
            try containsName name dir
            with _ -> Async.fromValue false

        member this.AttachSubDirsAsync dir subDirs =
            onError None (attachSubDirsAsync dir) subDirs

        member this.AttachFilesAsync dir files =
            onError None (attachFilesAsync dir) files

        member this.DeattachFilesAsync dir files =
            onError None (deattachFiles dir) files

        member this.DeattachSubDirsAsync dir subDirs =
            onError None (deattachSubDirs dir) subDirs

        member this.DeleteDir dir =
            onError None deleteDir dir
        
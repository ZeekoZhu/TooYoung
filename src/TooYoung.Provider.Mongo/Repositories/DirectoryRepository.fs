namespace TooYoung.Provider.Mongo.Repositories

open FsToolkit.ErrorHandling
open MongoDB.Driver
open MongoDB.Driver.Linq
open System
open System.Linq
open System.Collections.Generic
open TooYoung
open TooYoung.Domain
open TooYoung.Domain.FileDirectory
open TooYoung.Domain.Repositories
open TooYoung.Domain.Resource
open TooYoung.FunxAlias
open TooYoung.Provider.Mongo
open TooYoung.Provider.Mongo.Enities
open TooYoung.Provider.Mongo.MongoHelper
open Utils
open WebCommon.Impure

type DirectoryRepository(db: IMongoDatabase, fileRepo: IFileRepository) =
    let mapDir = Mapper.FileDirectory.toEntity
    let dirs = db.GetCollection<FileDirectoryEntity>("FileDirectory")
    let files = db.GetCollection<FileInfo>("FileInfo")

    let setFileChildren (x: Async<FileDirectoryEntity option>) =
        x |>
        Async.bind (function
            | Some entity ->
              async {
                  let! children = fileRepo.ListByIdAsync (entity.FileChildren |> ofCsList)
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

    let getDirs (dirIds: string list) userId =
        dirs.Find(fun x -> dirIds.Contains(x.Id) && x.OwnerId = userId)
            .ToListAsync()
        |> Async.AwaitTask
        |> Async.map (List.ofSeq >> List.map Mapper.FileDirectory.toModel)
        

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

    let containsFileName fileName (dir: FileDirectory) =
        dirs.AsQueryable()
            .Where(fun d -> d.Id = dir.Id)
            .SelectMany(fun d -> d.FileChildren :> IEnumerable<_>)
            .GroupJoin(files, (fun d -> d), (fun f -> f.Id), (fun _ f -> f))
            .SelectMany(fun f -> f)
            .Where(fun f -> f.Name = fileName)
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
    
    let getParentDir (file: FileInfo) =
        dirs.Find(fun x -> x.FileChildren.Contains(file.Id))
            .FirstOrDefaultAsync()
        |> Async.AwaitTask
        |> Async.map (Option.ofObj)
        |> Async.map (Option.map Mapper.FileDirectory.toModel)
        |> AsyncResult.ofSome
            (fun _ -> "Can not find parent directory" |> Validation |> Error)
         
    interface IDirectoryRepository with
        member this.BeginTransaction() =
            new MongoUnitOfWork(db.Client) :> UnitOfWork

        member this.GetRootDir userId =
            try getUserRootDir userId
            with _ -> asyncNone()

        member this.GetDir dirId userId =
            try getDir dirId userId
            with _ -> asyncNone()
            
        member __.GetDirs dirIds userId =
            getDirs dirIds userId

        member this.SaveNewNode dir =
            saveNewNode dir

        member this.UpdateNode dir =
            updateNode dir

        member this.ContainsName name dir =
            containsName name dir
        
        member this.ContainsFileName name dir =
            containsFileName name dir

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
        
        member this.GetParentDirAsync x =
            getParentDir x
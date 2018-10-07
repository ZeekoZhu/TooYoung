namespace TooYoung.Provider.Mongo.Repositories
open System
open System.IO
open System.Linq
open MongoDB.Driver
open MongoDB.Driver.GridFS
open FSharp.Control.Tasks.V2
open TooYoung
open TooYoung.Domain.Repositories
open TooYoung.Domain.Resource
open TooYoung.Provider.Mongo
open FsToolkit.ErrorHandling
open Utils

type FileRepository(db: IMongoDatabase) =
    let files = db.GetCollection<FileInfo>("FileInfo")
    let gridFs = GridFSBucket(db, GridFSBucketOptions(BucketName = "FileBinary"))
    /// 插入一个文件信息
    let addAsync fileInfo =
        files.InsertOneAsync(fileInfo)
        |> Async.AwaitTask
        |> Async.map (fun _ -> Ok fileInfo)

    /// 插入一个文件的二进制信息
    let createBinaryAsync bytes =
        let bin = FileBinary(Guid.NewGuid().ToString())
        bin.Binary <- bytes
        gridFs.UploadFromBytesAsync(bin.Id, bin.Binary)
        |> Async.AwaitTask
        |> Async.map (fun _ -> Ok bin)
        
    /// 删除一个文件的二进制信息
    let deleteBinaryAsync binId =
        let filter = Builders<GridFSFileInfo>.Filter.Eq((fun x -> x.Filename), binId)
        task {
            use cursor = gridFs.Find(filter)
            let! gridInfo = cursor.FirstOrDefaultAsync()
            do! gridFs.DeleteAsync(gridInfo.Id)
            return Ok()
        }
        |> Async.AwaitTask

    let getByIdAndUserAsync userId fileInfoId =
        files
            .Find(fun x -> x.Id = fileInfoId && x.OwnerId = userId)
            .FirstOrDefaultAsync()
            |> Async.AwaitTask
            |> Async.map Option.ofObj

    let updateAsync (fileInfo: FileInfo) =
        let filter = Builders<FileInfo>.Filter.Eq((fun x -> x.Id), fileInfo.Id)
        files.FindOneAndReplaceAsync(filter, fileInfo)
        |> Async.AwaitTask
        |> Async.map (fun _ -> Ok())

    let existsAsync fileInfoId userId =
        files
            .Find(fun x -> x.Id = fileInfoId && x.OwnerId = userId)
            .AnyAsync()
        |> Async.AwaitTask

    let deleteFileAsync fileInfoId =
        files.DeleteOneAsync(fun x -> x.Id = fileInfoId)
        |> Async.AwaitTask
        |> Async.map (fun _ -> Ok())
    
    let filterByBinaryId binId = Builders<GridFSFileInfo>.Filter.Eq((fun f -> f.Filename), binId)

    let getBinaryAsync binaryId =
        let filter = filterByBinaryId binaryId
        task {
            let! file = gridFs.DownloadAsBytesByNameAsync(binaryId)
            let result = FileBinary(binaryId)
            result.Binary <- file
            return Ok result
        }
        |> Async.AwaitTask
        
    let getBinStreamAsync binaryId =
        let filter = filterByBinaryId binaryId
        task {
            let! stream = gridFs.OpenDownloadStreamByNameAsync(binaryId, GridFSDownloadByNameOptions(Seekable = Nullable(true)))
            return Ok (stream :> Stream)
        }
        |> Async.AwaitTask

    interface IFileRepository with
        member this.BeginTransaction() =
            new MongoUnitOfWork(db.Client) :> UnitOfWork

        member this.AddAsync fileInfo =
            onError None addAsync fileInfo

        member this.ListByIdAsync ids =
            try
                files.Find(fun f -> ids.Contains(f.Id)).ToListAsync()
                |> Async.AwaitTask
                |> Async.map List.ofSeq
            with _ -> Async.fromValue []
        
        member this.GetByIdAsync id =
            try
                files.Find(fun f -> f.Id = id).FirstOrDefaultAsync()
                |> Async.AwaitTask
                |> Async.map Option.ofObj
            with _ -> None |> Async.fromValue
            
        member this.CreateBinaryAsync bytes =
            onError None createBinaryAsync bytes

        member this.DeleteBinaryAsync binId =
            onError None deleteBinaryAsync binId
        
        member this.GetByIdAndUserAsync userId fileInfoId =
            try
                getByIdAndUserAsync userId fileInfoId
            with _ -> None |> Async.fromValue
            
        member this.UpdateAsync fileInfo =
            onError None updateAsync fileInfo

        member this.ExistsAsync fileInfoId userId =
            try
                existsAsync fileInfoId userId
            with _ -> false |> Async.fromValue

        member this.DeleteFileAsync fileInfoId =
            onError None deleteFileAsync fileInfoId
        
        member this.GetBinaryAsync binId =
            onError None getBinaryAsync binId
            
        member this.GetBinaryStreamAsync binId =
            onError None getBinStreamAsync binId
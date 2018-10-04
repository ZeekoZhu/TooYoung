namespace TooYoung.Domain.Services
open System
open System.Collections.Generic
open TooYoung.Domain.Repositories
open TooYoung.Domain.Resource
open TooYoung
open Microsoft.FSharp.Core.Result

type FileInfoAddDto =
    { Name: string
      Extension: string
      Metadatas: Dictionary<string, string>
    }

open Asyncx
open Cvdm.ErrorHandling.Helpers


open FunxAlias
open TooYoung.Domain.Repositories
open TooYoung.Domain.Repositories
type FileService(repo: IFileRepository) =
    let startWork f = Repository.startWork repo f
    let unitWork f = Repository.unitWork repo f

    let copyMetadata (src: Dictionary<string, string>) (target: Dictionary<string, string>) =
        for KeyValue(key, value) in src do
            target.Add(key, value)

    let setBinary (fileInfo: FileInfo) (bytes: byte []) =
        // 持久化二进制数据
        repo.CreateBinaryAsync bytes
        >=> (fun bin ->
                fileInfo.BinaryId <- bin.Id
                fileInfo.FileSize <- bin.Binary.Length
                fileInfo
            )

    let deleteBinary (fileInfo: FileInfo) =
        if String.IsNullOrEmpty(fileInfo.BinaryId)
        then fileInfo |> Ok |> async.Return
        else repo.DeleteBinaryAsync fileInfo.BinaryId
             >=> (fun _ ->
                    fileInfo.BinaryId <- String.Empty
                    fileInfo
                )

    let tryGetFile fileInfoId userId =
        repo.GetByIdAndUserAsync userId fileInfoId
        |> Async.fromOption (Error "File not found")
        
    let prepareDownload fileInfoId =
        repo.GetByIdAsync fileInfoId
        <!> (function
            | None -> Error "File not found"
            | Some info -> Ok info
            ) 
        <!> Result.bind (function
            | x when String.IsNullOrEmpty x.BinaryId = false -> Ok x
            | _ -> Error "File is empty"
            )

    let downloadRange (fileInfoId: string) (fromPos: int64) (toPos: int64) =
        prepareDownload fileInfoId
        <!> Result.bind (fun file -> if int64 file.FileSize >= toPos then Ok file.BinaryId else Error "Out of range")
        =>> repo.GetBinaryStreamAsync
        =>> (fun stream ->
                stream.Seek(fromPos, IO.SeekOrigin.Begin) |> ignore
                let buffer = Array.zeroCreate(int (toPos - fromPos + 1L))
                stream.AsyncRead(buffer, 0, buffer.Length)
                <!> (fun x -> Ok buffer)
            )

    /// 创建一个新的文件，但是并没有为其设置内容
    member this.Add (dto: FileInfoAddDto, ownerId) =
        let fileInfo = FileInfo(Guid.NewGuid().ToString(), ownerId, dto.Name)
        copyMetadata dto.Metadatas fileInfo.Metadatas
        fileInfo.Extension <- dto.Extension
        startWork (fun _ -> repo.AddAsync(fileInfo))

    /// 为一个文件设置文件内容，之前设置的内容会被彻底删除
    member this.SetContentAsync (fileInfoId: string, bytes: byte [], userId: string) =
        // 先删除旧的数据，然后设置新的数据
        let deleteAndSetNew = deleteBinary >+> (flip setBinary bytes)
        tryGetFile fileInfoId userId
        =>> unitWork deleteAndSetNew
        
    
    /// 更新一个文件的内容
    member this.UpdateFileInfo (fileInfoId: string, userId: string, dto: FileInfoAddDto) =
        tryGetFile fileInfoId userId
        >=> (fun info ->    // 更新文件实体
                info.Name <- dto.Name
                info.Extension <- dto.Extension
                copyMetadata dto.Metadatas info.Metadatas
                info
            )
        =>> unitWork repo.UpdateAsync// 持久化
 

    /// 删除一个文件
    member this.DeleteFile (fileinfoId: string, userId: string) =
        repo.ExistsAsync fileinfoId userId
        |> Async.bind (function
            | false -> Error "File not found" |> async.Return
            | true ->
                startWork (fun _ -> repo.DeleteFileAsync fileinfoId)
            )

    /// 根据 id 获取文件信息
    member this.GetByIds (ids: string list) =
        repo.ListByIdAsync ids

    /// 根据 id 获取文件信息
    member this.GetById (id) =
        repo.GetByIdAsync id
        |> Async.fromOption (Error "File not found")

    /// 获取指定文件的内容
    member this.GetFileBinary (fileInfoId: string) =
        prepareDownload fileInfoId
        >=> (fun x -> x.BinaryId)
        =>> repo.GetBinaryAsync

    /// 当文件大小超过 100M 的时候，应该使用这个方法来获取 Stream
    member this.GetBinaryStream (binaryId: string) =
        repo.GetBinaryStreamAsync binaryId

    member this.GetFileBinaryRange (fileInfoId: string) (fromPos: int64) (toPos: int64) =
         downloadRange fileInfoId fromPos toPos
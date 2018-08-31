namespace TooYoung.Domain.Services
open System
open System.Collections.Generic
open System.Threading.Tasks
open TooYoung.Domain.Repositories
open TooYoung.Domain.Resource
open TooYoung
open Microsoft.FSharp.Core.Result

type FileInfoAddDto =
    { Name: string
      Extension: string
      Metadatas: Dictionary<string, string>
    }

open TaskxAlias
open FunxAlias
open TooYoung.Domain.Repositories
open TooYoung.Domain.Repositories
module Taskx = FSharpx.Task
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
                Ok fileInfo
            )

    let deleteBinary (fileInfo: FileInfo) =
        if String.IsNullOrEmpty(fileInfo.BinaryId)
        then fileInfo |> Ok |> Task.FromResult
        else repo.DeleteBinaryAsync fileInfo.BinaryId
             >=> (fun _ ->
                    fileInfo.BinaryId <- String.Empty
                    Ok fileInfo
                )

    let tryGetFile fileInfoId userId =
        repo.GetByIdAndUserAsync userId fileInfoId
        <!> (function 
            | None -> Error "File not found"
            | Some info -> Ok info
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
                Ok info
            )
        =>> unitWork repo.UpdateAsync// 持久化
        

    /// 删除一个文件
    member this.DeleteFile (fileinfoId: string, userId: string) =
        repo.ExistsAsync fileinfoId userId
        >>= (function
            | false -> Error "File not found" |> Task.FromResult
            | true ->
                startWork (fun _ -> repo.DeleteFileAsync fileinfoId)
            )
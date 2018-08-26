namespace TooYoung.Domain.Services
open System
open System.Collections.Generic
open System.Threading.Tasks
open TooYoung.Domain.Repositories
open TooYoung.Domain.Resource
open Microsoft.FSharp.Core.Result

type FileInfoAddDto =
    { Name: string
      OwnerId: string
      Extension: string
      Metadatas: Dictionary<string, string>
    }



type FileService(repo: IFileRepository) =
    let copyMetadata (src: Dictionary<string, string>) (target: Dictionary<string, string>) =
        for KeyValue(key, value) in src do
            target.Add(key, value)

    let setBinary (fileInfo:FileInfo) (bytes: byte []) =
        FSharpx.Task.task {
            let! bin = repo.CreateBinaryAsync bytes
            return
                match bin with
                | Ok bin ->
                    fileInfo.BinaryId <- bin.Id
                    fileInfo.FileSize <- bin.Binary.Length
                    Ok fileInfo
                | Error (e: string) -> Error e
        }
    member this.Add (dto: FileInfoAddDto) =
        let fileInfo = FileInfo(Guid.NewGuid().ToString(),dto.OwnerId, dto.Name)
        copyMetadata dto.Metadatas fileInfo.Metadatas
        repo.AddAsync(fileInfo)


    member this.SetContentAsync (fileInfoId: string, bytes: byte [], userId: string) =
        FSharpx.Task.task {
            let! fileInfo = repo.GetByIdAsync fileInfoId
            return 0
        }

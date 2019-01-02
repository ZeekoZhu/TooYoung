namespace TooYoung.App

open AppErrors
open FSharp.Control.Tasks.V2
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open HeyRed.Mime
open MimeTypes
open System
open System.IO
open System.Security.Cryptography
open System.Text
open TooYoung.Domain.Repositories
open TooYoung.Domain.Services
open TooYoung
open FunxAlias
open TooYoung.Async
open Utils

type AppFileInfo = TooYoung.Domain.Resource.FileInfo

// 上传文件
//      文件保存到临时文件夹
//      获取 MIME
//          拓展名 MediaTypeMap.Core || Binary Mime
//      获取 MD5

type FileMetadata =
    { Mime: string
      ETag: string
    }

type DownloadInfo =
    { ETag: string
      From: int
      To: int option
    }

type FileAppService
    ( fileSvc: FileService,
      fileRepo: IFileRepository
    ) =
    let octetStream = "application/octet-stream"
    /// save file to tmp file
    let saveToTmp (stream: Stream) =
        let tmpFile = Path.GetTempFileName()
        let fileInfo = FileInfo(tmpFile)
        task {
            use fs = fileInfo.OpenWrite ()
            do! stream.CopyToAsync fs
            do! fs.FlushAsync()
        }
        |> Async.AwaitTask
        |> Async.map (fun _ -> fileInfo)
        
    /// Save file to a tmp dir
    let getFileMetadata (stream: Stream) (fileName: string) =
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        let mimeFromFileName = MimeTypeMap.GetMimeType(fileName)
        let mime =
            if mimeFromFileName = octetStream
            then MimeGuesser.GuessMimeType(stream)
            else mimeFromFileName
        use md5 = MD5.Create()
        stream.Seek(0L, SeekOrigin.Begin) |> ignore
        let etag =
            md5.ComputeHash(stream)
            |> BitConverter.ToString
            |> (fun str -> str.Replace("-", "").ToLowerInvariant())
        { Mime = mime; ETag = etag }
    
    let uploadFile (stream: Stream) (fileInfo: AppFileInfo) =
        let updateFileInfo (file: AppFileInfo) =
            saveToTmp stream
            |> Async.map (fun info -> Ok (file, info))
            <>> ( fun (file, info) ->
                    let metadata = getFileMetadata stream file.Name
                    file.Metadatas.Item("Mime") <- metadata.Mime
                    file.Metadatas.Item("ETag") <- metadata.ETag
                    file.FileSize <- int info.Length
                    file, info
                )
            >>= (fun (file, info) ->
                    fileRepo.UpdateAsync file
                    <>> (fun _ -> (file, info))
                )
            >>= (fun (file, info) -> 
                    async {
                        use fs = info.OpenRead()
                        let! result = fileSvc.UploadStreamAsync file fs
                        info.Delete()
                        return result
                    }
                )
            <>> (fun _ -> file)
        updateFileInfo fileInfo

    let checkPermission fileInfoId userId =
        fileSvc.GetById fileInfoId
        >>= ( fun file ->
                if file.OwnerId = userId then AsyncResult.retn file
                else AsyncResult.returnError (Forbidden "Permission denied")
            )

    member this.UploadFileStream (userId: Guid) (fileInfoId: string) (stream: Stream) =
        checkPermission fileInfoId (userId.ToString())
        >>= uploadFile stream
    
    member this.PrepareForDownload fileInfoId fileName =
        let checkFileInfo fileName (fileInfo: AppFileInfo) =
            if fileInfo.Name <> fileName then AsyncResult.returnError <| NotFound "File not found"
            else AsyncResult.retn fileInfo
            
        let getStream (fileInfo: AppFileInfo) =
            fileSvc.GetBinaryStream fileInfo.BinaryId
            <>> (fun stream -> fileInfo, stream)
        fileSvc.GetById fileInfoId
        >>= checkFileInfo fileName
        >>= getStream

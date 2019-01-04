module TooYoung.Api.Handlers.FileHandlers
open FSharp.Control.Tasks.V2
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open Microsoft.Net.Http.Headers
open System
open System.Collections.Generic
open System.IO
open TooYoung
open TooYoung.Api.ServiceAccessor
open TooYoung.App
open TooYoung.Domain.Services
open TooYoung.Domain.Sharing
open TooYoung.WebCommon
open TooYoung.WebCommon.ErrorMessage

type AddFileDto =
    { DirId: string
      FileName: string
    }

let addFile (dto: AddFileDto): HttpHandler =
    fun next ctx ->
        let fileSvc = getFileSvc ctx
        let dirSvc = getDirSvc ctx
        let userId = ctx.UserId()
        let fileInfoAddDto: FileInfoAddDto =
            { Name = dto.FileName
              Metadatas = Dictionary()
            }
        Async.combine
            (dirSvc.GetDir (dto.DirId, userId))
            (fileSvc.Add (fileInfoAddDto, userId))
        |> AsyncResult.bind
            ( fun (dir, file) ->
                dirSvc.AddFile file dir
            )
        |> AppResponse.appResult next ctx

let uploadFile (fileInfoId: string): HttpHandler =
    fun next ctx ->
        let userId = ctx.UserGuid()
        let fileAppSvc = getFileAppSvc ctx
        ctx.Request.EnableBuffering()
        task {
            return!
                fileAppSvc.UploadFileStream userId fileInfoId ctx.Request.Body 
                |> AppResponse.appResult next ctx
        }

let getFileById (fileInfoId: string): HttpHandler =
    fun next ctx ->
        let fileSvc = getFileSvc ctx
        fileSvc.GetById fileInfoId
        |> AppResponse.appResult next ctx
        
let getDownloadInfo (ctx: HttpContext) =
    let range = ctx.Request.GetTypedHeaders().Range.Ranges |> Seq.tryHead
    let info =
        let tag = ctx.Request.GetTypedHeaders().IfRange.EntityTag.Tag.ToString()
        {ETag = tag; From = 0; To = None}
    match range with
    | Some range ->
        { info with
              From = range.From |> Option.ofNullable |> Option.defaultValue 0L |> int
              To = range.To |> Option.ofNullable |> Option.map int
        }
    | None -> { info with From = 0; To = None}

let getAccessClaim (ctx: HttpContext) : AccessClaim =
    let token =
        ctx.GetQueryStringValue("token")
        |> ( function
           | Ok s ->
                let pwd =
                    ctx.GetQueryStringValue("pwd")
                    |> Result.fold id (fun _ -> String.Empty)
                (s, DateTime.Now, pwd) |> Some
           | Error _ -> None
        )
    let referer =
        ctx.Request.GetTypedHeaders().Referer |> Option.ofObj |> Option.map (fun uri -> uri.AbsolutePath) |> Option.defaultValue "" |> Some
    { Host = referer; Token = token }

/// 下载文件
let downloadFile (fileInfoId: string) (fileName: string): HttpHandler =
    fun next ctx ->
        let fileAppSvc = getFileAppSvc ctx
        let accessClaim = getAccessClaim ctx
        let userId = ctx.UserId()
        task {
            let! result =
                fileAppSvc.PrepareForDownload fileInfoId fileName accessClaim userId
                >>= fileAppSvc.GetDownloadStream
            return!
                match result with
                | Error e -> ErrorMessage.appErrorToStatus e next ctx
                | Ok (fileInfo, respStream) ->
                    ctx.Response.ContentType <- fileInfo.Metadatas.Item "Mime"
                    streamData true respStream
                        (fileInfo.Metadatas.Item "ETag" |> sprintf "\"%s\"" |> StringSegment |> EntityTagHeaderValue |> Some)
                        None
                        next ctx
        }

let deleteFile (fileInfoId: string): HttpHandler =
    fun next ctx ->
        let fileAppSvc = getFileAppSvc ctx
        let userId = ctx.UserId()
        fileAppSvc.DeleteFile fileInfoId userId
        |> AppResponse.appResult next ctx

/// 测试能否下载文件
let testAccess (fileInfoId:string) (fileName: string):HttpHandler =
    fun next ctx ->
        let claim = getAccessClaim ctx
        let fileAppSvc = getFileAppSvc ctx
        let userId = ctx.UserId()
        fileAppSvc.PrepareForDownload fileInfoId fileName claim userId
        |> AppResponse.appResult next ctx

let getFileInfos (fileInfoIds: string list):HttpHandler =
    fun next ctx ->
        let userId = ctx.UserId()
        let fileAppSvc = getFileAppSvc ctx
        task {
            let! result = fileAppSvc.GetFileInfo fileInfoIds userId
            return! json result next ctx
        }

/// route
let routes: HttpHandler =
    subRouteCi "/files"
        ( choose
            [ POST >=> AuthGuard.requireAcitveUser >=> choose
                [ routeCi "/" >=> bindJson<AddFileDto> addFile
                  routeCi "/query" >=> bindJson<string list> getFileInfos 
                  routeCif "/%s/content" uploadFile
                ]
              GET >=> choose
                [ routeCif "/ping/%s/%s" (fun (x, y) -> testAccess x y)
                  routeCif "/%s/%s" ( fun (x, y) -> downloadFile x y)
                ]
              DELETE >=> AuthGuard.requireAcitveUser >=> choose
                [ routeCif "/%s" deleteFile
                ]
            ]
        )
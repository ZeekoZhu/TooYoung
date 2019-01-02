module TooYoung.Api.Handlers.FileHandlers
open System
open System.Collections.Generic
open System.IO
open Microsoft.AspNetCore.Http
open Giraffe
open TooYoung.Domain.Services
open TooYoung.WebCommon
open FSharp.Control.Tasks.V2
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Primitives
open Microsoft.Net.Http.Headers
open TooYoung
open TooYoung.WebCommon.ErrorMessage
open TooYoung.Api.ServiceAccessor
open TooYoung.App

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

let downloadFile (fileInfoId: string) (fileName: string): HttpHandler =
    fun next ctx ->
        let fileAppSvc = getFileAppSvc ctx
        task {
            let! result = fileAppSvc.PrepareForDownload fileInfoId fileName
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

/// route
let routes: HttpHandler =
    subRouteCi "/files"
        ( choose
            [ POST >=> choose
                [ routeCi "/" >=> bindJson<AddFileDto> addFile
                  routeCif "/%s/content" uploadFile
                ]
              GET >=> choose
                [ routeCif "/%s/%s" ( fun (x, y) -> downloadFile x y)
                  routeCif "/%s" getFileById
                ]
            ]
        )
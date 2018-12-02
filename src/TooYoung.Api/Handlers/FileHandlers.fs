module TooYoung.Api.Handlers.FileHandlers
open System.Collections.Generic
open System.IO
open Microsoft.AspNetCore.Http
open Giraffe
open TooYoung.Domain.Services
open TooYoung.WebCommon
open FSharp.Control.Tasks.V2
open FsToolkit.ErrorHandling
open TooYoung

let getFileSvc (ctx: HttpContext) = ctx.GetService<FileService>()
let getDirSvc (ctx: HttpContext) = ctx.GetService<DirectoryService>()

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
        task {
            let! result =
                Async.combine
                    (dirSvc.GetDir (dto.DirId, userId))
                    (fileSvc.Add (fileInfoAddDto, userId))
                |> AsyncResult.bind
                    ( fun (dir, file) ->
                        dirSvc.AddFile file dir
                    )
            return! jsonResult result 400 next ctx
        }

let uploadFile (fileInfoId: string): HttpHandler =
    fun next ctx ->
        let userId = ctx.UserId()
        let fileSvc = getFileSvc ctx
        ctx.Request.EnableBuffering()
        task {
            use ms = new MemoryStream()
            do! ctx.Request.Body.CopyToAsync(ms)
            let bodyBytes = ms.ToArray()
            let! result = fileSvc.SetContentAsync(fileInfoId, bodyBytes, userId)
            return! jsonResult result 400 next ctx
        }

let getFileById (fileInfoId: string): HttpHandler =
    fun next ctx ->
        let fileSvc = getFileSvc ctx
        task {
            let! result = fileSvc.GetById fileInfoId
            return! jsonResult result 400 next ctx
        }

let downloadFile (fileInfoId: string): HttpHandler =
    fun next ctx ->
        let fileSvc = getFileSvc ctx
        task {
            let! result =
                fileSvc.GetById fileInfoId
                |> AsyncResult.bind (fun _ -> fileSvc.GetFileBinary fileInfoId)
            return!
                match result with
                | Error err -> jsonResult result 400 next ctx
                | Ok fb ->
                    ctx.WriteBytesAsync (fb.Binary)
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
                [ routeCif "/%s/content" downloadFile
                  routeCif "/%s" getFileById
                ]
            ]
        )
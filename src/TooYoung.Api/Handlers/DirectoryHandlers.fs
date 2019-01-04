module TooYoung.Api.Handlers.DirectoryHandlers
open System
open Giraffe
open Microsoft.AspNetCore.Http
open TooYoung.Domain.Services
open TooYoung.WebCommon
open TooYoung.WebCommon.ErrorMessage
open FSharp.Control.Tasks.V2
open TooYoung.App

let getDirSvc (ctx: HttpContext) = ctx.GetService<DirectoryService>()

let getRootDir (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    task {
         let! rootDir = dirSvc.GetRootDirectory (ctx.UserId())
         return! jsonResult rootDir 400 next ctx
    }

let getDir (dirId: string) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    dirSvc.GetDir (dirId, ctx.UserId())
    |> AppResponse.appResult next ctx

let getDirWithPath (dirId: string) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    dirSvc.GetDirWithPath dirId (ctx.UserId())
    |> AppResponse.appResult next ctx

let createDir (dto: DirectoryAddDto) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    dirSvc.CreateDirectory dto (ctx.UserId())
    |> AppResponse.appResult next ctx
    

type DirRenameDto =
    { DirId: string
      Name: string
    }
    member this.HasError () =
        if (Guid.TryParse (this.DirId) |> fst) = false then Some "Invalid directory id"
        elif String.IsNullOrWhiteSpace this.Name then Some "Directory name cannot be empty"
        else None
    interface IModelValidation<DirRenameDto> with
        member this.Validate () =
            match this.HasError() with
            | Some msg -> Error (RequestErrors.badRequest (text msg))
            | None -> Ok this

let renameDir (dto: DirRenameDto) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    dirSvc.Rename (ctx.UserId()) dto.DirId dto.Name
    |> AppResponse.appResult next ctx

let deleteDir (dirId: string): HttpHandler =
    fun next ctx ->
        let dirSvc = getDirSvc ctx
        dirSvc.DeleteDir (ctx.UserId()) dirId true
        |> AppResponse.appResult next ctx

let getDirs (dirIds: string list): HttpHandler =
    fun next ctx ->
        let dirAppSvc = ctx.GetService<DirectoryAppService>()
        task {
            let! result = dirAppSvc.GetDirs dirIds (ctx.UserId())
            return! json result next ctx
        }


let routes: HttpHandler =
    subRouteCi "/dir"
        ( AuthGuard.requireAcitveUser >=> choose
            [ GET >=> choose
                [ routeCi "/root" >=> getRootDir
                  routeCif "/%s/path" getDirWithPath
                  routeCif "/%s" getDir
                ]
              POST >=> choose
                [ routeCi "/" >=> bindJson<DirectoryAddDto> createDir
                  routeCi "/query" >=> bindJson<string list> getDirs
                ]
              PUT >=> choose
                [ routeCi "/rename" >=> bindJson<DirRenameDto> renameDir
                ]
              DELETE >=> choose
                [ routeCif "/%s" deleteDir
                ]
            ]
        )
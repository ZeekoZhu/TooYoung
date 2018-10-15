
module TooYoung.Api.Handlers.DirectoryHandlers
open Giraffe
open Microsoft.AspNetCore.Http
open TooYoung.Domain.Services
open TooYoung.WebCommon
open FSharp.Control.Tasks.V2

let getDirSvc (ctx: HttpContext) = ctx.GetService<DirectoryService>()

let getRootDir (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    task {
         let! rootDir = dirSvc.GetRootDirectory (ctx.UserId())
         return! jsonResult rootDir 404 next ctx
    }

let getDir (dirId: string) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    task {
        let! dir = dirSvc.GetDir (dirId, ctx.UserId())
        return! jsonResult dir 404 next ctx
    }

let getDirWithPath (dirId: string) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    task {
        let! path = dirSvc.GetDirWithPath dirId (ctx.UserId())
        return! jsonResult path 404 next ctx
    }

let createDir (dto: DirectoryAddDto) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let dirSvc = getDirSvc ctx
    task {
        let! result = dirSvc.CreateDirectory dto (ctx.UserId())
        return! jsonResult result 400 next ctx
    }



let routes: HttpHandler =
    subRouteCi "/dir"
        ( choose
            [ GET >=> choose
                [ routeCi "/root" >=> getRootDir
                  routeCif "/%s" getDir
                  routeCif "/%s/path" getDirWithPath
                ]
              POST >=> choose
                [ routeCi "/" >=> bindJson<DirectoryAddDto> createDir
                ]
            ]
        )
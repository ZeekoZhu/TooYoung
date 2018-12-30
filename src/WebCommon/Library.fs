namespace TooYoung.WebCommon
open System
open System.Runtime.CompilerServices
open FSharp.Control.Tasks.V2
open Microsoft.AspNetCore.Http
open System.Linq
open System.Security.Claims;
open Giraffe

module ErrorMessage =
    type ErrorMessage = {
        Error: string
    }
    
    let appErrorToStatus error =
        let errMsg, status =
            match error with
            | Validation e -> e, 400
            | Forbidden e -> e, 403
            | Unauthorized e -> e, 401
            | Multiple e -> "Multiple error happened", 500
        json {Error = errMsg} >=> setStatusCode status

    let jsonResult x errorStatus =
        x
        |> function
            | Ok x -> json x >=> setStatusCode 200
            | Error e -> json {Error = e} >=> setStatusCode errorStatus

module AppResponse =
    open ErrorMessage
    let appResult next ctx (asyncResult: Async<Result<'t, AppError>>) : HttpFuncResult =
        task {
            let! result = asyncResult
            let fn =
                match result with 
                | Ok x -> setStatusCode 200 >=> json x
                | Error e -> appErrorToStatus e
            return! fn next ctx
        }

[<AutoOpen>][<Extension>]
module HttpContextHelper =
    [<Extension>]
    let UserId (ctx: HttpContext) =
        ctx.User.Claims.FirstOrDefault(fun c -> c.Type = ClaimTypes.NameIdentifier)
        |> Option.ofObj
        |> function
            | Some claim -> claim.Value
            | None -> String.Empty

    type HttpContext with
        member this.UserGuid () =
            let str = UserId this
            Guid str
        member this.UserId () =
            UserId this
        member this.UserName () =
            this.User.Claims.FirstOrDefault(fun c -> c.Type = ClaimTypes.Name)
            |> Option.ofObj
            |> function
                | Some claim -> claim.Value
                | None -> String.Empty
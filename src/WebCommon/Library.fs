namespace TooYoung.WebCommon
open System
open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Http
open System.Linq
open System.Security.Claims;
open Giraffe

[<AutoOpen>]
module ErrorMessage =
    type ErrorMessage = {
        Message: String
    }
    
    let jsonResult x errorStatus =
        x
        |> function
            | Ok x -> json x >=> setStatusCode 200
            | Error e -> json {Message = e} >=> setStatusCode errorStatus

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
        member this.UserId () =
            UserId this

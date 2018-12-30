module TooYoung.Api.Handlers.AuthGuard

open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Http
open Giraffe
open TooYoung.Api.ServiceAccessor
open TooYoung.WebCommon
open FSharp.Control.Tasks.V2
open FsToolkit.ErrorHandling
open TooYoung.WebCommon
open TooYoung.WebCommon
open Utils

let authenticationScheme = "Cookies"
let notLoggedIn =
    RequestErrors.UNAUTHORIZED authenticationScheme "too young" "You must sign in first"

let requireLogin f = requiresAuthentication notLoggedIn f

let requireUnlocked: HttpHandler =
    fun next ctx ->
        let accountRepo = getAccountRepo ctx
        task {
            let! user = accountRepo.FindByIdAsync (ctx.UserGuid())
            return!
                match user with
                | Some user -> user.Locked
                | None -> false
                |> function
                    | true ->
                        ErrorMessage.appErrorToStatus
                            (Forbidden "Your account has been locked")
                            next ctx
                    | false -> next ctx
        }

let requireAcitveUser: HttpHandler = requireLogin >=> requireUnlocked

let requireAdmin: HttpHandler =
    fun next ctx ->
        let accountSvc = getAccountSvc ctx
        task {
            let! result = 
                accountSvc.IsAdmin (ctx.UserGuid())
            return!
                if result then next ctx            
                else
                    ErrorMessage.appErrorToStatus
                        (Forbidden "You don't have super power")
                        next ctx
        }





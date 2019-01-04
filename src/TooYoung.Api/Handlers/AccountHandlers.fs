module TooYoung.Api.Handlers.AccountHandlers

open FSharp.Control.Tasks.V2
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Giraffe
open Giraffe.HttpStatusCodeHandlers
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Cryptography.KeyDerivation
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open System
open System.Security.Claims
open System.Text
open TooYoung.Api.Handlers
open TooYoung.Api.Handlers.AuthGuard
open TooYoung.App
open TooYoung.Async
open TooYoung.Domain.Authorization.UserGroup
open TooYoung.Domain.Repositories
open TooYoung.Domain.Services
open TooYoung.Domain.User
open TooYoung.WebCommon
open Utils
open TooYoung.Api.ServiceAccessor
open TooYoung.WebCommon
open TooYoung.Api.Handlers.Helper


let signIn (ctx: HttpContext) (user: User) =
    let identity = ClaimsIdentity(authenticationScheme)
    identity.AddClaim(Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String))
    identity.AddClaim(Claim(ClaimTypes.Name, user.UserName, ClaimValueTypes.String))
    
    let principal = ClaimsPrincipal(identity)
    ctx.SignInAsync(authenticationScheme, principal,AuthenticationProperties(IsPersistent = true))

let signOut (ctx: HttpContext) =
    ctx.SignOutAsync(authenticationScheme)

let login (model: LoginModel) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let accountRepo = getAccountRepo ctx
    let accountSvc = getAccountSvc ctx
    let deny = RequestErrors.FORBIDDEN "Don't know who you are"
    task {
        let! user = accountRepo.FindByUserName model.UserName
        return! match user with
                | None -> deny next ctx
                | Some user ->
                    if accountSvc.ValidateLogin model user
                    then task {
                            do! signIn ctx user
                            return! Successful.OK "Welcome~" next ctx
                        }
                    else deny next ctx
    }

let logout next ctx =
    task {
        do! signOut ctx
        return! Successful.OK "Bye~" next ctx
    }

let ping (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let accountRepo = getAccountRepo ctx
    task {
        let! user = accountRepo.FindByUserName (ctx.UserName())
        return! match user with
                | Some user -> json user next ctx
                | None -> text "Login first" next ctx
    }

let register (model: RegisterModel) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let accountSvc = getAccountSvc ctx
    task {
        let! result = accountSvc.CreateAccount model
        let fn =
            match result with
            | Error e -> RequestErrors.BAD_REQUEST e
            | Ok dir -> Successful.ok (json dir)
        return! fn next ctx
    }

[<CLIMutable>]
type UserProfileModel =
    { User: User
      Permissions: AccessDefinition list
      IsAdmin: bool
    }

let profile =
    fun next ctx ->
        let accountRepo = getAccountRepo ctx
        let accountSvc = getAccountSvc ctx
        let authSvc = getAuthService ctx
        task {
            let! user = accountRepo.FindByUserName (ctx.UserName())
            let! userGroup = authSvc.GetGroupByName (ctx.UserName())
            let! isAdmin = accountSvc.IsAdmin (ctx.UserGuid())
            let isAdmin =
                match isAdmin with
                | Ok x -> x
                | Error _ -> false
            let fn =
                match (user, userGroup) with
                | (Some user, Some group) ->
                    let model =
                        { User = user
                          Permissions = group.AccessDefinitions
                          IsAdmin = isAdmin
                        }
                    Successful.ok (json model)
                | _ -> raise (InvalidState("User has signed in but not found"))
            return! fn next ctx
        }

let updateProfile (userId: Guid) (model: UpdateProfileModel): HttpHandler =
    fun next ctx ->
        let authSvc = getAuthService ctx
        let accountSvc = getAccountSvc ctx
        let checkPermission () =
            if userId = ctx.UserGuid()
            then AsyncResult.retn userId
            else
                accountSvc.IsAdmin (ctx.UserGuid())
                >>= (fun isAdmin ->
                    if isAdmin
                    then AsyncResult.retn userId
                    else AsyncResult.returnError (Forbidden "Permission denied")
                )
        let update userId =
            accountSvc.UpdateProfile userId model
        
        checkPermission ()
        >>= update
        >>= ( fun user ->
                signIn ctx user
                |> Async.AwaitTask
                |> Async.map (fun _ -> Ok user)
            )
        |> AppResponse.appResult next ctx

let setLocked (userId: Guid, locked: bool): HttpHandler =
    fun next ctx ->
        let accountSvc = getAccountSvc ctx
        accountSvc.SetLockState userId locked
        |> AppResponse.appResult next ctx

let listUsers: HttpHandler =
    fun next ctx ->
        let accountSvc = getAccountSvc ctx
        task {
            let! result = accountSvc.GetAllUsers()
            return! json result next ctx
        }

let deleteUser userId: HttpHandler =
    fun next ctx ->
        let accountSvc = getAccountSvc ctx
        accountSvc.UserIdShouldExist userId
        >>= accountSvc.DeleteUser
        |> AppResponse.appResult next ctx

let routes: HttpHandler =
    subRouteCi "/account"
        ( choose
            [ POST >=> routeCi "/login" >=> bindJson<LoginModel> login
              POST >=> routeCi "/register" >=> bindJson<RegisterModel> register
              POST >=> routeCi "/logout" >=> requireAcitveUser >=> logout
              GET >=> routeCi "/users" >=> requireAdmin >=> listUsers
              GET >=> routeCi "/ping" >=> requireAcitveUser >=> ping
              GET >=> routeCi "/profile" >=> requireAcitveUser >=> profile
              PUT >=> subRouteCi "/profile"
                        requireAcitveUser
                        >=> combineParam
                                (routeCif "/%O")
                                (bindJson<UpdateProfileModel>)
                                updateProfile
              PATCH >=> routeCif "/%O/lock/%b" setLocked
              DELETE >=> routeCif "/%O" (fun userId -> requireAdmin >=> (deleteUser userId)) 
            ]
        )
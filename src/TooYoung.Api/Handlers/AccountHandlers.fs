module TooYoung.Api.Handlers.AccountHandlers

open System
open System.Security.Claims
open Giraffe
open System.Text
open TooYoung.Domain.Repositories
open FSharp.Control.Tasks.V2
open FsToolkit.ErrorHandling
open Giraffe.HttpStatusCodeHandlers
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Cryptography.KeyDerivation
open TooYoung.Api.Handlers
open TooYoung.Domain.User
open TooYoung.Domain.Services
open TooYoung.WebCommon
open TooYoung.Api.Handlers.AuthGuard
open TooYoung.App
open TooYoung.Domain.Authorization.UserGroup
open TooYoung.WebCommon

let getAccountRepo (ctx: HttpContext) = ctx.GetService<IAccountRepository>()
let getDirService (ctx: HttpContext) = ctx.GetService<DirectoryService>()
let getAuthService (ctx: HttpContext) = ctx.GetService<AuthorizationService>()
let getGetHashSalt (ctx: HttpContext) = ctx.GetService<IConfiguration>().GetSection("HashSalt")
let getAccountSvc (ctx: HttpContext) = ctx.GetService<AccountAppService>()

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

let isAdmin =
    List.contains
        { Target = "admin"
          AccessOperation = AccessOperation.Any
          Constraint = AccessConstraint.All
          Restrict = false
        }

let profile =
    fun next ctx ->
        let accountRepo = getAccountRepo ctx
        let authSvc = getAuthService ctx
        task {
            let! user = accountRepo.FindByUserName (ctx.UserName())
            let! userGroup = authSvc.GetGroupByName (ctx.UserName())

            let fn =
                match (user, userGroup) with
                | (Some user, Some group) ->
                    let model =
                        { User = user
                          Permissions = group.AccessDefinitions
                          IsAdmin = isAdmin group.AccessDefinitions
                        }
                    Successful.ok (json model)
                | _ -> raise (InvalidState("User has signed in but not found"))
            return! fn next ctx
        }

let routes: HttpHandler =
    subRouteCi "/account"
        ( choose
            [ POST >=> routeCi "/login" >=> bindJson<LoginModel> login
              POST >=> routeCi "/register" >=> bindJson<RegisterModel> register
              POST >=> routeCi "/logout" >=> requireLogin >=> logout
              GET >=> routeCi "/ping" >=> requireLogin >=> ping
              GET >=> routeCi "/profile" >=> requireLogin >=> profile
            ]
        )
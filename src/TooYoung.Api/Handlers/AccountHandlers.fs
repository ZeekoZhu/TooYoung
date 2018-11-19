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

let getAccountRepo (ctx: HttpContext) = ctx.GetService<IAccountRepository>()
let getDirService (ctx: HttpContext) = ctx.GetService<DirectoryService>()
let getAuthService (ctx: HttpContext) = ctx.GetService<AuthorizationService>()
let getGetHashSalt (ctx: HttpContext) = ctx.GetService<IConfiguration>().GetSection("HashSalt")

[<CLIMutable>]
type LoginModel = 
    { UserName: string
      Password: string
    }

let hashPassword ctx password =
    let saltStr = getGetHashSalt ctx
    let salt = Encoding.UTF8.GetBytes(saltStr.Value.PadLeft(16, 'x'))
    let res = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100, 32)
              |> Convert.ToBase64String
    res

let validateLogin ctx (model: LoginModel) (user: User) =
    user.UserName = model.UserName
    && user.Password = hashPassword ctx (model.Password)

let signIn (ctx: HttpContext) (user: User) =
    let identity = ClaimsIdentity(authenticationScheme)
    identity.AddClaim(Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String))
    identity.AddClaim(Claim(ClaimTypes.Name, user.UserName, ClaimValueTypes.String))
    
    let principal = ClaimsPrincipal(identity)
    ctx.SignInAsync(authenticationScheme, principal,AuthenticationProperties(IsPersistent = true))

let login (model: LoginModel) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let accountRepo = getAccountRepo ctx
    let deny = RequestErrors.FORBIDDEN "Don't know who you are"
    task {
        let! user = accountRepo.FindByUserName model.UserName
        return! match user with
                | None -> deny next ctx
                | Some user ->
                    if validateLogin ctx model user
                    then task {
                            do! signIn ctx user
                            return! Successful.OK "Welcome~" next ctx
                        }
                    else deny next ctx
    }

let ping (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let accountRepo = getAccountRepo ctx
    task {
        let! user = accountRepo.FindByUserName (ctx.UserName())
        return! match user with
                | Some user -> json user next ctx
                | None -> text "Login first" next ctx
    }

type RegisterModel =
    { UserName: string
      Password: string
      DisplayName: string
      Email: string
    }
    member this.HasError() =
        if (String.IsNullOrEmpty this.UserName) then Some "UserName can not be empty"
        else if (String.IsNullOrEmpty this.Password) then Some "Password can not be empty"
        else if (this.Password.Length < 4) then Some "Length of password can not be less than 4"
        else None
    interface IModelValidation<RegisterModel> with
        member this.Validate() =
             match this.HasError() with
             | Some msg -> Error (RequestErrors.badRequest (text msg))
             | None -> Ok this

let initUserSpace ctx (user:User) =
    let dirSvc = getDirService ctx
    let authSvc = getAuthService ctx

    authSvc.CreateGroupForUserAsync user
    |> AsyncResult.bind
        ( fun _ ->
            dirSvc.CreateRootDir (user.Id.ToString())
        )


let createUser ctx (model:RegisterModel) =
    let password = hashPassword ctx model.Password
    User ( Guid.NewGuid(),
           UserName = model.UserName,
           Password = password,
           DisplayName = model.DisplayName,
           Email = model.Email
         )

let register (model: RegisterModel) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let accountRepo = getAccountRepo ctx
    task {
        let! user =  accountRepo.FindByUserName model.UserName
        return! match user with
                | Some _ -> RequestErrors.BAD_REQUEST "UserName has been taken" next ctx
                | None ->
                        let newUser = createUser ctx model
                        task {
                            let! fn = accountRepo.Create newUser
                                    |> AsyncResult.bind (initUserSpace ctx)
                                    |> Async.map
                                        ( function
                                        | Error e -> RequestErrors.BAD_REQUEST e
                                        | Ok dir -> Successful.ok (json dir)
                                        )
                            return! fn next ctx
                        }
    }

let routes: HttpHandler =
    subRouteCi "/account"
        ( choose
            [ POST >=> routeCi "/login" >=> bindJson<LoginModel> login
              POST >=> routeCi "/register" >=> bindJson<RegisterModel> register
              GET >=> routeCi "/ping" >=> requireLogin >=> ping
            ]
        )
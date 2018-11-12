module TooYoung.Api.Handlers.AccountHandlers

open System
open System.Security.Claims
open Giraffe
open System.Text
open TooYoung.Domain.Repositories
open FSharp.Control.Tasks.V2
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Cryptography.KeyDerivation
open TooYoung.Domain.User

let getAccountRepo (ctx: HttpContext) = ctx.GetService<IAccountRepository>()
let getGetHashSalt (ctx: HttpContext) = ctx.GetService<IConfiguration>().GetSection("HashSalt")

[<CLIMutable>]
type LoginModel = 
    { UserName: string
      Password: string
    }

let hashPassword ctx password =
    let saltStr = getGetHashSalt ctx
    let salt = Encoding.UTF8.GetBytes(saltStr.Value.PadLeft(16, 'x'))
    KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100, 32)
    |> Convert.ToBase64String

let validateLogin ctx (model: LoginModel) (user: User) =
    user.UserName = model.UserName
    && user.Password = hashPassword ctx (model.Password)

let signIn (ctx: HttpContext) (user: User) =
    let identity = ClaimsIdentity()
    identity.AddClaim(Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String))
    ClaimsPrincipal(identity)
    |> ctx.SignInAsync

let login (model: LoginModel) (next: HttpFunc) (ctx: HttpContext): HttpFuncResult =
    let accountRepo = getAccountRepo ctx
    let deny = RequestErrors.UNAUTHORIZED "Cookie" "TooYoung" "Don't know who you are"
    task {
        let! user = accountRepo.FindByUserName model.UserName
        return! match user with
                | None -> deny next ctx
                | Some user ->
                    if validateLogin ctx model user
                    then task {
                            do! signIn ctx user
                            return! deny next ctx
                        }
                    else Successful.OK "" next ctx
    }

let routes: HttpHandler =
    subRouteCi "/account"
        ( choose
            [ POST >=> choose
                [ routeCi "/" >=> bindJson<LoginModel> login    
                ]
            ]
        )
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
open TooYoung.WebCommon

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
    let res = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100, 32)
              |> Convert.ToBase64String
    res

let validateLogin ctx (model: LoginModel) (user: User) =
    user.UserName = model.UserName
    && user.Password = hashPassword ctx (model.Password)

let signIn (ctx: HttpContext) (user: User) =
    let identity = ClaimsIdentity()
    identity.AddClaim(Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String))
    identity.AddClaim(Claim(ClaimTypes.Name, user.UserName, ClaimValueTypes.String))
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
                     task {
                        let newUser = createUser ctx model
                        let! result = accountRepo.Create newUser
                        return! match result  with
                                | Error e -> ServerErrors.INTERNAL_ERROR e next ctx
                                | Ok user -> Successful.ok (json user) next ctx
                    }
    }

let routes: HttpHandler =
    subRouteCi "/account"
        ( choose
            [ POST >=> routeCi "/login" >=> bindJson<LoginModel> login
              POST >=> routeCi "/register" >=> bindJson<RegisterModel> register
              GET >=> routeCi "/ping" >=> ping
            ]
        )
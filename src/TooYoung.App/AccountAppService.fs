namespace TooYoung.App

open System
open System.Text
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Cryptography.KeyDerivation
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open FSharp.Control.Tasks.V2
open Giraffe
open TooYoung.Domain.Repositories
open TooYoung.Domain.Services
open TooYoung.Domain.User
open TooYoung.Async
[<CLIMutable>]
type LoginModel = 
    { UserName: string
      Password: string
    }

[<CLIMutable>]
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

type AccountAppService
        ( config: IConfiguration,
          accountRepo: IAccountRepository,
          dirSvc: DirectoryService,
          authSvc: AuthorizationService
        ) =
    /// salt to hash passowrd
    let saltStr = config.GetSection("HashSalt").Value
    
    /// hash password
    let hashPassword password =
        let salt = Encoding.UTF8.GetBytes(saltStr.PadLeft(16, 'x'))
        let res = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100, 32)
                  |> Convert.ToBase64String
        res
        
    /// initialize user space
    let initUserSpace (user:User) =
        authSvc.CreateGroupForUserAsync user
        |> AsyncResult.bind
            ( fun _ ->
                dirSvc.CreateRootDir (user.Id.ToString())
            )
     
    /// create new user
    let createUser (model:RegisterModel) =
        let password = hashPassword model.Password
        User ( Guid.NewGuid(),
               UserName = model.UserName,
               Password = password,
               DisplayName = model.DisplayName,
               Email = model.Email
             )
             
    member this.CreateAccount (model: RegisterModel) =
        async {
            let! user = accountRepo.FindByUserName model.UserName
            let uow = accountRepo.BeginTransaction()
            return!
                UnitOfWork.startWork uow
                    ( fun _ ->
                        match user with
                        | Some _ -> AsyncResult.returnError "UserName has been taken"
                        | None ->
                            let newUser = createUser model
                            accountRepo.Create newUser
                            >>= (initUserSpace)
                            <>> (fun _ -> newUser)
                    )
        }

    member this.ValidateLogin (model: LoginModel) (user: User) =
        user.UserName = model.UserName
        && user.Password = hashPassword (model.Password)

namespace TooYoung.App

open FSharp.Control.Tasks.V2
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Giraffe
open Microsoft.AspNetCore.Cryptography.KeyDerivation
open Microsoft.Extensions.Configuration
open System
open System.Text
open TooYoung
open TooYoung.Async
open TooYoung.Domain.Repositories
open TooYoung.Domain.Services
open TooYoung.Domain.User
open TooYoung.FunxAlias
open TooYoung.Domain.Authorization.UserGroup

open Microsoft.Extensions.Logging
open Utils

[<CLIMutable>]
type LoginModel = 
    { UserName: string
      Password: string
    }

[<CLIMutable>]
type UpdateProfileModel =
    { UserName: string
      DisplayName: string
      Email: string
      Password: string option
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
          authSvc: AuthorizationService,
          logger: ILogger<AccountAppService>
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

    
    let userIdShouldExist userId =
        accountRepo.FindByIdAsync userId
        |> AsyncResult.ofSome (fun _ -> Error (Validation "Target user not found"))
    
    let containsAdmin =
        List.contains
            { Target = "admin"
              AccessOperation = AccessOperation.Any
              Constraint = AccessConstraint.All
              Restrict = false
            }
    member this.UserIdShouldExist userId = userIdShouldExist userId

    member this.UpdateProfile (userId: Guid) (model: UpdateProfileModel) =
        let model = {model with Password = Option.map hashPassword model.Password }
        
        let userNameShouldNotExist () =
            accountRepo.FindByUserName model.UserName
            |> Async.map
                (Option.bind
                    ( fun user ->
                        if user.Id = userId then None else Some user)
                    )
            |> AsyncResult.ofNone (fun _ -> Error (Validation "UserName has been taken"))
        
        /// Update user model
        let updateUser (user: User) =
            user.UserName <- model.UserName
            user.DisplayName <- model.DisplayName
            match model.Password with
            | Some pwd -> user.Password <- pwd
            | None -> ()
            user.Email <- model.Email
            user
        
        /// update user group
        let updateUserGroupAsync (user: User) =
            authSvc.EnsureGroupByName user.UserName
            <>> (fun group -> group.Name <- model.UserName; group)
            >>= authSvc.UpdateGroupAsync
            <>> (fun _ -> user)
        
        userNameShouldNotExist ()
        >>= just userIdShouldExist userId
        >>= updateUserGroupAsync
        <>> updateUser
        >>= accountRepo.UpdateAsync

    member this.GetAllUsers () =
        accountRepo.GetAllUsers()

    member this.DeleteUser user =
        // todo: remove user group
        // todo: clear links
        // todo: clean user space
        accountRepo.DeleteUser user

    member this.CreateAccount (model: RegisterModel) =
        async {
            let! user = accountRepo.FindByUserName model.UserName
            let uow = accountRepo.BeginTransaction()
            return!
                UnitOfWork.startWork uow
                    ( fun _ ->
                        match user with
                        | Some _ -> AsyncResult.returnError (Validation "UserName has been taken")
                        | None ->
                            let newUser = createUser model
                            accountRepo.Create newUser
                            >>= (initUserSpace)
                            <>> (fun _ -> newUser)
                    )
        }

    member this.SetLockState userId locked =
        let setLockState (user: User) =
            user.Locked <- locked
            accountRepo.UpdateAsync user
        userIdShouldExist userId
        >>= setLockState

    member this.IsAdmin userId =
        let getPermissions (user: User) =
            authSvc.GetAccessDefinitionsForUserAsync user.Id "admin"
            
        userIdShouldExist userId
        >>= getPermissions
        <>> (containsAdmin)

    member this.ValidateLogin (model: LoginModel) (user: User) =
        user.UserName = model.UserName
        && user.Password = hashPassword (model.Password)

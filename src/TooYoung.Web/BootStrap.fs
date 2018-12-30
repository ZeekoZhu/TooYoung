module TooYoung.Web.BootStrap

open System
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Giraffe
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open TooYoung.App
open TooYoung.Domain.Services
open TooYoung.Domain.Repositories
open TooYoung.Domain.User
open TooYoung
open TooYoung
open TooYoung.Async
open Utils
open TooYoung.Domain.Authorization.UserGroup

[<CLIMutable>]
type BootstrapOptions =
    { Admin: RegisterModel
    }

let bootstrap (sp: IServiceProvider) =
    use scope = sp.CreateScope()
    let sp = scope.ServiceProvider
    let logger = sp.GetService<ILogger<BootstrapOptions>>()
    let authSvc = sp.GetService<AuthorizationService>()
    let accountSvc = sp.GetService<AccountAppService>()
    let bootstrapOptions = sp.GetService<IOptions<BootstrapOptions>>().Value
    let adminModel = bootstrapOptions.Admin
    let accountRepo = sp.GetService<IAccountRepository>()
    
    /// Add admin permission to user
    let addAdminPermission (user: User) =
        authSvc.GetGroupByName user.UserName
        |> Async.map
            ( Result.ofSome
                (fun _ -> raise (InvalidState "User should have its own group"))
            )
        >>= ( fun group ->
                group.AddPermissions
                   [ { Target = "admin"
                       Constraint = AccessConstraint.All
                       AccessOperation = AccessOperation.Any
                     }
                   ]
                authSvc.UpdateGroupAsync group
            ) 

    accountRepo.FindByUserName adminModel.UserName
    |> Async.bind
        ( function
          | Some _ ->
            logger.LogInformation("Admin user has been initialized")
            Async.fromValue (Ok ())
          | None ->
                accountSvc.CreateAccount adminModel
                >>= addAdminPermission
                <>> ( fun _ ->
                        logger.LogInformation ("Admin user initialized succeed")
                    )
                <>> ignore
        )

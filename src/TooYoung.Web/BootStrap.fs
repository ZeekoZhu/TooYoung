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
    let findAdminGroup () =
        authSvc.GetGroupByName "__admin"
    let createAdminGroup () =
        authSvc.CreateNewGroup "__admin"
           [ { Target = "admin"
               Constraint = AccessConstraint.All
               AccessOperation = AccessOperation.Any
             }
           ]
    /// Add user to admin group
    let addAdminPermission group (user: User) =
        authSvc.AddUserToAsync group user.Id

    findAdminGroup()
    |> Async.bind
        ( function
          | Some _ ->
            logger.LogInformation("Admin user has been initialized")
            Async.fromValue (Ok ())
          | None ->
                createAdminGroup()
                >>= (fun group ->
                    accountSvc.CreateAccount adminModel
                    >>= addAdminPermission group
                )
                <>> ( fun _ ->
                        logger.LogInformation ("Admin user initialized succeed")
                    )
                <>> ignore
        )

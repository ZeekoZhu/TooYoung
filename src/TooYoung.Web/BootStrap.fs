module TooYoung.Web.BootStrap

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open TooYoung.App
open TooYoung.Domain.Services
open TooYoung.Domain.Repositories

[<CLIMutable>]
type BootstrapOptions =
    { Admin: RegisterModel
    }

let bootstrap (services: IServiceCollection) =
    let sp = services.BuildServiceProvider()
    let authSvc = sp.GetService<AuthorizationService>()
    let accountSvc = sp.GetService<AccountAppService>()
    let bootstrapOptions = sp.GetService<IOptions<BootstrapOptions>>().Value
    let adminModel = bootstrapOptions.Admin
    let accountRepo = sp.GetService<IAccountRepository>()
    accountRepo.FindByUserName adminModel.UserName
    |> Async.bind
        ( function
          | Some _ -> async.Return()
          | None ->
                accountSvc.CreateAccount adminModel
                |> AsyncResult.bind
        )

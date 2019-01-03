module TooYoung.App.AppServices
open Microsoft.Extensions.DependencyInjection

let register (services: IServiceCollection) =
    services.AddScoped<AccountAppService>()
            .AddScoped<FileAppService>()
            .AddScoped<DirectoryAppService>()
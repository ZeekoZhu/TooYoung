namespace  TooYoung.Domain

module RegisterDomainServices =
    open Microsoft.Extensions.DependencyInjection
    open TooYoung.Domain.Services
    let register (services: IServiceCollection) =
        services.AddScoped<FileService>()
                .AddScoped<DirectoryService>()
                .AddScoped<SharingService>()
                .AddSingleton<EventBus>()
                .AddScoped<AuthorizationService>()


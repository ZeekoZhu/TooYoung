module TooYoung.Web.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Giraffe.Serialization.Json
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.CookiePolicy
open TooYoung.Api.Handlers
open TooYoung.Provider
open TooYoung.Domain
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open TooYoung.App
open TooYoung.Web.LoggingConfig
open TooYoung.Web.BootStrap

// ---------------------------------
// Web app
// ---------------------------------
let apiV1 = subRouteCi "/api/v1"
let webApp =
    choose [
        route "/alive" >=> text "OK"
        apiV1
            ( choose
                [ route "/alive" >=> text "OK"
                  AccountHandlers.routes
                  DirectoryHandlers.routes
                  FileHandlers.routes
                  QrCodeHandlers.routes
                ] )
           ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : Microsoft.Extensions.Logging.ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

/// Configure Application
let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
//        .UseCors(configureCors)
        .UseCookiePolicy(CookiePolicyOptions(HttpOnly = HttpOnlyPolicy.Always))
        .UseAuthentication()
        .UseStaticFiles()
    |> ignore
    app.UseGiraffe(webApp)
    app.UseSpaStaticFiles()
    
    app.UseSpa(fun spa ->
        spa.Options.SourcePath <- "client-app"
        if env.IsDevelopment() then
            spa.UseProxyToSpaDevelopmentServer("http://localhost:1234")
    )
    |> ignore

/// Configure services
let configureServices (hostBuilderCtx: WebHostBuilderContext) (services : IServiceCollection) =
//    services.AddCors()    |> ignore
    let customJsonSettings =
        JsonSerializerSettings()
    customJsonSettings.Converters.Insert(0, JsonConverters.OptionConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer(customJsonSettings))
    |> ignore
    services
        .AddGiraffe()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie()
        |> ignore
    services.AddSpaStaticFiles(fun cfg ->
            cfg.RootPath <- "client-app"
        )
    |> ignore
    services.Configure<BootstrapOptions>(hostBuilderCtx.Configuration.GetSection("BootStrap"))
    |> ignore
    services
    |> Mongo.BootStrap.addMongoDbRepository hostBuilderCtx.Configuration
    |> RegisterDomainServices.register
    |> AppServices.register
    |> ignore

[<EntryPoint>]
let main (args: string[]) =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    let webHost =
        WebHost.CreateDefaultBuilder(args)
               .UseWebRoot(webRoot)
               .Configure(Action<IApplicationBuilder> configureApp)
               .ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
               .UseSerilog()
               .UseUrls("http://localhost:1503")
               .UseKestrel(fun options ->
                    options.Limits.MaxRequestBodySize <- Nullable()
               )
               .Build()
    async {
        let! result = BootStrap.bootstrap (webHost.Services)
        return
            match result with
            | Ok _ -> webHost.Run(); 0
            | Error e ->
                Console.Error.Write(e)
                -1
    }
    |> Async.RunSynchronously

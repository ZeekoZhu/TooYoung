module TooYoung.Web.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.CookiePolicy
open TooYoung.Api.Handlers
open TooYoung.Provider.Mongo
open TooYoung.Domain
open Microsoft.AspNetCore.Http

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
                ] )
           ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
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
    app.UseSpaStaticFiles()
    app.UseGiraffe(webApp)
    app.Map(PathString("/app"), fun spaApp ->
        app.UseSpa(fun spa ->
            spa.Options.SourcePath <- "client-app"
            if env.IsDevelopment() then
                spa.UseProxyToSpaDevelopmentServer("http://localhost:3000")
        )
    )
    |> ignore

/// Configure services
let configureServices (hostBuilderCtx: WebHostBuilderContext) (services : IServiceCollection) =
//    services.AddCors()    |> ignore
    services
        .AddGiraffe()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie()
        |> ignore
    services.AddSpaStaticFiles(fun cfg ->
            cfg.RootPath <- "client-app"
        )
    |> ignore
    services
    |> BootStrap.addMongoDbRepository hostBuilderCtx.Configuration
    |> RegisterDomainServices.register
    |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main (args: string[]) =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    WebHost.CreateDefaultBuilder(args)
           .UseWebRoot(webRoot)
           .Configure(Action<IApplicationBuilder> configureApp)
           .ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
           .ConfigureLogging(configureLogging)
           .UseUrls("http://localhost:1503")
           .Build()
           .Run()
    0

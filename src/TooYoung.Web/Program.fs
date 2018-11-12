module TooYoung.Web.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

open AutoMapper
open Microsoft.AspNetCore
open TooYoung.Api.Handlers
open TooYoung.Api.Handlers
open TooYoung.Provider.Mongo

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
                ] )
        setStatusCode 404 >=> text "Not Found" ]

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
        .UseStaticFiles()
        .UseGiraffe(webApp)

/// Configure services
let configureServices (hostBuilderCtx: WebHostBuilderContext) (services : IServiceCollection) =
//    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services |> BootStrap.addMongoDbRepository hostBuilderCtx.Configuration
    |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main (args: string[]) =
    Mapper.Initialize(fun cfg -> BootStrap.addMongoProviderMapping cfg)
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
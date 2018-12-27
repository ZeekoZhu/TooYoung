module TooYoung.Web.LoggingConfig
open Microsoft.AspNetCore.Hosting
open Serilog

let useSerilog (context: WebHostBuilderContext) (config: LoggerConfiguration) =
    config.ReadFrom.Configuration(context.Configuration)
    |> ignore

type IWebHostBuilder with
    member builder.UseSerilog() =
        builder.UseSerilog(useSerilog)



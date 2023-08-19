namespace azure_function_fsharp.Startup

open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

open azure_function_fsharp.Infrastructure.Constants
open azure_function_fsharp.Infrastructure.DbConnection
open azure_function_fsharp.Infrastructure.Telemetry
open azure_function_fsharp.Infrastructure.Dapper
open azure_function_fsharp.Infrastructure.ErrorHandler

type Startup() =
    inherit FunctionsStartup()

    override this.Configure(builder: IFunctionsHostBuilder) =

        Dapper.registerOptionType ()

        let configuration = builder.GetContext().Configuration

        builder
            .Services
            .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
            .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()  
        |> ignore

        builder.Services.AddTransient<ErrorHandler>()
        |> ignore

[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()
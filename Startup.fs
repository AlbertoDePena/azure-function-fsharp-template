namespace azure_function_fsharp

open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

open azure_function_fsharp.Constants
open azure_function_fsharp.DataAccess
open azure_function_fsharp.Telemetry

type Startup() =
    inherit FunctionsStartup()

    override this.Configure(builder: IFunctionsHostBuilder) =

        OptionTypeHandler.register ()

        let configuration = builder.GetContext().Configuration

        builder
            .Services
            .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
            .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()  
        |> ignore

        if configuration.GetValue<bool> ConfigurationName.ENABLE_SQL_TELEMETRY then
            builder.Services.AddSingleton<ITelemetryInitializer, SqlTelemetryInitializer>()
            |> ignore

        builder.Services.AddTransient<ErrorHandler>()
        |> ignore

[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()
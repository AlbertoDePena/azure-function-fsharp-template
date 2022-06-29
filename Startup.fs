namespace azure_function_fsharp

open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

type Startup() =
    inherit FunctionsStartup()

    override this.Configure(builder: IFunctionsHostBuilder) =

        let configuration = builder.GetContext().Configuration

        builder
            .Services
            .AddOptions<GreeterOptions>()
            .Configure<IConfiguration>(fun settings configuration -> configuration.GetSection("Greeter").Bind(settings))
        |> ignore

        builder
            .Services
            .AddSingleton<ITelemetryInitializer, CloudRoleVersionInitializer>()
            .AddSingleton<ITelemetryInitializer, SqlTelemetryInitializer>()
        |> ignore

        if configuration.GetValue<bool> "ENABLE_DETAILED_SQL_TELEMETRY" then
            builder.Services.AddSingleton<ITelemetryInitializer, SqlTelemetryInitializer>()
            |> ignore

[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()
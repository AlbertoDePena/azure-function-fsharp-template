namespace azure_function_fsharp

open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

type Startup() =
    inherit FunctionsStartup()

    override this.Configure(builder: IFunctionsHostBuilder) =

        let configuration = builder.GetContext().Configuration

        OptionTypes.register ()

        builder
            .Services
            .AddOptions<ApplicationOptions>()
            .Configure<IConfiguration>(fun settings configuration -> configuration.GetSection("Application").Bind(settings))
        |> ignore

        builder
            .Services
            .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
            .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()  
        |> ignore

        if configuration.GetValue<bool> "ENABLE_SQL_TELEMETRY" then
            builder.Services.AddSingleton<ITelemetryInitializer, SqlTelemetryInitializer>()
            |> ignore

        builder.Services.AddTransient<FunctionsMiddleware>()
        |> ignore

[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()
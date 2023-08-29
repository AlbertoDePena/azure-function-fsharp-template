namespace azure_function_fsharp.Startup

open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect

open azure_function_fsharp.Infrastructure.Constants
open azure_function_fsharp.Infrastructure.Telemetry
open azure_function_fsharp.Infrastructure.Dapper
open azure_function_fsharp.Infrastructure.HttpRequestHandler

type Startup() =
    inherit FunctionsStartup()

    override this.Configure(builder: IFunctionsHostBuilder) =

        Dapper.registerOptionType ()

        let configuration = builder.GetContext().Configuration

        builder.Services
            .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
            .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()
        |> ignore

        let tenantId = configuration.GetValue<string>(ConfigurationKey.AzureAd_TenantId)

        let clientId = configuration.GetValue<string>(ConfigurationKey.AzureAd_ClientId)

        let metadataAddress =
            $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration?appid={clientId}"

        builder.Services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(
            ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, OpenIdConnectConfigurationRetriever())
        )
        |> ignore

        //builder.Services.AddHttpClient() |> ignore

        builder.Services.AddTransient<HttpRequestHandler>() |> ignore

[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()

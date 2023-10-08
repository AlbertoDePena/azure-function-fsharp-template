namespace MyFunctionApp.Startup

open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect

open MyFunctionApp.Infrastructure.Dapper
open MyFunctionApp.Infrastructure.Telemetry
open MyFunctionApp.Infrastructure.Authentication
open MyFunctionApp.Infrastructure.HttpRequestHandler
open MyFunctionApp.Infrastructure.Options

type Startup() =
    inherit FunctionsStartup()

    override this.Configure(builder: IFunctionsHostBuilder) =
        
        Dapper.registerOptionType ()

        builder.Services
            .AddOptions<Application>()
            .Configure<IConfiguration>(fun settings configuration ->
                configuration.GetSection(nameof Application).Bind(settings))
        |> ignore

        builder.Services
            .AddOptions<Database>()
            .Configure<IConfiguration>(fun settings configuration ->
                configuration.GetSection(nameof Database).Bind(settings))
        |> ignore

        builder.Services
            .AddOptions<AzureAd>()
            .Configure<IConfiguration>(fun settings configuration ->
                configuration.GetSection(nameof AzureAd).Bind(settings))
        |> ignore

        builder.Services
            .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
            .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()
        |> ignore

        builder.Services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(
            (fun serviceProvider ->
                let azureAdOptions = serviceProvider.GetRequiredService<IOptions<AzureAd>>()

                let metadataAddress =
                    $"https://login.microsoftonline.com/{azureAdOptions.Value.TenantId}/v2.0/.well-known/openid-configuration?appid={azureAdOptions.Value.ClientId}"

                ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataAddress,
                    OpenIdConnectConfigurationRetriever()
                )
                :> IConfigurationManager<OpenIdConnectConfiguration>)
        )
        |> ignore

        //builder.Services.AddHttpClient() |> ignore

        builder.Services.AddSingleton<Authentication>() |> ignore        
        builder.Services.AddTransient<HttpRequestHandler>() |> ignore

[<assembly: FunctionsStartup(typeof<Startup>)>]
do ()

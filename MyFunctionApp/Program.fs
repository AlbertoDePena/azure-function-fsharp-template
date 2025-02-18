namespace MyFunctionApp.Program

open Microsoft.Azure.Functions.Worker
open Microsoft.ApplicationInsights.Extensibility
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect

open MyFunctionApp.Infrastructure.Telemetry
open MyFunctionApp.HttpTriggers.HttpRequestHandler
open MyFunctionApp.Infrastructure.Options
open MyFunctionApp.Infrastructure.Database
open Microsoft.Extensions.Hosting

[<RequireQualifiedAccess>]
module Program =

    let host =
        HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureAppConfiguration(fun builder -> builder.AddEnvironmentVariables() |> ignore)
            .ConfigureServices(fun context services ->

                services.AddApplicationInsightsTelemetryWorkerService() |> ignore

                services.ConfigureFunctionsApplicationInsights() |> ignore

                services
                    .AddOptions<Database>()
                    .Configure<IConfiguration>(fun settings configuration ->
                        configuration.GetSection(nameof Database).Bind(settings))
                |> ignore

                services
                    .AddOptions<AzureAd>()
                    .Configure<IConfiguration>(fun settings configuration ->
                        configuration.GetSection(nameof AzureAd).Bind(settings))
                |> ignore

                services
                    .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
                    .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()
                |> ignore

                services.AddSingleton<UserDatabase>() |> ignore

                services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(
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

                services.AddHttpClient() |> ignore

                services.AddTransient<HttpRequestHandler>() |> ignore)
            .Build()

    host.Run()

namespace azure_function_fsharp.HttpTriggers.SayHello

open System
open System.Data

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.ApplicationInsights

open Dapper
open FsToolkit.ErrorHandling

open azure_function_fsharp.Infrastructure.DbConnection
open azure_function_fsharp.Infrastructure.Constants
open azure_function_fsharp.Domain.CustomTypes
open azure_function_fsharp.Infrastructure.HttpRequestHandler

type SayHello
    (
        configuration: IConfiguration,
        logger: ILogger<SayHello>,
        httpRequestHandler: HttpRequestHandler,
        telemetryClient: TelemetryClient
    ) =

    let createDbConnection =
        (fun () ->
            configuration.GetValue<string> ConfigurationKey.DbConnectionString_Main
            |> DbConnection.create)

    [<FunctionName(nameof SayHello)>]
    member this.Run([<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get)>] httpRequest: HttpRequest) =

        httpRequestHandler.Handle httpRequest [ Role.Viewer ] (fun () ->
            async {
                let guid = Guid.NewGuid()
                let correlationId = guid.ToString()

                let message = configuration.GetValue<string>(ConfigurationKey.MyApplication_Message)

                telemetryClient.GetMetric(MetricName.SayHello).TrackValue(1) |> ignore

                logger.LogDebug("what it do? {CorrelationId}", correlationId)

                return OkObjectResult(message) :> IActionResult
            })

namespace MyFunctionApp.HttpTriggers.SayHello

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

open MyFunctionApp.Infrastructure.DbConnection
open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Domain.ConstraintTypes
open MyFunctionApp.Infrastructure.HttpRequestHandler

type SayHello
    (
        configuration: IConfiguration,
        logger: ILogger<SayHello>,
        httpRequestHandler: HttpRequestHandler,
        telemetryClient: TelemetryClient
    ) =

    let createDbConnection () =
        ConfigurationKey.DbConnectionString_Main
        |> configuration.GetValue<string> 
        |> DbConnection.create

    [<FunctionName(nameof SayHello)>]
    member this.Run([<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get)>] httpRequest: HttpRequest) =

        httpRequestHandler.Handle httpRequest [ Role.Viewer ] (fun () ->
            async {
                let guid = Guid.NewGuid()
                let correlationId = guid.ToString()

                let message = 
                    ConfigurationKey.MyApplication_Message
                    |> configuration.GetValue<string>

                telemetryClient.GetMetric(MetricName.SayHello).TrackValue(1) |> ignore

                logger.LogDebug("what it do? {CorrelationId}", correlationId)

                return OkObjectResult(message) :> IActionResult
            })

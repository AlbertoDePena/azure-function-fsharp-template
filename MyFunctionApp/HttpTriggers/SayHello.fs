namespace MyFunctionApp.HttpTriggers.SayHello

open System
open System.Data

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.ApplicationInsights
open Microsoft.Extensions.Options

open Dapper
open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.DbConnection
open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Domain.ConstraintTypes
open MyFunctionApp.Infrastructure.HttpRequestHandler
open MyFunctionApp.Infrastructure.Options

type SayHello
    (
        logger: ILogger<SayHello>,
        applicationOptions: IOptions<Application>,
        databaseOptions: IOptions<Database>,
        httpRequestHandler: HttpRequestHandler,
        telemetryClient: TelemetryClient
    ) =

    let createDbConnection () =
        DbConnection.create databaseOptions.Value.ConnectionString

    [<FunctionName(nameof SayHello)>]
    member this.Run([<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get)>] httpRequest: HttpRequest) =

        httpRequestHandler.Handle httpRequest [ Role.Viewer ] (fun () ->
            async {
                let guid = Guid.NewGuid()
                let correlationId = guid.ToString()

                let message = applicationOptions.Value.Message

                telemetryClient.GetMetric(MetricName.SayHello).TrackValue(1) |> ignore

                logger.LogDebug("what it do? {CorrelationId}", correlationId)

                return OkObjectResult(message) :> IActionResult
            })

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

open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Domain.Invariants
open MyFunctionApp.Infrastructure.HttpRequestHandler
open MyFunctionApp.Infrastructure.Options
open MyFunctionApp.Domain.User

open MyFunctionApp.Infrastructure.UserRepository

type SayHello
    (
        logger: ILogger<SayHello>,
        applicationOptions: IOptions<Application>,
        databaseOptions: IOptions<Database>,
        httpRequestHandler: HttpRequestHandler,
        telemetryClient: TelemetryClient
    ) =

    [<FunctionName(nameof SayHello)>]
    member this.Run([<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get, Route = "v1/SayHello")>] httpRequest: HttpRequest) =

        httpRequestHandler.Handle httpRequest [ UserGroup.Viewer ] (fun userName ->
            async {
                let dbConnectionString =
                    databaseOptions.Value.ConnectionString
                    |> DbConnectionString.TryCreate
                    |> Result.valueOr failwith

                let emailAddress =
                    userName.Value
                    |> EmailAddress.TryCreate
                    |> Result.valueOr failwith

                let! userOption = UserRepository.tryFindByEmailAddress dbConnectionString emailAddress

                let guid = Guid.NewGuid()
                let correlationId = guid.ToString()

                let message = applicationOptions.Value.Message

                telemetryClient.GetMetric(MetricName.SayHello).TrackValue(1) |> ignore

                logger.LogDebug("what it do? {CorrelationId}", correlationId)

                return OkObjectResult(message) :> IActionResult
            })

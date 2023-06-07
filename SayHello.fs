namespace azure_function_fsharp

open System

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.ApplicationInsights


open System.Data
open azure_function_fsharp.Exceptions
open Dapper
open FsToolkit.ErrorHandling

open azure_function_fsharp.DataAccess
open azure_function_fsharp.Constants

type SayHello(configuration: IConfiguration, logger: ILogger<SayHello>, errorHandler: ErrorHandler, telemetryClient: TelemetryClient) =

    let createDbConnection =
        (fun () -> configuration.GetValue<string> ConfigurationKey.DB_CONNECTION_STRING |> DbConnection.create)

    [<FunctionName(nameof SayHello)>]
    member this.Run
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get)>] httpRequest: HttpRequest            
        ) =

        errorHandler.Handle httpRequest (fun () ->
            async {
                let guid = Guid.NewGuid()
                let correlationId = guid.ToString()

                let message = configuration.GetValue<string>(ConfigurationKey.APPLICATION_MESSAGE)

                telemetryClient.GetMetric(MetricName.SayHello).TrackValue(1) |> ignore

                logger.LogDebug("what it do? {CorrelationId}", correlationId)

                return OkObjectResult(message) :> IActionResult
            })

        

namespace azure_function_fsharp

open System

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.ApplicationInsights

open azure_function_fsharp.Constants

type Greeter(configuration: IConfiguration, errorHandler: ErrorHandler, telemetryClient: TelemetryClient) =

    [<FunctionName("SayHello")>]
    member this.SayHello
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get)>] httpRequest: HttpRequest,
            logger: ILogger
        ) =

        errorHandler.Handle httpRequest (fun () ->
            async {
                let guid = Guid.NewGuid()
                let correlationId = guid.ToString()

                let message = configuration.GetValue<string>("Application_Message")

                telemetryClient.GetMetric(MetricName.SayHello).TrackValue(1) |> ignore

                logger.LogDebug("what it do? {CorrelationId}", correlationId)

                return OkObjectResult(message) :> IActionResult
            })

        

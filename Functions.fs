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
open azure_function_fsharp.CustomTypes

type Greeter(configuration: IConfiguration, errorHandler: ErrorHandler, telemetryClient: TelemetryClient) =

    [<FunctionName("SayHello")>]
    member this.SayHello
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get)>] httpRequest: HttpRequest,
            logger: ILogger
        ) =

        errorHandler.Handle httpRequest (fun () ->
            async {
                let correlationId = Guid.NewGuid() |> fun x -> x.ToString()

                let message = configuration.GetValue<string>("Application_Message")

                telemetryClient.GetMetric(MetricName.SayHello).TrackValue(1) |> ignore

                logger.LogDebug(
                    LogEvent.AuthorizationError, 
                    Logging.Template, 
                    EntityType.Invoice,
                    "INV0023412", 
                    LogStatus.Failed, 
                    Checkpoint.Publisher, 
                    correlationId,                     
                    "Just testing structured logs"
                )

                return OkObjectResult(message) :> IActionResult
            })

        

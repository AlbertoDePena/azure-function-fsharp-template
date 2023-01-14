namespace azure_function_fsharp

open System

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration

open azure_function_fsharp.Constants

type Greeter(configuration: IConfiguration, functionsMiddleware: FunctionsMiddleware) =

    [<FunctionName("SayHello")>]
    member this.SayHello
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get)>] httpRequest: HttpRequest,
            logger: ILogger
        ) =

        functionsMiddleware.Execute httpRequest (fun () ->
            async {
                let guid = Guid.NewGuid()
                let correlationId = guid.ToString()

                let message = configuration.GetValue<string>("Application_Message")

                logger.LogDebug("what it do? {CorrelationId}", correlationId)

                return OkObjectResult(message) :> IActionResult
            })

        

namespace azure_function_fsharp

open System

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Options

open azure_function_fsharp.Constants
open azure_function_fsharp.Options

type Greeter(applicationOptions: IOptions<ApplicationOptions>, functionsMiddleware: FunctionsMiddleware) =

    [<FunctionName("SayHello")>]
    member this.SayHello
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get)>] httpRequest: HttpRequest,
            logger: ILogger
        ) =

        functionsMiddleware.Execute httpRequest (fun () ->
            async {
                let correlationId = Guid.NewGuid().ToString()

                let message = applicationOptions.Value.Message

                logger.LogDebug("what it do? {CorrelationId}", correlationId)

                return OkObjectResult(message) :> IActionResult
            })

        

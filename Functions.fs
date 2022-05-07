namespace azure_function_fsharp

open System

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Options

type Greeter(greeterOptions: IOptions<GreeterOptions>) =

    [<FunctionName("SayHello")>]
    member this.SayHello
        (
            [<HttpTrigger(AuthorizationLevel.Function, HttpMethod.Get)>] request: HttpRequest,
            logger: ILogger
        ) =

        let correlationId = Guid.NewGuid().ToString()

        let message = greeterOptions.Value.Message

        logger.LogDebug("what it do? {CorrelationId}", correlationId)

        OkObjectResult(message) :> IActionResult

namespace azure_function_fsharp

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Options

type Greeter(greeterConfigurationOptions: IOptions<GreeterConfiguration>) =

    let greeterConfiguration = greeterConfigurationOptions.Value

    [<FunctionName("SayHello")>]
    member this.SayHello
        (
            [<HttpTrigger(AuthorizationLevel.Function, HttpMethod.Get)>] request: HttpRequest,
            logger: ILogger
        ) =

        let message = greeterConfiguration.Message

        OkObjectResult(message) :> IActionResult

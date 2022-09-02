namespace azure_function_fsharp

open System.Web.Http

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.ApplicationInsights
open Microsoft.Extensions.Options

open FsToolkit.ErrorHandling

open azure_function_fsharp.Extensions
open azure_function_fsharp.Options

type FunctionsMiddleware
    (
        logger: ILogger<FunctionsMiddleware>,
        telemetryClient: TelemetryClient,
        applicationOptions: IOptions<ApplicationOptions>
    ) =

    /// <exception cref="InvalidOperationException"></exception>
    member this.GetUserName(httpRequest: HttpRequest) =
        invalidOp "The user name is not available in the HTTP request"

    /// <summary>Executes the computation and functions as a top level error handler</summary>
    member this.Execute (httpRequest: HttpRequest) (computation: unit -> Async<IActionResult>) =
        let computation =
            async {
                try
                    telemetryClient.Context.User.AuthenticatedUserId <- (this.GetUserName httpRequest)

                    let! actionResult = computation ()

                    return actionResult
                with
                | ex ->
                    logger.LogError(ex, ex.Message)

                    return InternalServerErrorResult() :> IActionResult
            }

        computation |> Async.StartAsTask
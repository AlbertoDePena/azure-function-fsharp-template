namespace azure_function_fsharp

open System.Web.Http

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.ApplicationInsights

open FsToolkit.ErrorHandling

open azure_function_fsharp.Exceptions
open azure_function_fsharp.Constants

type ErrorHandler
    (
        logger: ILogger<ErrorHandler>,
        telemetryClient: TelemetryClient
    ) =

    /// <exception cref="AuthenticationException"></exception>
    member this.GetUserName(httpRequest: HttpRequest) =
        "azure-function-user"

    /// <summary>Executes the computation and functions as a top level error handler</summary>
    member this.Handle (httpRequest: HttpRequest) (computation: unit -> Async<IActionResult>) =
        let computation =
            async {
                try
                    let userName = this.GetUserName httpRequest
                    
                    telemetryClient.Context.User.AuthenticatedUserId <- userName
                    
                    let! actionResult = computation ()

                    return actionResult
                with
                | :? AuthenticationException as ex ->
                    logger.LogDebug(LogEvent.AuthenticationError, ex, ex.Message)

                    return UnauthorizedResult() :> IActionResult

                | :? AuthorizationException as ex ->
                    logger.LogDebug(LogEvent.AuthorizationError, ex, ex.Message)

                    return ForbidResult() :> IActionResult

                | :? DataAccessException as ex ->
                    logger.LogError(LogEvent.DataAccessError, ex, ex.Message)

                    return InternalServerErrorResult() :> IActionResult

                | ex ->
                    logger.LogError(LogEvent.InternalServerError, ex, ex.Message)

                    return InternalServerErrorResult() :> IActionResult
            }

        computation |> Async.StartAsTask
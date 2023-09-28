namespace MyFunctionApp.Infrastructure.HttpRequestHandler

open System.Net
open System.Web.Http

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.ApplicationInsights

open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Exceptions
open MyFunctionApp.Infrastructure.Authorization
open MyFunctionApp.Infrastructure.Authentication
open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Domain.ConstraintTypes

type HttpRequestHandler
    (
        logger: ILogger<HttpRequestHandler>,
        telemetryClient: TelemetryClient,
        authentication: Authentication,
        authorization: Authorization
    ) =

    /// <exception cref="AuthenticationException"></exception>
    member this.GetUserName(httpRequest: HttpRequest) =
        try
            // TODO - get user from claims principal.
            "azure-function-user"
        with ex ->
            AuthenticationException(ex) |> raise

    /// <summary>Executes the computation and functions as a top level error handler</summary>
    member this.Handle (httpRequest: HttpRequest) (roles: Role list) (computation: unit -> Async<IActionResult>) =
        let computation =
            async {
                try
                    let! claimsPrincipal = authentication.Authenticate httpRequest

                    match claimsPrincipal with
                    | None -> return UnauthorizedResult() :> IActionResult
                    | Some claimsPrincipal ->

                        httpRequest.HttpContext.User <- claimsPrincipal

                        telemetryClient.Context.User.AuthenticatedUserId <- (this.GetUserName httpRequest)

                        if authorization.IsAuthorized (httpRequest, roles) then

                            let! actionResult = computation ()

                            return actionResult
                        else
                            logger.LogDebug(
                                LogEvent.AuthorizationError,
                                "The user is not authorized to access the requested resource"
                            )

                            return ForbidResult() :> IActionResult
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

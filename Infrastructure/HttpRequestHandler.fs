namespace azure_function_fsharp.Infrastructure.HttpRequestHandler

open System.Net
open System.Web.Http

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.ApplicationInsights

open FsToolkit.ErrorHandling

open azure_function_fsharp.Infrastructure.Exceptions
open azure_function_fsharp.Infrastructure.Constants
open azure_function_fsharp.Domain.CustomTypes

type HttpRequestHandler
    (
        logger: ILogger<HttpRequestHandler>,
        telemetryClient: TelemetryClient
    ) =

    member this.IsAuthorized (roles: Role list) (httpRequest: HttpRequest) =
        if httpRequest.HttpContext.User.Identity.IsAuthenticated then
            let roleClaims =
                roles
                |> List.map (fun role ->
                    match role with
                    | Role.Administrator -> ClaimValue.Administrator
                    | Role.Editor -> ClaimValue.Editor
                    | Role.Viewer -> ClaimValue.Viewer)

            // All users have the implied Viewer role
            roles |> List.contains Role.Viewer
            || httpRequest.HttpContext.User.FindAll(fun claim -> claim.Type = ClaimType.Role)
            |> Seq.exists (fun claim -> roleClaims |> List.contains claim.Value)
        else
            false

    /// <exception cref="AuthenticationException"></exception>
    member this.GetUserName(httpRequest: HttpRequest) =
        try
            "azure-function-user"
        with ex ->
            AuthenticationException (ex) |> raise

    /// <summary>Executes the computation and functions as a top level error handler</summary>
    member this.Handle (httpRequest: HttpRequest) (roles: Role list) (computation: unit -> Async<IActionResult>) =
        let computation =
            async {
                try
                    let userName = this.GetUserName httpRequest
                    
                    telemetryClient.Context.User.AuthenticatedUserId <- userName
                    
                    if httpRequest |> this.IsAuthorized roles then
                        
                        let! actionResult = computation ()

                        return actionResult
                    else
                        logger.LogDebug(
                            LogEvent.AuthorizationError,
                            "The user is not authorized to access the requested resource"
                        )

                        return StatusCodeResult(int HttpStatusCode.Forbidden) :> IActionResult
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
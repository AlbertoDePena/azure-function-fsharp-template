namespace MyFunctionApp.Infrastructure.HttpRequestHandler

open System.Net
open System.Web.Http

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.ApplicationInsights
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.Extensions.Configuration

open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Infrastructure.Exceptions
open MyFunctionApp.Infrastructure.Authentication
open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Domain.CustomTypes

type HttpRequestHandler
    (
        logger: ILogger<HttpRequestHandler>,
        configuration: IConfiguration,
        openIdConfigurationManager: IConfigurationManager<OpenIdConnectConfiguration>,
        telemetryClient: TelemetryClient
    ) =

    member this.IsAuthorized (roles: Role list) (httpRequest: HttpRequest) =
        let claimValues =
            roles
            |> List.map (fun role ->
                match role with
                | Role.Administrator -> ClaimValue.Administrator
                | Role.Editor -> ClaimValue.Editor
                | Role.Viewer -> ClaimValue.Viewer)

        httpRequest.HttpContext.User.Identity.IsAuthenticated
        && httpRequest.HttpContext.User.FindAll(fun claim -> claim.Type = ClaimType.Role)
           |> Seq.ofNull
           |> Seq.exists (fun claim -> claimValues |> List.contains claim.Value)

    /// <exception cref="AuthenticationException"></exception>
    member this.GetUserName(httpRequest: HttpRequest) =
        try
            "azure-function-user"
        with ex ->
            AuthenticationException(ex) |> raise

    /// <summary>Executes the computation and functions as a top level error handler</summary>
    member this.Handle (httpRequest: HttpRequest) (roles: Role list) (computation: unit -> Async<IActionResult>) =
        let computation =
            async {
                try
                    let! claimsPrincipal =
                        Authentication.authenticate logger configuration openIdConfigurationManager httpRequest

                    match claimsPrincipal with
                    | None -> return UnauthorizedResult() :> IActionResult
                    | Some claimsPrincipal ->

                        httpRequest.HttpContext.User <- claimsPrincipal

                        telemetryClient.Context.User.AuthenticatedUserId <- (this.GetUserName httpRequest)

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

namespace MyFunctionApp.Infrastructure.HttpRequestHandler

open System.Security.Claims
open System.Web.Http

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.ApplicationInsights

open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Exceptions
open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Infrastructure.Authentication
open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Domain.Invariants
open MyFunctionApp.Domain.User

type HttpRequestHandler
    (logger: ILogger<HttpRequestHandler>, telemetryClient: TelemetryClient, authentication: Authentication) =

    member this.Handle
        (httpRequest: HttpRequest)
        (userGroups: UserGroup list)
        (computation: UserName -> Async<IActionResult>)
        =

        /// <exception cref="AuthenticationException"></exception>
        let getUserName (claimsPrincipal: ClaimsPrincipal) =
            claimsPrincipal.TryGetClaimValue ClaimType.EmailAddress
            |> Option.defaultValue String.defaultValue
            |> UserName.TryCreate
            |> Result.valueOr (AuthenticationException >> raise)

        /// <exception cref="AuthorizationException"></exception>
        let checkAuthorization (claimsPrincipal: ClaimsPrincipal) (userGroups: UserGroup list) =
            let claimValues =
                userGroups
                |> List.map (fun userGroup ->
                    match userGroup with
                    | UserGroup.Viewer -> ClaimValue.Viewer
                    | UserGroup.PotentialDelayAdministrator -> ClaimValue.PotentialDelayAdministrator
                    | UserGroup.PotentialDelayApprover -> ClaimValue.PotentialDelayApprover)

            claimsPrincipal.FindAll(fun claim -> claim.Type = ClaimType.Role)
            |> Seq.ofNull
            |> Seq.exists (fun claim -> claimValues |> List.contains claim.Value)
            |> fun isAuthorized ->
                if not isAuthorized then
                    "The user is not authorized to access the requested resource"
                    |> AuthorizationException
                    |> raise

        let computation =
            async {
                try
                    let! claimsPrincipal = authentication.Authenticate httpRequest

                    let userName = getUserName claimsPrincipal

                    httpRequest.HttpContext.User <- claimsPrincipal

                    telemetryClient.Context.User.AuthenticatedUserId <- userName.Value

                    checkAuthorization claimsPrincipal userGroups

                    let! actionResult = computation userName

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

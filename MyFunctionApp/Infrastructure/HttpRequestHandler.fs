namespace MyFunctionApp.Infrastructure.HttpRequestHandler

open System
open System.Security.Claims
open System.Threading.Tasks
open System.Web.Http

open System.IdentityModel.Tokens.Jwt
open System.Threading

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.ApplicationInsights

open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens
open Microsoft.Extensions.Options

open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Options
open MyFunctionApp.Exceptions
open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Invariants
open MyFunctionApp.Extensions
open MyFunctionApp.User.Domain

type HttpRequestHandler
    (
        logger: ILogger<HttpRequestHandler>,
        azureAdOptions: IOptions<AzureAd>,
        openIdConfigurationManager: IConfigurationManager<OpenIdConnectConfiguration>,
        telemetryClient: TelemetryClient
    ) =

    member this.HandleAsync
        (httpRequest: HttpRequest)
        (userGroups: UserGroup list)
        (getActionResultAsync: UserName -> Task<IActionResult>)
        =

        /// <exception cref="AuthenticationException"></exception>
        let getClaimsPrincipalAsync () =
            task {
                try
                    match httpRequest.TryGetBearerToken() with
                    | None -> return failwith "The HTTP request does not have a bearer token"
                    | Some idToken ->

                        let tokenValidator = JwtSecurityTokenHandler()

                        let! openIdConfiguration =
                            openIdConfigurationManager.GetConfigurationAsync(CancellationToken.None)

                        let validationParameters =
                            TokenValidationParameters(
                                RequireSignedTokens = true,
                                ValidAudience = azureAdOptions.Value.ClientId,
                                ValidateAudience = true,
                                ValidateIssuer = true,
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKeys = openIdConfiguration.SigningKeys,
                                ValidIssuer = openIdConfiguration.Issuer
                            )

                        let mutable securityToken = Unchecked.defaultof<SecurityToken>

                        let claimsPrincipal =
                            tokenValidator.ValidateToken(idToken, validationParameters, ref securityToken)

                        if not claimsPrincipal.Identity.IsAuthenticated then
                            failwith "The user is not authenticated"

                        return claimsPrincipal
                with ex ->
                    return (AuthenticationException ex |> raise)
            }

        /// <exception cref="AuthenticationException"></exception>
        let getUserName (claimsPrincipal: ClaimsPrincipal) : UserName =
            claimsPrincipal.TryGetClaimValue ClaimType.EmailAddress
            |> Option.defaultValue String.defaultValue
            |> Text.tryCreate
            |> Option.defaultWith (fun () ->
                AuthenticationException "Email address not found in the claims principal"
                |> raise)

        /// <exception cref="AuthorizationException"></exception>
        let checkAuthorization (claimsPrincipal: ClaimsPrincipal) (userGroups: UserGroup list) =
            let claimValues =
                userGroups
                |> List.map (fun userGroup ->
                    match userGroup with
                    | UserGroup.Viewer -> ClaimValue.Viewer
                    | UserGroup.Editor -> ClaimValue.Editor
                    | UserGroup.Administrator -> ClaimValue.Administrator)

            claimsPrincipal.FindAll(fun claim -> claim.Type = ClaimType.Role)
            |> Seq.ofNull
            |> Seq.exists (fun claim -> claimValues |> List.contains claim.Value)
            |> fun isAuthorized ->
                if not isAuthorized then
                    "The user is not authorized to access the requested resource"
                    |> AuthorizationException
                    |> raise

        task {
            try
                let! claimsPrincipal = getClaimsPrincipalAsync ()

                let userName = getUserName claimsPrincipal

                httpRequest.HttpContext.User <- claimsPrincipal

                telemetryClient.Context.User.AuthenticatedUserId <- (Text.value userName)

                checkAuthorization claimsPrincipal userGroups

                let! actionResult = getActionResultAsync userName

                return actionResult
            with
            | :? AuthenticationException as ex ->
                if logger.IsEnabled LogLevel.Debug then
                    logger.LogDebug(LogEvent.AuthenticationError, ex, ex.Message)

                return UnauthorizedResult() :> IActionResult

            | :? AuthorizationException as ex ->
                if logger.IsEnabled LogLevel.Debug then
                    logger.LogDebug(LogEvent.AuthorizationError, ex, ex.Message)

                return ForbidResult() :> IActionResult

            | :? DataStorageException as ex ->
                if logger.IsEnabled LogLevel.Error then
                    logger.LogError(LogEvent.DataStorageError, ex, ex.Message)

                return InternalServerErrorResult() :> IActionResult

            | ex ->
                if logger.IsEnabled LogLevel.Error then
                    logger.LogError(LogEvent.InternalServerError, ex, ex.Message)

                return InternalServerErrorResult() :> IActionResult
        }

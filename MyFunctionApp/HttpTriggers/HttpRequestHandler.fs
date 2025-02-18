namespace MyFunctionApp.HttpTriggers.HttpRequestHandler

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
open MyFunctionApp.Infrastructure.Exceptions
open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Domain

type UserName = Text

type HttpRequestHandler
    (
        logger: ILogger<HttpRequestHandler>,
        azureAdOptions: IOptions<AzureAd>,
        openIdConfigurationManager: IConfigurationManager<OpenIdConnectConfiguration>,
        telemetryClient: TelemetryClient
    ) =

    member this.HandleAsync
        (httpRequest: HttpRequest)
        (roles: UserRole list)
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
            |> Option.bind Text.TryCreate
            |> Option.defaultWith (fun () ->
                AuthenticationException "Email address not found in the claims principal"
                |> raise)

        /// <exception cref="AuthorizationException"></exception>
        let checkAuthorization (claimsPrincipal: ClaimsPrincipal) (roles: UserRole list) =
            let roleClaimValues =
                roles
                |> List.map (fun role ->
                    match role with
                    | UserRole.Viewer -> RoleClaimValue.Viewer
                    | UserRole.Editor -> RoleClaimValue.Editor
                    | UserRole.Administrator -> RoleClaimValue.Administrator)

            claimsPrincipal.FindAll(fun claim -> claim.Type = ClaimType.Role)
            |> Seq.ofNull
            |> Seq.exists (fun claim -> roleClaimValues |> List.contains claim.Value)
            |> fun isMember ->
                if not isMember then
                    "The user is not authorized to access the requested resource"
                    |> AuthorizationException
                    |> raise

        task {
            try
                let! claimsPrincipal = getClaimsPrincipalAsync ()

                let userName = getUserName claimsPrincipal

                httpRequest.HttpContext.User <- claimsPrincipal

                telemetryClient.Context.User.AuthenticatedUserId <- userName.Value

                checkAuthorization claimsPrincipal roles

                let! actionResult = getActionResultAsync userName

                return actionResult
            with
            | :? AuthenticationException as ex ->
                logger.LogDebug(LogEvent.AuthenticationError, ex, ex.Message)

                return UnauthorizedResult() :> IActionResult

            | :? AuthorizationException as ex ->
                logger.LogDebug(LogEvent.AuthorizationError, ex, ex.Message)

                return ForbidResult() :> IActionResult

            | :? DataStorageException as ex ->
                logger.LogError(LogEvent.DataStorageError, ex, ex.Message)

                return InternalServerErrorResult() :> IActionResult

            | ex ->
                logger.LogError(LogEvent.InternalServerError, ex, ex.Message)

                return InternalServerErrorResult() :> IActionResult
        }

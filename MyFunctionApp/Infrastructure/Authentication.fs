namespace MyFunctionApp.Infrastructure.Authentication

open System.IdentityModel.Tokens.Jwt
open System.Threading

open Microsoft.AspNetCore.Http
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens
open Microsoft.Extensions.Options

open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Exceptions
open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Infrastructure.Options

type Authentication
    (azureAdOptions: IOptions<AzureAd>, openIdConfigurationManager: IConfigurationManager<OpenIdConnectConfiguration>) =

    /// <exception cref="AuthenticationException"></exception>
    member this.Authenticate(httpRequest: HttpRequest) =
        async {
            try
                match httpRequest.TryGetBearerToken() with
                | None -> return failwith "The HTTP request does not have a bearer token"
                | Some idToken ->

                    let tokenValidator = JwtSecurityTokenHandler()

                    let! openIdConfiguration =
                        openIdConfigurationManager.GetConfigurationAsync(CancellationToken.None)
                        |> Async.AwaitTask

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

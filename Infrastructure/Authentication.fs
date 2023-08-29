namespace azure_function_fsharp.Infrastructure.Authentication

open System.IdentityModel.Tokens.Jwt
open System.Threading

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens
open Microsoft.Extensions.Configuration

open FsToolkit.ErrorHandling

open azure_function_fsharp.Infrastructure.Extensions
open azure_function_fsharp.Infrastructure.Constants

[<RequireQualifiedAccess>]
module Authentication =

    let authenticate
        (logger: ILogger)
        (configuration: IConfiguration)
        (openIdConfigurationManager: IConfigurationManager<OpenIdConnectConfiguration>)
        (httpRequest: HttpRequest)
        =
        async {
            try
                match httpRequest.TryGetBearerToken() with
                | None -> return None
                | Some idToken ->

                    let tokenValidator = JwtSecurityTokenHandler()

                    let! openIdConfiguration =
                        openIdConfigurationManager.GetConfigurationAsync(CancellationToken.None)
                        |> Async.AwaitTask

                    let validationParameters =
                        TokenValidationParameters(
                            RequireSignedTokens = true,
                            ValidAudience = configuration.GetValue<string>(ConfigurationKey.AzureAd_ClientId),
                            ValidateAudience = true,
                            ValidateIssuer = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKeys = openIdConfiguration.SigningKeys,
                            ValidIssuer = openIdConfiguration.Issuer
                        )

                    let mutable securityToken = Unchecked.defaultof<SecurityToken>

                    return Some(tokenValidator.ValidateToken(idToken, validationParameters, ref securityToken))
            with ex ->
                logger.LogError(LogEvent.AuthenticationError, ex, ex.Message)

                return None
        }

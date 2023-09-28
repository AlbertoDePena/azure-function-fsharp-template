namespace MyFunctionApp.Infrastructure.Authorization

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open FsToolkit.ErrorHandling

open MyFunctionApp.Domain.ConstraintTypes
open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Infrastructure.Constants

type Authorization(logger: ILogger<Authorization>) =

    member this.IsAuthorized(httpRequest: HttpRequest, roles: Role list) =
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

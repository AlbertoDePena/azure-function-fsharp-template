namespace MyFunctionApp.Infrastructure.Extensions

[<AutoOpen>]
module ClaimsPrincipalExtensions =
    open System
    open System.Security.Claims

    type ClaimsPrincipal with

        member this.TryGetClaimValue(name: string) =
            if this.Identity.IsAuthenticated then
                this.FindFirst(fun claim -> claim.Type = name)
                |> Option.ofObj
                |> Option.map (fun claim -> claim.Value)
                |> Option.filter (String.IsNullOrWhiteSpace >> not)
            else
                None

namespace MyFunctionApp.Infrastructure.Extensions

open System
open System.Security.Claims

[<AutoOpen>]
module ClaimsPrincipalExtensions =
    
    type ClaimsPrincipal with

        member this.TryGetClaimValue(name: string) =
            if this.Identity.IsAuthenticated then
                this.FindFirst(fun claim -> claim.Type = name)
                |> Option.ofObj
                |> Option.map (fun claim -> claim.Value)
                |> Option.filter (String.IsNullOrWhiteSpace >> not)
            else
                None

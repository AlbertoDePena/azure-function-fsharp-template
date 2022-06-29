namespace azure_function_fsharp

[<AutoOpen>]
module ClaimsPrincipalExtensions =
    open System.Security.Claims

    type ClaimsPrincipal with

        member this.TryGetClaimValue(name: string) =
            if this.Identity.IsAuthenticated then
                this.FindFirst(fun claim -> claim.Type = name)
                |> Option.ofObj
                |> Option.map (fun claim -> claim.Value)
            else
                None

[<AutoOpen>]
module HttpRequestExtensions =
    open System
    open System.IO
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Configuration
    open Newtonsoft.Json
    open FsToolkit.ErrorHandling

    type HttpRequest with

        member this.IsLocalEnvironment() =
            this
                .HttpContext
                .RequestServices
                .GetRequiredService<IConfiguration>()
                .GetValue<string>("ASPNETCORE_ENVIRONMENT") = "Local"

        member this.TryGetBearerToken() =
            this.Headers
            |> Seq.tryFind (fun q -> q.Key = "Authorization")
            |> Option.bind (fun q ->
                if Seq.isEmpty q.Value then
                    None
                else
                    q.Value |> Seq.tryHead)
            |> Option.filter (fun h -> h.Contains("Bearer"))
            |> Option.map (fun h -> h.Substring("Bearer ".Length).Trim())

        member this.TryGetQueryStringValue(name: string) =
            let hasValue, values = this.Query.TryGetValue(name)
            if hasValue then values |> Seq.tryHead else None

        member this.TryGetHeaderValue(name: string) =
            match this.Headers.TryGetValue name with
            | true, value -> value.ToString() |> Some
            | _ -> None

        member this.TryGetFormValue(key: string) =
            match this.HasFormContentType with
            | false -> None
            | true ->
                match this.Form.TryGetValue key with
                | true, value -> value.ToString() |> Some
                | false, _ -> None
                
        member this.ReadJsonAsAsync<'a>() =
            use reader = new StreamReader(this.Body)
            reader.ReadToEndAsync()            
            |> Async.AwaitTask
            |> Async.map (JsonConvert.DeserializeObject<'a>)

        member this.ReadFormAsJson() =
            let canonicalizeValue (value: string) =
                if String.IsNullOrWhiteSpace value then null
                elif value = "null" then null
                else value

            this.Form
            |> Seq.map (fun item ->
                (item.Key,
                 item.Value
                 |> Seq.tryHead
                 |> Option.map canonicalizeValue
                 |> Option.defaultValue null))
            |> dict
            |> JsonConvert.SerializeObject

        member this.ReadFormAs<'a>() =
            this.ReadFormAsJson() |> JsonConvert.DeserializeObject<'a>

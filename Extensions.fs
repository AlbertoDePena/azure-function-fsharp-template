namespace azure_function_fsharp.Extensions

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

[<AutoOpen>]
module HttpRequestExtensions =    
    open System
    open System.IO
    open Microsoft.AspNetCore.Http
    open Newtonsoft.Json
    open FsToolkit.ErrorHandling

    type HttpRequest with

        member this.TryGetBearerToken() =
            this.Headers
            |> Seq.tryFind (fun q -> q.Key = "Authorization")
            |> Option.bind (fun q ->
                if Seq.isEmpty q.Value then
                    None
                else
                    q.Value |> Seq.tryHead)
            |> Option.filter (fun h -> h.Contains("Bearer "))
            |> Option.map (fun h -> h.Substring("Bearer ".Length).Trim())

        member this.TryGetQueryStringValue(key: string) =
            let hasValue, values = this.Query.TryGetValue(key)
            if hasValue then 
                values |> Seq.tryHead |> Option.filter (String.IsNullOrWhiteSpace >> not)
            else None

        member this.TryGetHeaderValue(key: string) =            
            let hasValue, values = this.Headers.TryGetValue(key)
            if hasValue then 
                values |> Seq.tryHead |> Option.filter (String.IsNullOrWhiteSpace >> not)
            else None

        member this.TryGetFormValue(key: string) =
            match this.HasFormContentType with
            | false -> None
            | true ->
                let hasValue, values = this.Form.TryGetValue(key)
                if hasValue then 
                    values |> Seq.tryHead |> Option.filter (String.IsNullOrWhiteSpace >> not)
                else None
                
        member this.TryReadJsonAsAsync<'a>() =
            use reader = new StreamReader(this.Body)
            reader.ReadToEndAsync()            
            |> Async.AwaitTask
            |> Async.map (JsonConvert.DeserializeObject<'a> >> Option.ofNull)


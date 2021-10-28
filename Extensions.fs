namespace azure_function_fsharp

[<AutoOpen>]
module HttpRequestExtensions =
    open System
    open Microsoft.AspNetCore.Http
    open Newtonsoft.Json

    type HttpRequest with

        member this.TryGetBearerToken() =
            this.Headers
            |> Seq.tryFind (fun q -> q.Key = "Authorization")
            |> Option.map (fun q -> if Seq.isEmpty q.Value then String.Empty else q.Value |> Seq.head)
            |> Option.map (fun h -> h.Substring("Bearer ".Length).Trim())

        member this.TryGetQueryStringValue(name: string) =
            let hasValue, values = this.Query.TryGetValue(name)
            if hasValue then values |> Seq.tryHead else None

        member this.TryGetHeaderValue(name: string) =
            let hasHeader, values = this.Headers.TryGetValue(name)
            if hasHeader then values |> Seq.tryHead else None

        member this.TryGetFormValue(key: string) =
            match this.HasFormContentType with
            | false -> None
            | true ->
                match this.Form.TryGetValue key with
                | true, value -> Some(value.ToString())
                | false, _ -> None

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

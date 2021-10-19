namespace azure_function_fsharp

[<AutoOpen>]
module HttpRequestExtensions =
    open Microsoft.AspNetCore.Http
    open Newtonsoft.Json

    type HttpRequest with

        member this.TryGetFormValue(key: string) =
            match this.HasFormContentType with
            | false -> None
            | true ->
                match this.Form.TryGetValue key with
                | true, value -> Some(value.ToString())
                | false, _ -> None

        member this.ReadFormAsJson() =
            this.Form
            |> Seq.map (fun item -> (item.Key, item.Value.ToString()))
            |> dict
            |> JsonConvert.SerializeObject

        member this.ReadFormAs<'a>() =
            this.ReadFormAsJson() |> JsonConvert.DeserializeObject<'a>
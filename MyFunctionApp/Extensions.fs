namespace MyFunctionApp.Extensions

[<RequireQualifiedAccess>]
module Array =

    /// Convert a potentially null value to an empty array.
    let ofNull (items: 'T array) =
        if isNull items then Array.empty<'T> else items

[<RequireQualifiedAccess>]
module List =

    /// Filter out strings with null/white-space and discard duplicate entries.
    let canonicalizeStrings items =
        items |> List.filter (System.String.IsNullOrWhiteSpace >> not) |> List.distinct

[<RequireQualifiedAccess>]
module Seq =

    /// Convert a potentially null value to an empty sequence.
    let ofNull (items: 'T seq) =
        if isNull items then Seq.empty<'T> else items

[<RequireQualifiedAccess>]
module String =

    let capitalize (value: string) =
        let character = value.[0]
        let letter = character.ToString().ToUpper()
        letter + value.Substring(1)

    let defaultValue = null

    let toQueryString (query: (string * string) list) =
        System.String.Join("&", query |> List.map (fun (key, value) -> $"{key}={value}"))

[<RequireQualifiedAccess>]
module Task =
    open System.Threading.Tasks
    open FsToolkit.ErrorHandling

    [<RequireQualifiedAccess>]
    type Status<'a> =
        | Completed of 'a
        | Failed

    let handleException (handler: exn -> unit) (task: Task<_>) =
        task
        |> Task.catch
        |> Task.map (fun choice ->
            match choice with
            | Choice1Of2 x -> Status.Completed x
            | Choice2Of2 ex ->
                handler ex
                Status.Failed)

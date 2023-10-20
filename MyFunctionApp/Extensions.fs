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

    /// The default value of a primitive string is null
    let defaultValue = null

    let toQueryString (query: (string * string) list) =
        System.String.Join("&", query |> List.map (fun (key, value) -> $"{key}={value}"))

[<AutoOpen>]
module SqlDataReaderExtensions =
    open System.Threading.Tasks
    open Microsoft.Data.SqlClient

    type SqlDataReader with

        /// Map all records with the provided function available in the result set
        member this.ReadAllAsync<'T>(mapper: SqlDataReader -> 'T) : Task<'T seq> =
            task {
                let items = ResizeArray<'T>()

                let mutable keepGoing = false
                let! hasMoreItems = this.ReadAsync()
                keepGoing <- hasMoreItems

                while keepGoing do
                    items.Add(mapper this)
                    let! hasMoreItems = this.ReadAsync()
                    keepGoing <- hasMoreItems

                return items.ToArray() |> Seq.ofArray
            }

        /// Map the first record with the provided function available in the result set
        member this.ReadFirstOrAsync<'T>(mapper: SqlDataReader -> 'T, defaultValue: 'T) : Task<'T> =
            task {
                let! hasMoreItems = this.ReadAsync()

                let item = if hasMoreItems then mapper this else defaultValue

                return item
            }

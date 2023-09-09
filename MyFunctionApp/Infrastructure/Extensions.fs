namespace MyFunctionApp.Infrastructure.Extensions

[<RequireQualifiedAccess>]
module Array =

    /// Convert a potentially null value to an empty array.
    let ofNull (items: 'T array) =
        if isNull items then Array.empty<'T> else items

[<RequireQualifiedAccess>]
module Seq =

    /// Convert a potentially null value to an empty sequence.
    let ofNull (items: 'T seq) =
        if isNull items then Seq.empty<'T> else items

[<RequireQualifiedAccess>]
module String =

    let defaultValue = null
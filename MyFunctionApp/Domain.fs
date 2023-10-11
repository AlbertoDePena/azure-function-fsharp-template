namespace MyFunctionApp.Domain

open System
open MyFunctionApp.Invariants

[<RequireQualifiedAccess>]
type SortDirection =
    | Ascending
    | Descending

    member this.Value =
        match this with
        | Ascending -> "Ascending"
        | Descending -> "Descending"

    static member FromString(value: string) =
        match value with
        | "Ascending" -> Some Ascending
        | "Descending" -> Some Descending        
        | _ -> None

type Query =
    { SearchCriteria: Text256 option
      ActiveOnly: bool
      Page: PositiveNumber
      PageSize: PositiveNumber
      SortBy: PropertyName option
      SortDirection: SortDirection option }

type PagedData<'a> =
    { Page: PositiveNumber
      PageSize: PositiveNumber
      TotalCount: PositiveNumber
      SortBy: PropertyName option
      SortDirection: SortDirection option
      Data: 'a list }
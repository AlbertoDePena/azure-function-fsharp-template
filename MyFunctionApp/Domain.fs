namespace MyFunctionApp.Domain

open MyFunctionApp.Invariants

[<RequireQualifiedAccess>]
type SortDirection =
    | Ascending
    | Descending

    member this.Value =
        match this with
        | SortDirection.Ascending -> "Ascending"
        | SortDirection.Descending -> "Descending"

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
        match value with
        | "Ascending" -> Some SortDirection.Ascending
        | "Descending" -> Some SortDirection.Descending
        | _ -> None

type Query =
    { SearchCriteria: Text option
      ActiveOnly: bool
      Page: Number
      PageSize: Number
      SortBy: Text option
      SortDirection: SortDirection option }

type PagedData<'T> =
    { Page: Number
      PageSize: Number
      TotalCount: Number
      SortBy: Text option
      SortDirection: SortDirection option
      Data: 'T list }

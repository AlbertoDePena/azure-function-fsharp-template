namespace MyFunctionApp.Domain

open System
open FsToolkit.ErrorHandling
open MyFunctionApp.Invariants

type StorageException = Exception

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
      SortBy: Text256 option
      SortDirection: SortDirection option }

type PagedData<'a> =
    { Page: PositiveNumber
      PageSize: PositiveNumber
      TotalCount: WholeNumber
      SortBy: Text256 option
      SortDirection: SortDirection option
      Data: 'a list }

    member this.CalculateNumberOfPages() =
        let pageCount = this.TotalCount.Value / this.PageSize.Value

        let integer =            
            if (this.TotalCount.Value % this.PageSize.Value) = 0 then
                pageCount
            else
                pageCount + 1

        integer |> WholeNumber.TryCreate |> Result.defaultValue WholeNumber.DefaultValue

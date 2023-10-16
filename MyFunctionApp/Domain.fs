namespace MyFunctionApp.Domain

open FsToolkit.ErrorHandling
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
        let pageCount =
            WholeNumber.value this.TotalCount / PositiveNumber.value this.PageSize

        let integer =
            if (WholeNumber.value this.TotalCount % PositiveNumber.value this.PageSize) = 0 then
                pageCount
            else
                pageCount + 1

        integer
        |> WholeNumber.tryCreate "Number of pages"
        |> Result.defaultValue WholeNumber.defaultValue

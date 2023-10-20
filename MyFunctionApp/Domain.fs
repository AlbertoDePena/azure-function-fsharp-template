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
    { SearchCriteria: Text option
      ActiveOnly: bool
      Page: PositiveNumber
      PageSize: PositiveNumber
      SortBy: Text option
      SortDirection: SortDirection option }

type PagedData<'a> =
    { Page: PositiveNumber
      PageSize: PositiveNumber
      TotalCount: WholeNumber
      SortBy: Text option
      SortDirection: SortDirection option
      Data: 'a list }

[<RequireQualifiedAccess>]
module PagedData =

    let calculateNumberOfPages (pagedData: PagedData<'a>) =
        let pageCount =
            WholeNumber.value pagedData.TotalCount / PositiveNumber.value pagedData.PageSize

        let integer =
            if (WholeNumber.value pagedData.TotalCount % PositiveNumber.value pagedData.PageSize) = 0 then
                pageCount
            else
                pageCount + 1

        integer
        |> WholeNumber.tryCreate
        |> Option.defaultValue WholeNumber.defaultValue

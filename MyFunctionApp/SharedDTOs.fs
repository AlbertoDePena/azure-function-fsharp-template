namespace MyFunctionApp.Shared.DTOs

[<CLIMutable>]
type ApiMessageResponse = { Messages: string array }

[<RequireQualifiedAccess>]
module ApiMessageResponse =

    let fromMessages (messages: string list) = { Messages = List.toArray messages }

    let fromMessage (message: string) = List.singleton message |> fromMessages

[<NoComparison>]
[<CLIMutable>]
type PagedDataResponse<'a> =
    { Page: int
      PageSize: int
      TotalCount: int
      NumberOfPages: int
      SortBy: string
      SortDirection: string
      Data: 'a array }

[<RequireQualifiedAccess>]
module PagedDataResponse =
    open FsToolkit.ErrorHandling
    open MyFunctionApp.Extensions
    open MyFunctionApp.Invariants
    open MyFunctionApp.Domain

    let fromDomain mapping (source: PagedData<'a>) : PagedDataResponse<'b> =

        let pageCount = source.TotalCount / source.PageSize

        let numberOfPages =
            if (source.TotalCount % source.PageSize) = 0 then
                pageCount
            else
                pageCount + 1

        { PageSize = source.PageSize
          Page = source.Page
          TotalCount = source.TotalCount
          NumberOfPages = numberOfPages
          SortBy =
            source.SortBy
            |> Option.map (fun x -> x.Value)
            |> Option.defaultValue String.defaultValue
          SortDirection =
            source.SortDirection
            |> Option.map Type.toString
            |> Option.defaultValue String.defaultValue
          Data = source.Data |> List.map mapping |> Array.ofList }

[<CLIMutable>]
type QueryRequest =
    { SearchCriteria: string
      ActiveOnly: bool
      Page: int
      PageSize: int
      SortBy: string
      SortDirection: string }

[<RequireQualifiedAccess>]
module QueryRequest =
    open FsToolkit.ErrorHandling
    open MyFunctionApp.Invariants
    open MyFunctionApp.Domain

    let toDomain (query: QueryRequest) : Validation<Query, string> =
        validation {
            let! _ = query.Page > 0 |> Result.requireTrue "Page is required"

            and! _ = query.PageSize > 0 |> Result.requireTrue "Page size is required"

            return
                { SearchCriteria = query.SearchCriteria |> Text.TryCreate
                  ActiveOnly = query.ActiveOnly
                  Page = query.Page
                  PageSize = query.PageSize
                  SortBy = query.SortBy |> Text.TryCreate
                  SortDirection = query.SortDirection |> SortDirection.TryCreate }
        }

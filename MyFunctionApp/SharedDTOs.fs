namespace MyFunctionApp.Shared.DTOs

[<CLIMutable>]
type ApiMessageResponse = { Messages: string array }

[<RequireQualifiedAccess>]
module ApiMessageResponse =

    let fromMessages (messages: string array) = { Messages = messages }

    let fromMessage (message: string) = [| message |] |> fromMessages

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
            let! searchCriteria = query.SearchCriteria |> Text256.TryCreateOption
            and! page = query.Page |> PositiveNumber.TryCreate
            and! pageSize = query.PageSize |> PositiveNumber.TryCreate
            and! sortBy = query.SortBy |> Text256.TryCreateOption

            let sortDirection = query.SortDirection |> SortDirection.FromString

            return
                { SearchCriteria = searchCriteria
                  ActiveOnly = query.ActiveOnly
                  Page = page
                  PageSize = pageSize
                  SortBy = sortBy
                  SortDirection = sortDirection }
        }

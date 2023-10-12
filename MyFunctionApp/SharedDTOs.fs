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
    open MyFunctionApp.Domain

    let fromDomain mapping (source: PagedData<'a>) : PagedDataResponse<'b> =
        { PageSize = source.PageSize.Value
          Page = source.Page.Value
          TotalCount = source.TotalCount.Value
          NumberOfPages = source.CalculateNumberOfPages().Value
          SortBy =
            source.SortBy
            |> Option.map (fun x -> x.Value)
            |> Option.defaultValue String.defaultValue
          SortDirection =
            source.SortDirection
            |> Option.map (fun x -> x.Value)
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

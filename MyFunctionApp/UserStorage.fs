namespace MyFunctionApp.User.Storage

open System.Data

open Microsoft.Data.SqlClient
open Dapper
open FsToolkit.ErrorHandling

open MyFunctionApp.Invariants
open MyFunctionApp.Domain
open MyFunctionApp.User.Domain

[<RequireQualifiedAccess>]
module UserStorage =

    let search (dbConnectionString: DbConnectionString) (query: Query) : Async<PagedData<User>> =
        async {

            use connection = new SqlConnection(dbConnectionString.Value)

            let! gridReader =
                connection.QueryMultipleAsync(
                    "dbo.Users_Search",
                    param =
                        {| SearchCriteria = query.SearchCriteria
                           ActiveOnly = query.ActiveOnly
                           Page = query.Page
                           PageSize = query.PageSize
                           SortBy = query.SortBy
                           SortDirection = query.SortDirection |},
                    commandType = CommandType.StoredProcedure
                )
                |> Async.AwaitTask

            let! items = gridReader.ReadAsync<User>() |> Async.AwaitTask |> Async.map Seq.toList

            let! totalCount =
                gridReader.ReadFirstOrDefaultAsync<int>()
                |> Async.AwaitTask
                |> Async.map (
                    Option.ofNull
                    >> Option.defaultValue 0
                    >> WholeNumber.TryCreate
                    >> Result.defaultValue WholeNumber.DefaultValue
                )

            return
                { Page = query.Page
                  PageSize = query.PageSize
                  TotalCount = totalCount
                  SortBy = query.SortBy
                  SortDirection = query.SortDirection
                  Data = items }
        }

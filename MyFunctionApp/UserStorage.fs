namespace MyFunctionApp.User.Storage

open System.Data
open System.Threading.Tasks

open Microsoft.Data.SqlClient
open Dapper
open FsToolkit.ErrorHandling

open MyFunctionApp.Invariants
open MyFunctionApp.Domain
open MyFunctionApp.User.Domain

[<RequireQualifiedAccess>]
module UserStorage =

    let getPagedData (dbConnectionString: DbConnectionString) (query: Query) : Task<PagedData<User>> =
        task {
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

            let! items = gridReader.ReadAsync<User>() |> Task.map Seq.toList

            let! totalCount =
                gridReader.ReadFirstOrDefaultAsync<int>()
                |> Task.map (
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

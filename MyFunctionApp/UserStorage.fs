namespace MyFunctionApp.User.Storage

open System.Data
open System.Threading.Tasks

open Microsoft.Data.SqlClient
open Dapper
open FsToolkit.ErrorHandling

open MyFunctionApp.Exceptions
open MyFunctionApp.Invariants
open MyFunctionApp.Domain
open MyFunctionApp.User.Domain

[<RequireQualifiedAccess>]
module UserStorage =
    
    type DbConnectionString = Text

    /// <exception cref="DataStorageException"></exception>
    let getPagedData (dbConnectionString: DbConnectionString) (query: Query) : Task<PagedData<User>> =
        task {
            try
                use connection = new SqlConnection(Text.value dbConnectionString)

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

                let! users = gridReader.ReadAsync<User>() |> Task.map Seq.toList

                let! totalCount =
                    gridReader.ReadFirstOrDefaultAsync<int>()
                    |> Task.map (
                        Option.ofNull
                        >> Option.defaultValue 0
                        >> WholeNumber.tryCreate
                        >> Option.defaultValue WholeNumber.defaultValue
                    )

                return
                    { Page = query.Page
                      PageSize = query.PageSize
                      TotalCount = totalCount
                      SortBy = query.SortBy
                      SortDirection = query.SortDirection
                      Data = users }
            with ex ->
                return (DataStorageException ex |> raise)
        }

    /// <exception cref="DataStorageException"></exception>
    let tryFindByEmailAddress
        (dbConnectionString: DbConnectionString)
        (emailAddress: EmailAddress)
        : Task<UserDetails option> =
        task {
            try
                use connection = new SqlConnection(Text.value dbConnectionString)

                let! gridReader =
                    connection.QueryMultipleAsync(
                        "dbo.Users_FindByEmailAddress",
                        param = {| EmailAddress = EmailAddress.value emailAddress |},
                        commandType = CommandType.StoredProcedure
                    )

                let! userDetails = gridReader.ReadFirstOrDefaultAsync<UserDetails>() |> Task.map Option.ofNull

                let! permissions = gridReader.ReadAsync<UserPermission>() |> Task.map Seq.toList

                let! groups = gridReader.ReadAsync<UserGroup>() |> Task.map Seq.toList

                return
                    userDetails
                    |> Option.map (fun userDetails ->
                        { userDetails with
                            Permissions = permissions
                            Groups = groups })
            with ex ->
                return (DataStorageException ex |> raise)
        }

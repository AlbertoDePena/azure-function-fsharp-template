namespace MyFunctionApp.Infrastructure.UserRepository

open System
open System.Data
open Microsoft.Data.SqlClient

open Dapper
open FsToolkit.ErrorHandling

open MyFunctionApp.Invariants
open MyFunctionApp.User.Domain
open MyFunctionApp.Infrastructure.Exceptions

[<NoComparison>]
[<CLIMutable>]
type UserProjection =
    { Id: Guid
      EmailAddress: string
      DisplayName: string
      Type: string
      Permissions: string seq 
      Groups: string seq }

    static member Sql =
        @"SELECT 
            Users.Id, 
            Users.EmailAddress, 
            Users.DisplayName, 
            UserTypes.[Name] AS [Type]
        FROM dbo.Users 
        INNER JOIN dbo.UserTypes
            ON Users.TypeId = UserTypes.Id
        WHERE Users.EmailAddress = @EmailAddress"

[<RequireQualifiedAccess>]
module UserRepository =

    /// <exception cref="DataAccessException"></exception>
    let tryFindByEmailAddress
        (dbConnectionString: DbConnectionString)
        (emailAddress: EmailAddress)
        : Async<UserProjection option> =
        async {
            try
                use connection = new SqlConnection(dbConnectionString.Value)

                let! result =
                    connection.QueryFirstOrDefaultAsync<UserProjection>(
                        UserProjection.Sql,
                        param = {| EmailAddress = emailAddress.Value |},
                        commandType = CommandType.Text
                    )
                    |> Async.AwaitTask
                    |> Async.map Option.ofNull

                return result
            with ex ->
                return (DataAccessException ex |> raise)
        }

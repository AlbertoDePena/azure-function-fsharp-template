namespace MyFunctionApp.Infrastructure.Database

open System.Data
open System.Threading.Tasks

open Microsoft.Data.SqlClient
open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Exceptions
open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Infrastructure.Options
open MyFunctionApp.Domain
open Microsoft.Extensions.Options

type UserDatabase(databaseOptions: IOptions<Database>) =

    let readUserRole (reader: SqlDataReader) : UserRole =
        reader.GetOrdinal("RoleName")
        |> reader.GetString
        |> UserRole.TryCreate
        |> Option.defaultWith (fun () -> failwith "Missing RoleName column")

    let readUserPermission (reader: SqlDataReader) : UserPermission =
        reader.GetOrdinal("PermissionName")
        |> reader.GetString
        |> UserPermission.TryCreate
        |> Option.defaultWith (fun () -> failwith "Missing PermissionName column")

    let readUser (reader: SqlDataReader) : User =
        { Id =
            reader.GetOrdinal("Id")
            |> reader.GetGuid
            |> UniqueId.TryCreate
            |> Option.defaultWith (fun () -> failwith "Missing Id column")
          EmailAddress =
            reader.GetOrdinal("EmailAddress")
            |> reader.GetString
            |> EmailAddress.TryCreate
            |> Option.defaultWith (fun () -> failwith "Missing EmailAddress column")
          DisplayName =
            reader.GetOrdinal("DisplayName")
            |> reader.GetString
            |> Text.TryCreate
            |> Option.defaultWith (fun () -> failwith "Missing DisplayName column")
          Type =
            reader.GetOrdinal("Type")
            |> reader.GetString
            |> UserType.TryCreate
            |> Option.defaultWith (fun () -> failwith "Missing Type column") }

    /// <exception cref="DataStorageException"></exception>
    member this.GetPagedData(query: Query) : Task<PagedData<User>> =
        task {
            try
                use connection = new SqlConnection(databaseOptions.Value.ConnectionString)
                use command = new SqlCommand("dbo.Users_Search", connection)

                command.CommandType <- CommandType.StoredProcedure

                command.Parameters.AddWithValue(
                    "@SearchCriteria",
                    query.SearchCriteria
                    |> Option.map (fun x -> x.Value)
                    |> Option.defaultValue String.defaultValue
                )
                |> ignore

                command.Parameters.AddWithValue("@ActiveOnly", query.ActiveOnly) |> ignore

                command.Parameters.AddWithValue("@Page", query.Page) |> ignore

                command.Parameters.AddWithValue("@PageSize", query.PageSize) |> ignore

                command.Parameters.AddWithValue(
                    "@SortBy",
                    query.SortBy
                    |> Option.map (fun x -> x.Value)
                    |> Option.defaultValue String.defaultValue
                )
                |> ignore

                command.Parameters.AddWithValue(
                    "@SortDirection",
                    query.SortDirection
                    |> Option.map (fun x -> x.Value)
                    |> Option.defaultValue String.defaultValue
                )
                |> ignore

                do! connection.OpenAsync()

                use! reader = command.ExecuteReaderAsync()

                let! users = reader.ReadAllAsync readUser

                let! hasNextResult = reader.NextResultAsync()

                let totalCount = if hasNextResult then reader.GetInt32(0) else 0

                return
                    { Page = query.Page
                      PageSize = query.PageSize
                      TotalCount = totalCount
                      SortBy = query.SortBy
                      SortDirection = query.SortDirection
                      Data = users |> Seq.toList }
            with ex ->
                return (DataStorageException ex |> raise)
        }

    /// <exception cref="DataStorageException"></exception>
    member this.TryFindByEmailAddress(emailAddress: EmailAddress) : Task<UserDetails option> =
        task {
            try
                use connection = new SqlConnection(databaseOptions.Value.ConnectionString)
                use command = new SqlCommand("dbo.Users_FindByEmailAddress", connection)

                command.CommandType <- CommandType.StoredProcedure

                command.Parameters.AddWithValue("@EmailAddress", emailAddress.Value) |> ignore

                do! connection.OpenAsync()

                use! reader = command.ExecuteReaderAsync()

                let! users = reader.ReadAllAsync readUser

                let! hasNextResult = reader.NextResultAsync()

                let! userPermissions =
                    if hasNextResult then
                        reader.ReadAllAsync readUserPermission
                    else
                        Task.singleton []

                let! hasNextResult = reader.NextResultAsync()

                let! userRoles =
                    if hasNextResult then
                        reader.ReadAllAsync readUserRole
                    else
                        Task.singleton []

                let userDetailsOption =
                    users
                    |> Seq.tryHead
                    |> Option.map (fun user ->
                        { User = user
                          Permissions = userPermissions |> Seq.toList
                          Roles = userRoles |> Seq.toList })

                return userDetailsOption
            with ex ->
                return (DataStorageException ex |> raise)
        }

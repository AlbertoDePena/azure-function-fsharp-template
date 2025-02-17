namespace MyFunctionApp.User.Storage

open System.Data
open System.Threading.Tasks

open Microsoft.Data.SqlClient
open FsToolkit.ErrorHandling

open MyFunctionApp.Exceptions
open MyFunctionApp.Extensions
open MyFunctionApp.Invariants
open MyFunctionApp.Domain
open MyFunctionApp.User.Domain

[<RequireQualifiedAccess>]
module UserStorage =

    type DbConnectionString = Text

    let private readUserGroup (reader: SqlDataReader) : UserGroup =
        reader.GetOrdinal("GroupName")
        |> reader.GetString
        |> UserGroup.TryCreate
        |> Option.defaultWith (fun () -> failwith "Missing GroupName column")

    let private readUserPermission (reader: SqlDataReader) : UserPermission =
        reader.GetOrdinal("PermissionName")
        |> reader.GetString
        |> UserPermission.TryCreate
        |> Option.defaultWith (fun () -> failwith "Missing PermissionName column")

    let private readUser (reader: SqlDataReader) : User =
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
    let getPagedData (dbConnectionString: DbConnectionString) (query: Query) : Task<PagedData<User>> =
        task {
            try
                use connection = new SqlConnection(dbConnectionString.Value)
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
                    |> Option.map Type.toString
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
    let tryFindByEmailAddress
        (dbConnectionString: DbConnectionString)
        (emailAddress: EmailAddress)
        : Task<UserDetails option> =
        task {
            try
                use connection = new SqlConnection(dbConnectionString.Value)
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

                let! userGroups =
                    if hasNextResult then
                        reader.ReadAllAsync readUserGroup
                    else
                        Task.singleton []

                let userDetailsOption =
                    users
                    |> Seq.tryHead
                    |> Option.map (fun user ->
                        { User = user
                          Permissions = userPermissions |> Seq.toList
                          Groups = userGroups |> Seq.toList })

                return userDetailsOption
            with ex ->
                return (DataStorageException ex |> raise)
        }

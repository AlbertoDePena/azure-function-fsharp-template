namespace azure_function_fsharp.Infrastructure.DbConnection

[<RequireQualifiedAccess>]
module DbConnection =
    open System
    open System.Data
    open Microsoft.Data.SqlClient

    /// <exception cref="System.ArgumentException"></exception>
    let create (dbConnectionString: string) =
        if String.IsNullOrWhiteSpace dbConnectionString then
            invalidArg "dbConnectionString" "Database connection string cannot be null or empty"
        else
            new SqlConnection(dbConnectionString) :> IDbConnection
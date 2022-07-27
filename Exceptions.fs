namespace azure_function_fsharp.Exceptions

open System

/// An exception wrapper for any authentication operation to make authentication exception handling implementation agnostic
type AuthenticationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthenticationException(Exception message)

/// An exception wrapper for any data retrieval operation to make data exception handling implementation agnostic
type DataAccessException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = DataAccessException(Exception message)

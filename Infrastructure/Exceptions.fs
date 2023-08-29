namespace MyFunctionApp.Infrastructure.Exceptions

open System

type AuthorizationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthorizationException(Exception message)

type AuthenticationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthenticationException(Exception message)

type DataAccessException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = DataAccessException(Exception message)

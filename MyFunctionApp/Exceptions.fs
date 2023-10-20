﻿namespace MyFunctionApp.Exceptions

open System

type AuthorizationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthorizationException(Exception message)

type AuthenticationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthenticationException(Exception message)

type DataStorageException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = DataStorageException(Exception message)

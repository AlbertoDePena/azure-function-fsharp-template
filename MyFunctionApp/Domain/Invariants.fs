namespace MyFunctionApp.Domain.Invariants

open System

[<RequireQualifiedAccess>]
module private ConstraintTypes =

    /// Create a constrained string using the constructor provided.
    /// Return Error if input is null/empty or length > maxLength.
    let createString fieldName ctor maxLength value =
        if String.IsNullOrEmpty(value) then
            Error(sprintf "%s is required and cannot empty" fieldName)
        elif value.Length > maxLength then
            Error(sprintf "%s must not be more than %i characters" fieldName maxLength)
        else
            Ok(ctor value)

    /// Create a optional constrained string using the constructor provided.
    /// Return Ok None if input is null/empty.
    /// Return Error if length > maxLength.
    /// Return Ok Some if the input is valid.
    let createStringOption fieldName ctor maxLength value =
        if String.IsNullOrEmpty(value) then
            Ok None
        elif value.Length > maxLength then
            Error(sprintf "%s must not be more than %i characters" fieldName maxLength)
        else
            Ok(ctor value |> Some)

    /// Create a constrained string using the constructor provided.
    /// Return Error if input is null/empty or does not match the regular expression pattern.
    let createStringLike fieldName ctor pattern value =
        if String.IsNullOrEmpty(value) then
            Error(sprintf "%s is required and cannot be empty" fieldName)
        elif System.Text.RegularExpressions.Regex.IsMatch(value, pattern) then
            Ok(ctor value)
        else
            Error(sprintf "%s: '%s' must match the pattern '%s'" fieldName value pattern)

    /// Create a constrained integer using the constructor provided.
    /// Return Error if the input is less than minValue or more than maxValue.
    let createInteger fieldName ctor minValue maxValue value =
        if value < minValue then
            Error(sprintf "%s: must not be less than %i" fieldName minValue)
        elif value > maxValue then
            Error(sprintf "%s: must not be greater than %i" fieldName maxValue)
        else
            Ok(ctor value)

    /// Create a constrained decimal using the constructor provided.
    /// Return Error if the input is less than minValue or more than maxValue.
    let createDecimal fieldName ctor minValue maxValue value =
        if value < minValue then
            Error(sprintf "%s: must not be less than %M" fieldName minValue)
        elif value > maxValue then
            Error(sprintf "%s: must not be greater than %M" fieldName maxValue)
        else
            Ok(ctor value)

type DbConnectionString =
    private
    | DbConnectionString of string

    member this.Value =
        match this with
        | DbConnectionString value -> value

    static member TryCreate(value: string) =
        ConstraintTypes.createString "Database connection string" DbConnectionString 256 value

type DisplayName =
    private
    | DisplayName of string

    member this.Value =
        match this with
        | DisplayName value -> value

    static member TryCreate(value: string) =
        ConstraintTypes.createString "Display Name" DisplayName 256 value

type EmailAddress =
    private
    | EmailAddress of string

    member this.Value =
        match this with
        | EmailAddress value -> value

    static member TryCreate(value: string) =
        ConstraintTypes.createStringLike "Email Address" EmailAddress @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$" value

type UniqueId =
    private
    | UniqueId of Guid

    member this.Value =
        match this with
        | UniqueId value -> value

    static member TryCreate(value: Guid) =
        if value <> Guid.Empty then
            value |> UniqueId |> Ok
        else
            Error(sprintf "%O is not a valid unique identifier" value)

    static member Create() =
        RT.Comb.Provider.Sql.Create() |> UniqueId

type UserName =
    private
    | UserName of string

    member this.Value =
        match this with
        | UserName value -> value

    static member TryCreate(value: string) =
        ConstraintTypes.createString "User Name" UserName 256 value


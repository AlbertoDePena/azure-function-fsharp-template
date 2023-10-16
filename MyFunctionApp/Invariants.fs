namespace MyFunctionApp.Invariants

open System

[<RequireQualifiedAccess>]
module private ConstraintTypes =

    /// Create a constrained string using the constructor provided.
    /// Return Error if input is null or length > maxLength.
    let createString fieldName ctor maxLength (value: string) =
        if isNull value then
            Error(sprintf "%s is required" fieldName)
        elif value.Length > maxLength then
            Error(sprintf "%s must not be more than %i characters" fieldName maxLength)
        else
            Ok(ctor value)

    /// Create a optional constrained string using the constructor provided.
    /// Return Ok None if input is null.
    /// Return Error if length > maxLength.
    /// Return Ok Some if the input is valid.
    let createStringOption fieldName ctor maxLength (value: string) =
        if isNull value then
            Ok None
        elif value.Length > maxLength then
            Error(sprintf "%s must not be more than %i characters" fieldName maxLength)
        else
            Ok(ctor value |> Some)

    /// Create a constrained string using the constructor provided.
    /// Return Error if input is null or does not match the regular expression pattern.
    let createStringLike fieldName ctor pattern value =
        if isNull value then
            Error(sprintf "%s is required" fieldName)
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

type EmailAddress = private EmailAddress of string

[<RequireQualifiedAccess>]
module EmailAddress =

    let value (EmailAddress x) = x

    let tryCreate (x: string) =
        ConstraintTypes.createStringLike "Email address" EmailAddress @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$" x

type PositiveNumber = private PositiveNumber of int

[<RequireQualifiedAccess>]
module PositiveNumber =

    let defaultValue = PositiveNumber 1

    let value (PositiveNumber x) = x

    let tryCreate fieldName x =
        ConstraintTypes.createInteger fieldName PositiveNumber 1 Int32.MaxValue x

type Text = private Text of string

[<RequireQualifiedAccess>]
module Text =

    let value (Text x) = x

    let tryCreate fieldName x =
        ConstraintTypes.createString fieldName Text Int32.MaxValue x

    let tryCreateOption fieldName x =
        ConstraintTypes.createStringOption fieldName Text Int32.MaxValue x

type Text256 = private Text256 of string

[<RequireQualifiedAccess>]
module Text256 =

    let value (Text256 x) = x

    let tryCreate fieldName x =
        ConstraintTypes.createString fieldName Text256 256 x

    let tryCreateOption fieldName x =
        ConstraintTypes.createStringOption fieldName Text256 256 x

type UniqueId = private UniqueId of Guid

[<RequireQualifiedAccess>]
module UniqueId =

    let value (UniqueId x) = x

    let tryCreate (x: Guid) =
        if x <> Guid.Empty then
            x |> UniqueId |> Ok
        else
            Error "The unique identifier cannot be empty"

    let create () =
        RT.Comb.Provider.Sql.Create() |> UniqueId

type WholeNumber = private WholeNumber of int

[<RequireQualifiedAccess>]
module WholeNumber =

    let defaultValue = WholeNumber 0

    let value (WholeNumber x) = x

    let tryCreate fieldName x =
        ConstraintTypes.createInteger fieldName WholeNumber 0 Int32.MaxValue x

[<AutoOpen>]
module Alias =

    type DbConnectionString = Text

    type DisplayName = Text256

    type UserName = Text256

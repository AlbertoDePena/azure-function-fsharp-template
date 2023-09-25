namespace MyFunctionApp.Domain.ConstraintTypes

open System

[<RequireQualifiedAccess>]
module private Singleton =
    open System.Text.RegularExpressions

    let private lazyEmailRegex =
        lazy (new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))

    let EmailRegex = lazyEmailRegex.Force()

type EmailAddress =
    private
    | EmailAddress of string

    member this.Value =
        match this with
        | EmailAddress value -> value

    static member TryCreate(value: string) =
        if Singleton.EmailRegex.Match(value).Success then
            value.ToLower() |> EmailAddress |> Ok
        else
            Error(sprintf "%s is not a valid email address" value)

/// Positive whole number including zero
type NaturalNumber =
    private
    | NaturalNumber of int

    member this.Value =
        match this with
        | NaturalNumber value -> value

    static member TryCreate(value: int) =
        if value >= 0 then
            value |> NaturalNumber |> Ok
        else
            Error(sprintf "%i is not a natural number" value)

/// Positive decimal number including zero
type NaturalDecimalNumber =
    private
    | NaturalDecimalNumber of decimal

    member this.Value =
        match this with
        | NaturalDecimalNumber value -> value

    static member TryCreate(value: decimal) =
        if value >= 0.0M then
            value |> NaturalDecimalNumber |> Ok
        else
            Error(sprintf "%f is not a natural decimal number" value)

/// Positive whole number excluding zero
type PositiveNumber =
    private
    | PositiveNumber of int

    member this.Value =
        match this with
        | PositiveNumber value -> value

    static member TryCreate(value: int) =
        if value > 0 then
            value |> PositiveNumber |> Ok
        else
            Error(sprintf "%i is not a positive number" value)

/// Decimal number excluding zero
type PositiveDecimalNumber =
    private
    | PositiveDecimalNumber of decimal

    member this.Value =
        match this with
        | PositiveDecimalNumber value -> value

    static member TryCreate(value: decimal) =
        if value > 0M then
            value |> PositiveDecimalNumber |> Ok
        else
            Error(sprintf "%f is not a positive decimal number" value)

type NonEmptyText =
    private
    | NonEmptyText of string

    member this.Value =
        match this with
        | NonEmptyText value -> value

    static member TryCreate(value: string) =
        if String.IsNullOrWhiteSpace value then
            Error "The text is required and cannot be empty"
        else
            value |> NonEmptyText |> Ok

type Text =
    private
    | Text of string

    member this.Value =
        match this with
        | Text value -> value

    static member TryCreate(value: string) =
        if isNull value then
            Error "The text is required"
        else
            value |> Text |> Ok

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

[<RequireQualifiedAccess>]
type Role =
    | Administrator
    | Editor
    | Viewer

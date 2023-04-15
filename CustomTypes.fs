namespace azure_function_fsharp.CustomTypes

open System

type EmailAddress = private EmailAddress of string

[<RequireQualifiedAccess>]
module EmailAddress =
    open System.Text.RegularExpressions

    let private emailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")

    let value (EmailAddress x) = x

    let tryCreate (value: string) =
        if emailRegex.Match(value).Success then
            value.ToLower() |> EmailAddress |> Some
        else
            None

/// Positive whole number including zero
type NaturalNumber = private NaturalNumber of int

[<RequireQualifiedAccess>]
module NaturalNumber =
    
    let value (NaturalNumber x) = x

    let tryCreate (value: int) =
        if value >= 0 then
            value |> NaturalNumber |> Some
        else
            None

/// Positive decimal number including zero
type NaturalDecimalNumber = private NaturalDecimalNumber of decimal

[<RequireQualifiedAccess>]
module NaturalDecimalNumber =
    
    let value (NaturalDecimalNumber x) = x

    let tryCreate (value: decimal) =
        if value >= 0.0M then
            value |> NaturalDecimalNumber |> Some
        else
            None            

/// Positive whole number excluding zero
type PositiveNumber = private PositiveNumber of int            

[<RequireQualifiedAccess>]
module PositiveNumber =
    
    let value (PositiveNumber x) = x

    let tryCreate (value: int) =
        if value > 0 then
            value |> PositiveNumber |> Some
        else
            None

/// Decimal number excluding zero
type PositiveDecimalNumber = private PositiveDecimalNumber of decimal               

[<RequireQualifiedAccess>]
module PositiveDecimalNumber =
    
    let value (PositiveDecimalNumber x) = x

    let tryCreate (value: decimal) =
        if value > 0M then
            value |> PositiveDecimalNumber |> Some
        else
            None

type Text = private Text of string

[<RequireQualifiedAccess>]
module Text =

    let value (Text x) = x

    let tryCreate (value: string) =
        if String.IsNullOrWhiteSpace value then
            None
        else
            value.Trim() |> Text |> Some

type UniqueId = private UniqueId of Guid

[<RequireQualifiedAccess>]
module UniqueId =
    open RT.Comb

    let value (UniqueId x) = x

    let create () = Provider.Sql.Create() |> UniqueId

    let tryCreate (value: Guid) =
        if value = Guid.Empty then
            None
        else
            value |> UniqueId |> Some
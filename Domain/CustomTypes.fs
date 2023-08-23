namespace azure_function_fsharp.Domain.CustomTypes

open System
open System.Text.RegularExpressions

[<RequireQualifiedAccess>]
module private Singleton =

    let private emailRegex = lazy(new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))

    let EmailRegex = emailRegex.Force()

type EmailAddress = 
    private 
    | EmailAddress of string

    member this.Value =
        match this with
        | EmailAddress value -> value

    static member TryCreate (value: string) =
        if Singleton.EmailRegex.Match(value).Success then
            value.ToLower() |> EmailAddress |> Ok
        else
            Error (sprintf "%s is not a valid email address" value)
    
/// Positive whole number including zero
type NaturalNumber = private NaturalNumber of int

[<RequireQualifiedAccess>]
module NaturalNumber =
    
    let isValid (x: int) = x >= 0

    let value (NaturalNumber x) = x

    let tryCreate (x: int) =
        if isValid x then
            x |> NaturalNumber |> Some
        else
            None

/// Positive decimal number including zero
type NaturalDecimalNumber = private NaturalDecimalNumber of decimal

[<RequireQualifiedAccess>]
module NaturalDecimalNumber =
    
    let isValid (x: decimal) =  x >= 0.0M

    let value (NaturalDecimalNumber x) = x

    let tryCreate (x: decimal) =
        if isValid x then
            x |> NaturalDecimalNumber |> Some
        else
            None            

/// Positive whole number excluding zero
type PositiveNumber = private PositiveNumber of int            

[<RequireQualifiedAccess>]
module PositiveNumber =
    
    let isValid (x: int) = x > 0

    let value (PositiveNumber x) = x

    let tryCreate (x: int) =
        if isValid x then
            x |> PositiveNumber |> Some
        else
            None

/// Decimal number excluding zero
type PositiveDecimalNumber = private PositiveDecimalNumber of decimal               

[<RequireQualifiedAccess>]
module PositiveDecimalNumber =
    
    let isValid (x: decimal) = x > 0M

    let value (PositiveDecimalNumber x) = x

    let tryCreate (x: decimal) =
        if isValid x then
            x |> PositiveDecimalNumber |> Some
        else
            None

type Text = private Text of string

[<RequireQualifiedAccess>]
module Text =

    let isValid (x: string) = String.IsNullOrWhiteSpace x |> not

    let value (Text x) = x

    let tryCreate (x: string) =
        if isValid x then
            x.Trim() |> Text |> Some
        else
            None

type UniqueId = private UniqueId of Guid

[<RequireQualifiedAccess>]
module UniqueId =
    open RT.Comb

    let isValid (x: Guid) = x <> Guid.Empty

    let value (UniqueId x) = x

    let create () = Provider.Sql.Create() |> UniqueId

    let tryCreate (x: Guid) =
        if isValid x then
            x |> UniqueId |> Some
        else
            None
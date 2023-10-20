namespace MyFunctionApp.Invariants

open System

type EmailAddress = private EmailAddress of string

[<RequireQualifiedAccess>]
module EmailAddress =

    let value (EmailAddress x) = x

    let tryCreate (value: string) =
        if isNull value then
            None
        elif
            System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")
            |> not
        then
            None
        else
            let emailAddress = value.ToLower()
            Some(EmailAddress emailAddress)

type PositiveNumber = private PositiveNumber of int

[<RequireQualifiedAccess>]
module PositiveNumber =

    let defaultValue = PositiveNumber 1

    let value (PositiveNumber x) = x

    let tryCreate value =
        if value < 1 then None else Some(PositiveNumber value)

type Text = private Text of string

[<RequireQualifiedAccess>]
module Text =

    let value (Text x) = x

    let tryCreate (value: string) =
        if isNull value then None else Some(Text value)

type Text256 = private Text256 of string

[<RequireQualifiedAccess>]
module Text256 =

    let value (Text256 x) = x

    let tryCreate (value: string) =
        if isNull value then None
        elif value.Length > 256 then None
        else Some(Text256 value)

type UniqueId = private UniqueId of Guid

[<RequireQualifiedAccess>]
module UniqueId =

    let value (UniqueId x) = x

    let tryCreate (value: Guid) =
        if value = Guid.Empty then None else Some(UniqueId value)

    let create () =
        RT.Comb.Provider.Sql.Create() |> UniqueId

type WholeNumber = private WholeNumber of int

[<RequireQualifiedAccess>]
module WholeNumber =

    let defaultValue = WholeNumber 0

    let value (WholeNumber x) = x

    let tryCreate value =
        if value < 0 then None else Some(WholeNumber value)

[<AutoOpen>]
module Alias =

    type DbConnectionString = Text

    type DisplayName = Text256

    type UserName = Text256

namespace MyFunctionApp.Invariants

open System

type EmailAddress =
    private
    | EmailAddress of string

    member this.Value =
        let (EmailAddress value) = this
        value

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
        if String.IsNullOrWhiteSpace value then
            None
        elif Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") then
            Some(EmailAddress(value.ToLower()))
        else
            None

type UniqueId = 
    private 
    | UniqueId of Guid

    member this.Value =
        let (UniqueId value) = this
        value

    override this.ToString() = this.Value |> fun x -> x.ToString()
        
    static member TryCreate (value: Guid) =
        if value = Guid.Empty then None else Some(UniqueId value)

    static member CreateSql () =
        RT.Comb.Provider.Sql.Create() |> UniqueId
    
type Text =
    private
    | Text of string

    member this.Value =
        let (Text value) = this
        value

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
        if String.IsNullOrWhiteSpace value then
            None
        else
            Some(Text value)

[<AutoOpen>]
module Alias =
    
    type BigNumber = Int64
    type Money = Decimal
    type Number = Int32    
    type Timestamp = DateTimeOffset
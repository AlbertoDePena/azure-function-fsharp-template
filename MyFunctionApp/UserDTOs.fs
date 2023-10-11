namespace MyFunctionApp.User.DTOs

open System

[<NoComparison>]
[<CLIMutable>]
type UserDto =
    { Id: Guid
      EmailAddress: string
      DisplayName: string
      Type: string
      Permissions: string seq
      Groups: string seq }

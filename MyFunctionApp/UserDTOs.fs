namespace MyFunctionApp.User.DTOs

open System
open MyFunctionApp.Invariants
open MyFunctionApp.User.Domain
open MyFunctionApp.Extensions

[<CLIMutable>]
type UserResponse =
    { Id: Guid
      EmailAddress: string
      DisplayName: string
      Type: string }

[<RequireQualifiedAccess>]
module UserResponse =

    let fromDomain (model: User) : UserResponse =
        { Id = model.Id.Value
          EmailAddress = model.EmailAddress.Value
          DisplayName = model.DisplayName.Value
          Type = model.Type |> Type.toString }

[<NoComparison>]
[<CLIMutable>]
type UserDetailsResponse =
    { Id: Guid
      EmailAddress: string
      DisplayName: string
      Type: string
      Permissions: string seq
      Groups: string seq }

[<RequireQualifiedAccess>]
module UserDetailsResponse =

    let fromDomain (models: UserDetails list) : UserDetailsResponse seq =
        models
        |> List.map (fun model ->
            { Id = model.User.Id.Value
              EmailAddress = model.User.EmailAddress.Value
              DisplayName = model.User.DisplayName.Value
              Type = model.User.Type |> Type.toString
              Permissions = model.Permissions |> List.map Type.toString |> Seq.ofList
              Groups = model.Roles |> List.map Type.toString |> Seq.ofList })
        |> Seq.ofList

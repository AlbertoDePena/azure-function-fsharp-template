namespace MyFunctionApp.User.DTOs

open System
open MyFunctionApp.Invariants
open MyFunctionApp.User.Domain

[<CLIMutable>]
type UserResponse =
    { Id: Guid
      EmailAddress: string
      DisplayName: string
      Type: string }

[<RequireQualifiedAccess>]
module UserResponse =

    let fromDomain (model: User) : UserResponse =
        { Id = model.Id |> UniqueId.value
          EmailAddress = model.EmailAddress |> EmailAddress.value
          DisplayName = model.DisplayName |> Text.value
          Type = model.Type |> UserType.value }

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
            { Id = model.User.Id |> UniqueId.value
              EmailAddress = model.User.EmailAddress |> EmailAddress.value
              DisplayName = model.User.DisplayName |> Text.value
              Type = model.User.Type |> UserType.value
              Permissions = model.Permissions |> List.map UserPermission.value |> Seq.ofList
              Groups = model.Groups |> List.map UserGroup.value |> Seq.ofList })
        |> Seq.ofList

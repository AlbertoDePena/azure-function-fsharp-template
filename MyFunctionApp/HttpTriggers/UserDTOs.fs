namespace MyFunctionApp.HttpTriggers.DTOs

open System
open MyFunctionApp.Domain

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
          Type = model.Type.Value }

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
              Type = model.User.Type.Value
              Permissions = model.Permissions |> List.map (fun x -> x.Value) |> Seq.ofList
              Groups = model.Roles |> List.map (fun x -> x.Value) |> Seq.ofList })
        |> Seq.ofList

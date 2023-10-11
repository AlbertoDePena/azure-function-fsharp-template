namespace MyFunctionApp.User.DTOs

open System
open MyFunctionApp.User.Domain

[<CLIMutable>]
type UserDto =
    { Id: Guid
      EmailAddress: string
      DisplayName: string
      Type: string }

[<RequireQualifiedAccess>]
module UserDto =

    let fromDomain (models: User list) : UserDto seq =
        models
        |> List.map (fun model ->
            { Id = model.Id.Value
              EmailAddress = model.EmailAddress.Value
              DisplayName = model.DisplayName.Value
              Type = model.Type.Value })
        |> Seq.ofList

[<NoComparison>]
[<CLIMutable>]
type UserDetailsDto =
    { Id: Guid
      EmailAddress: string
      DisplayName: string
      Type: string
      Permissions: string seq
      Groups: string seq }

[<RequireQualifiedAccess>]
module UserDetailsDto =

    let fromDomain (models: UserDetails list) : UserDetailsDto seq =
        models
        |> List.map (fun model ->
            { Id = model.User.Id.Value
              EmailAddress = model.User.EmailAddress.Value
              DisplayName = model.User.DisplayName.Value
              Type = model.User.Type.Value
              Permissions = model.Permissions |> List.map (fun permission -> permission.Value) |> Seq.ofList
              Groups = model.Groups |> List.map (fun group -> group.Value) |> Seq.ofList })
        |> Seq.ofList

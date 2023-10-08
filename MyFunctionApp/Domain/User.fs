namespace MyFunctionApp.Domain.User

open System
open MyFunctionApp.Domain.Invariants

[<RequireQualifiedAccess>]
type UserGroup =    
    | Viewer
    | PotentialDelayAdministrator
    | PotentialDelayApprover

    member this.Value =
        match this with
        | Viewer -> "Viewer"
        | PotentialDelayAdministrator -> "Potential Delay Administrator"
        | PotentialDelayApprover -> "Potential Delay Approver"

    static member TryCreate (value: string) =
        match value with
        | "Viewer" -> Ok Viewer
        | "Potential Delay Administrator" -> Ok PotentialDelayAdministrator
        | "Potential Delay Approver" -> Ok PotentialDelayApprover
        | something when String.IsNullOrWhiteSpace something -> Error "The user group is required"
        | _ -> Error (sprintf "%s is not a valid user group" value)

[<RequireQualifiedAccess>]
type UserType =    
    | Client
    | CraneEmployee

    member this.Value =
        match this with
        | Client -> "Client"
        | CraneEmployee -> "Crane Employee"

    static member TryCreate (value: string) =
        match value with
        | "Client" -> Ok Client
        | "Crane Employee" -> Ok CraneEmployee
        | something when String.IsNullOrWhiteSpace something -> Error "The user type is required"
        | _ -> Error (sprintf "%s is not a valid user type" value)

[<RequireQualifiedAccess>]
type UserPermission =    
    | ViewAirShipments
    | ViewGroundShipments
    | ViewOceanShipments
    | ViewFinancials
    | ViewBookings
    | ExportSearchResults
    | ViewInventory
    | ViewAnalytics

type User =
    { Id: UniqueId
      EmailAddress: EmailAddress
      DisplayName: DisplayName
      Type: UserType
      Permissions: UserPermission list
      Groups: UserGroup list }

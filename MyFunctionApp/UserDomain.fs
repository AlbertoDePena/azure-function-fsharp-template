namespace MyFunctionApp.User.Domain

open MyFunctionApp.Invariants

[<RequireQualifiedAccess>]
type UserGroup =    
    | Viewer
    | Editor
    | PotentialDelayAdministrator
    | PotentialDelayApprover

    member this.Value =
        match this with
        | Viewer -> "Viewer"
        | Editor -> "Editor"
        | PotentialDelayAdministrator -> "Potential Delay Administrator"
        | PotentialDelayApprover -> "Potential Delay Approver"

    static member FromString (value: string) =
        match value with
        | "Viewer" -> Some Viewer
        | "Editor" -> Some Editor
        | "Potential Delay Administrator" -> Some PotentialDelayAdministrator
        | "Potential Delay Approver" -> Some PotentialDelayApprover
        | _ -> None

[<RequireQualifiedAccess>]
type UserType =    
    | Client
    | CraneEmployee

    member this.Value =
        match this with
        | Client -> "Client"
        | CraneEmployee -> "Crane Employee"

    static member FromString (value: string) =
        match value with
        | "Client" -> Some Client
        | "Crane Employee" -> Some CraneEmployee
        | _ -> None

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

    member this.Value =
        match this with
        | ViewAirShipments -> "View Air Shipments"
        | ViewGroundShipments -> "View Ground Shipments"
        | ViewOceanShipments -> "View Ocean Shipments"
        | ViewFinancials -> "View Financials"
        | ViewBookings -> "View Bookings"
        | ExportSearchResults -> "Export Search Results"
        | ViewInventory -> "View Inventory"
        | ViewAnalytics -> "View Analytics"

type User =
    { Id: UniqueId
      EmailAddress: EmailAddress
      DisplayName: Text256
      Type: UserType }

type UserDetails =
    { User: User
      Permissions: UserPermission list
      Groups: UserGroup list }



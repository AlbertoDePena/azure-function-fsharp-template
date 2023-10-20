namespace MyFunctionApp.User.Domain

open MyFunctionApp.Invariants

[<RequireQualifiedAccess>]
type UserGroup =    
    | Viewer
    | Editor
    | Administrator

    member this.Value =
        match this with
        | Viewer -> "Viewer"
        | Editor -> "Editor"
        | Administrator -> "Administrator"

    static member FromString (value: string) =
        match value with
        | "Viewer" -> Some Viewer
        | "Editor" -> Some Editor
        | "Administrator" -> Some Administrator
        | _ -> None

[<RequireQualifiedAccess>]
type UserType =    
    | Customer
    | Employee

    member this.Value =
        match this with
        | Customer -> "Customer"
        | Employee -> "Employee"

    static member FromString (value: string) =
        match value with
        | "Customer" -> Some Customer
        | "Employee" -> Some Employee
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
      DisplayName: Text
      Type: UserType }

type UserDetails =
    { User: User
      Permissions: UserPermission list
      Groups: UserGroup list }



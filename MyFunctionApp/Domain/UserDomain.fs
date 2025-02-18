namespace MyFunctionApp.Domain

[<RequireQualifiedAccess>]
type UserRole =
    | Viewer
    | Editor
    | Administrator

    member this.Value =
        match this with
        | UserRole.Viewer -> "Viewer"
        | UserRole.Editor -> "Editor"
        | UserRole.Administrator -> "Administrator"

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
        match value with
        | "Viewer" -> Some UserRole.Viewer
        | "Editor" -> Some UserRole.Editor
        | "Administrator" -> Some UserRole.Administrator
        | _ -> None

[<RequireQualifiedAccess>]
type UserType =
    | Customer
    | Employee

    member this.Value =
        match this with
        | UserType.Customer -> "Customer"
        | UserType.Employee -> "Employee"

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
        match value with
        | "Customer" -> Some UserType.Customer
        | "Employee" -> Some UserType.Employee
        | _ -> None

[<RequireQualifiedAccess>]
type UserPermission =
    | ViewShipments
    | ViewFinancials
    | ExportSearchResults

    member this.Value =
        match this with
        | UserPermission.ViewShipments -> "View Shipments"
        | UserPermission.ViewFinancials -> "View Financials"
        | UserPermission.ExportSearchResults -> "Export Search Results"

    override this.ToString() = this.Value

    static member TryCreate(value: string) =
        match value with
        | "View Shipments" -> Some UserPermission.ViewShipments
        | "View Financials" -> Some UserPermission.ViewFinancials
        | "Export Search Results" -> Some UserPermission.ExportSearchResults
        | _ -> None

type User =
    { Id: UniqueId
      EmailAddress: EmailAddress
      DisplayName: Text
      Type: UserType }

type UserDetails =
    { User: User
      Permissions: UserPermission list
      Roles: UserRole list }

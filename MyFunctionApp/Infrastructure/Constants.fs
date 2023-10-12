namespace MyFunctionApp.Infrastructure.Constants

[<RequireQualifiedAccess>]
module ClaimType =

    [<Literal>]
    let EmailAddress =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"

    [<Literal>]
    let Role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"

    [<Literal>]
    let Groups = "groups"

[<RequireQualifiedAccess>]
module ClaimValue =

    [<Literal>]
    let PotentialDelayAdministrator = "Potential-Delay-Administrator"

    [<Literal>]
    let PotentialDelayApprover = "Potential-Delay-Approver"

    [<Literal>]
    let Viewer = "Viewer"

    [<Literal>]
    let Editor = "Editor"

[<RequireQualifiedAccess>]
module DimensionName =

    [<Literal>]
    let UserName = "MyApp.UserName"

[<RequireQualifiedAccess>]
module HttpMethod =

    [<Literal>]
    let Delete = "DELETE"

    [<Literal>]
    let Get = "GET"

    [<Literal>]
    let Post = "POST"

    [<Literal>]
    let Put = "PUT"

[<RequireQualifiedAccess>]
module LogEvent =
    open Microsoft.Extensions.Logging

    let AuthenticationError = EventId(10401, "AuthenticationError")

    let AuthorizationError = EventId(10403, "AuthorizationError")

    let DataStorageError = EventId(10000, "DataStorageError")

    let InternalServerError = EventId(10500, "InternalServerError")

[<RequireQualifiedAccess>]
module MetricName =

    [<Literal>]
    let SearchUsers = "MyFunctionApp.SearchUsers"

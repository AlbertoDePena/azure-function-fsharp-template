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
    let Administrator = "MyApp_Administrator"

    [<Literal>]
    let Editor = "MyApp_Editor"

    [<Literal>]
    let Viewer = "MyApp_Viewer"

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

    let DataAccessError = EventId(10000, "DataAccessError")

    let AuthenticationError = EventId(10401, "AuthenticationError")

    let AuthorizationError = EventId(10403, "AuthorizationError")

    let InternalServerError = EventId(10500, "InternalServerError")

[<RequireQualifiedAccess>]
module MetricName =

    [<Literal>]
    let SayHello = "MyApp.SayHello"

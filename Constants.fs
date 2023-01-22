namespace azure_function_fsharp.Constants

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

    let InternalServerError = EventId(10000, "InternalServerError")

    let DataAccessError = EventId(10002, "DataAccessError")

    let AuthenticationError = EventId(10003, "AuthenticationError")

    let AuthorizationError = EventId(10004, "AuthorizationError")

[<RequireQualifiedAccess>]
module MetricName =

    [<Literal>]
    let AuthenticatedUsers = "MyApp.AuthenticatedUsers"

    [<Literal>]
    let SayHello = "MyApp.SayHello"

[<RequireQualifiedAccess>]
module DimensionName =

    [<Literal>]
    let UserName = "MyApp.UserName"
namespace azure_function_fsharp.Constants

[<RequireQualifiedAccess>]
module ConfigurationName =

    [<Literal>]
    let APPLICATION_MESSAGE = "APPLICATION_MESSAGE"

    [<Literal>]
    let ENABLE_SQL_TELEMETRY = "ENABLE_SQL_TELEMETRY"
    
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
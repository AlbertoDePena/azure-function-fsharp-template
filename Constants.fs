namespace azure_function_fsharp.Constants

[<RequireQualifiedAccess>]
module ConfigurationKey =

    [<Literal>]
    let APPLICATION_MESSAGE = "MY_APP_APPLICATION_MESSAGE"

    [<Literal>]
    let DB_CONNECTION_STRING = "MY_APP_DB_CONNECTION_STRING"
    
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
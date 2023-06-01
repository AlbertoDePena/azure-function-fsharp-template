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

[<RequireQualifiedAccess>]
module Logging =

    /// Template for consisted structured logging accross multiple functions, each field is described below: 
    /// EntityType: Business Entity Type being processed: e.g. Invoice, Shipment, etc.
    /// EntityId: Id of the Business Entity being processed: e.g. Invoice Number, Shipment Id, etc. 
    /// Status: Status of the Log Event, e.g. Succeeded, Failed, Discarded.
    /// CheckPoint: To classify and be able to correlate tracking events, e.g. Publisher, Subscriber.
    /// CorrelationId: Unique identifier of the message that can be processed by more than one component. 
    /// Description: A detailed description of the log event. 
    [<Literal>]
    let Template = "{EntityType}, {EntityId}, {Status}, {CheckPoint}, {CorrelationId}, {Description}"
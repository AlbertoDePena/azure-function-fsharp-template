namespace azure_function_fsharp

[<CLIMutable>]
type GreeterConfiguration = { Message: string }

[<RequireQualifiedAccess>]
module HttpMethod =

    [<Literal>]
    let Delete = "delete"

    [<Literal>]
    let Get = "get"

    [<Literal>]
    let Post = "post"

    [<Literal>]
    let Put = "put"

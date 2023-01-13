namespace azure_function_fsharp.ObjectResults

open System
open Microsoft.AspNetCore.Mvc

type AcceptedObjectResult(id) =
    inherit ObjectResult({| id = id |})

    override _.ExecuteResultAsync(context: ActionContext) =
        context.HttpContext.Response.StatusCode <- Net.HttpStatusCode.Accepted |> int

        let hostString = context.HttpContext.Request.Host

        let uri =
            UriBuilder(
                schemeName = context.HttpContext.Request.Scheme,
                hostName = hostString.Host,
                Path = $"{context.HttpContext.Request.Path}/{id}"
            )

        if hostString.Port.HasValue then
            uri.Port <- hostString.Port.Value

        context.HttpContext.Response.Headers.Add("Location", uri.ToString())

        base.ExecuteResultAsync(context)

type CreatedObjectResult(id) =
    inherit ObjectResult({| id = id |})

    override _.ExecuteResultAsync(context: ActionContext) =
        context.HttpContext.Response.StatusCode <- Net.HttpStatusCode.Created |> int

        let hostString = context.HttpContext.Request.Host

        let uri =
            UriBuilder(
                schemeName = context.HttpContext.Request.Scheme,
                hostName = hostString.Host,
                Path = $"{context.HttpContext.Request.Path}/{id}"
            )

        if hostString.Port.HasValue then
            uri.Port <- hostString.Port.Value

        context.HttpContext.Response.Headers.Add("Location", uri.ToString())

        base.ExecuteResultAsync(context)
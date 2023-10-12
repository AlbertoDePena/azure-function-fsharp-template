namespace MyFunctionApp.HttpTriggers.SayHello

open System
open System.Data
open System.Web.Http

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.ApplicationInsights
open Microsoft.Extensions.Options

open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Infrastructure.HttpRequestHandler
open MyFunctionApp.Infrastructure.Options

open MyFunctionApp.Invariants
open MyFunctionApp.Extensions
open MyFunctionApp.User.Domain
open MyFunctionApp.User.Storage
open MyFunctionApp.Shared.DTOs
open MyFunctionApp.User.DTOs

type SayHello
    (
        logger: ILogger<SayHello>,
        databaseOptions: IOptions<Database>,
        httpRequestHandler: HttpRequestHandler,
        telemetryClient: TelemetryClient
    ) =

    [<FunctionName(nameof SayHello)>]
    member this.Run
        ([<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get, Route = "v1/SayHello")>] httpRequest: HttpRequest)
        =

        httpRequestHandler.Handle httpRequest [ UserGroup.Viewer ] (fun userName ->
            async {
                let dbConnectionString =
                    databaseOptions.Value.ConnectionString
                    |> DbConnectionString.TryCreate
                    |> Result.valueOr failwith

                let emailAddress =
                    userName.Value |> EmailAddress.TryCreate |> Result.valueOr failwith

                let queryValidation =
                    QueryRequest.toDomain
                        { SearchCriteria =
                            httpRequest.TryGetQueryStringValue "searchCriteria"
                            |> Option.defaultValue String.defaultValue
                          ActiveOnly =
                            httpRequest.TryGetQueryStringValue "activeOnly"
                            |> Option.bind (Boolean.TryParse >> Option.ofPair)
                            |> Option.defaultValue false
                          Page =
                            httpRequest.TryGetQueryStringValue "page"
                            |> Option.bind (Int32.TryParse >> Option.ofPair)
                            |> Option.defaultValue 1
                          PageSize =
                            httpRequest.TryGetQueryStringValue "pageSize"
                            |> Option.bind (Int32.TryParse >> Option.ofPair)
                            |> Option.defaultValue 1
                          SortBy =
                            httpRequest.TryGetQueryStringValue "sortBy"
                            |> Option.defaultValue String.defaultValue
                          SortDirection =
                            httpRequest.TryGetQueryStringValue "sortDirection"
                            |> Option.defaultValue String.defaultValue }

                match queryValidation with
                | Error errors ->
                    return BadRequestObjectResult(ApiMessageResponse.fromMessages errors) :> IActionResult

                | Ok query ->
                    let! pagedDataResult = UserStorage.search dbConnectionString query |> Async.Catch

                    match pagedDataResult with
                    | Choice1Of2 pagedData ->
                        let guid = Guid.NewGuid()
                        let correlationId = guid.ToString()

                        let pagedDataResponse =
                            pagedData |> PagedDataResponse.fromDomain UserResponse.fromDomain

                        telemetryClient.GetMetric(MetricName.SayHello).TrackValue(1) |> ignore

                        logger.LogDebug("what it do? {CorrelationId}", correlationId)

                        return OkObjectResult(pagedDataResponse) :> IActionResult

                    | Choice2Of2 ex ->
                        logger.LogError(LogEvent.InternalServerError, ex, ex.Message)
                        return InternalServerErrorResult() :> IActionResult
            })

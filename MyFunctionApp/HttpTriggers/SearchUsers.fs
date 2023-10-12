namespace MyFunctionApp.HttpTriggers.SayHello

open System
open System.Data
open System.Web.Http
open System.Threading.Tasks

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
open MyFunctionApp.Domain
open MyFunctionApp.User.Domain
open MyFunctionApp.User.Storage
open MyFunctionApp.Shared.DTOs
open MyFunctionApp.User.DTOs

type SearchUsers
    (
        logger: ILogger<SearchUsers>,
        databaseOptions: IOptions<Database>,
        httpRequestHandler: HttpRequestHandler,
        telemetryClient: TelemetryClient
    ) =

    [<FunctionName(nameof SearchUsers)>]
    member this.Run
        ([<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get, Route = "v1/Users/Search")>] httpRequest:
            HttpRequest)
        =

        httpRequestHandler.HandleAsync httpRequest [ UserGroup.Viewer ] (fun userName ->
            task {
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
                    let! pagedDataStatus =
                        UserStorage.getPagedData dbConnectionString query
                        |> Task.handleException (fun ex -> logger.LogError(LogEvent.DataStorageError, ex, ex.Message))

                    match pagedDataStatus with
                    | Task.TaskStatus.Completed pagedData ->
                        let guid = Guid.NewGuid()
                        let correlationId = guid.ToString()

                        let pagedDataResponse =
                            pagedData |> PagedDataResponse.fromDomain UserResponse.fromDomain

                        telemetryClient.GetMetric(MetricName.SearchUsers).TrackValue(1) |> ignore

                        logger.LogDebug("what it do? {CorrelationId}", correlationId)

                        return OkObjectResult(pagedDataResponse) :> IActionResult

                    | Task.TaskStatus.Failed -> return InternalServerErrorResult() :> IActionResult
            })

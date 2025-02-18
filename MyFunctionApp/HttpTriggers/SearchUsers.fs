namespace MyFunctionApp.HttpTriggers

open System
open System.Data
open System.Threading.Tasks

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc
open Microsoft.ApplicationInsights
open Microsoft.Extensions.Options

open FsToolkit.ErrorHandling

open MyFunctionApp.Infrastructure.Constants
open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.HttpTriggers.HttpRequestHandler
open MyFunctionApp.Infrastructure.Options

open MyFunctionApp.Infrastructure.Extensions
open MyFunctionApp.Domain
open MyFunctionApp.Infrastructure.Database
open MyFunctionApp.HttpTriggers.DTOs
open Microsoft.Azure.Functions.Worker

type SearchUsers
    (
        logger: ILogger<SearchUsers>,
        httpRequestHandler: HttpRequestHandler,
        telemetryClient: TelemetryClient,
        userDatabase: UserDatabase
    ) =

    [<Function(nameof SearchUsers)>]
    member this.Run
        ([<HttpTrigger(AuthorizationLevel.Anonymous, HttpMethod.Get, Route = "v1/Users/Search")>] httpRequest:
            HttpRequest)
        =

        httpRequestHandler.HandleAsync httpRequest [ UserRole.Viewer ] (fun userName ->
            task {
                
                logger.LogInformation("{UserName} is requesting data", userName)

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
                    let! pagedData = userDatabase.GetPagedData query

                    let guid = Guid.NewGuid()
                    let correlationId = guid.ToString()

                    let pagedDataResponse =
                        pagedData |> PagedDataResponse.fromDomain UserResponse.fromDomain

                    telemetryClient.GetMetric(MetricName.SearchUsers).TrackValue(1) |> ignore

                    logger.LogDebug("what it do? {CorrelationId}", correlationId)

                    return OkObjectResult(pagedDataResponse) :> IActionResult
            })

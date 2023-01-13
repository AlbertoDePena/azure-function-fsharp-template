namespace azure_function_fsharp.Telemetry

open System.Reflection

open Microsoft.ApplicationInsights.Extensibility
open Microsoft.ApplicationInsights.Channel
open Microsoft.ApplicationInsights.DataContracts
open Microsoft.Data.SqlClient

type ComponentVersionInitializer() =

    interface ITelemetryInitializer with

        member this.Initialize(telemetry: ITelemetry) =
            telemetry.Context.Component.Version <-
                Assembly
                    .GetAssembly(
                        typeof<ComponentVersionInitializer>
                    )
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion

type CloudRoleNameInitializer() =

    interface ITelemetryInitializer with

        member this.Initialize(telemetry: ITelemetry) = 
            telemetry.Context.Cloud.RoleName <- "azure-function-template"

type SqlTelemetryInitializer() =

    interface ITelemetryInitializer with

        member this.Initialize(telemetry: ITelemetry) =

            if telemetry :? DependencyTelemetry then
                let dependencyTelemetry = telemetry :?> DependencyTelemetry

                if dependencyTelemetry.Type = "SQL" then
                    match dependencyTelemetry.TryGetOperationDetail("SqlCommand") with
                    | true, operationDetail ->
                        let sqlCommand = operationDetail :?> SqlCommand
                        let commandType = sqlCommand.CommandType
                        let commandTimeout = sqlCommand.CommandTimeout

                        dependencyTelemetry.Data <- sqlCommand.CommandText

                        dependencyTelemetry.Properties.Add(
                            nameof (sqlCommand.CommandType),
                            commandType.ToString()
                        )

                        dependencyTelemetry.Properties.Add(
                            nameof (sqlCommand.CommandTimeout),
                            commandTimeout.ToString()
                        )
                    | _ -> ()

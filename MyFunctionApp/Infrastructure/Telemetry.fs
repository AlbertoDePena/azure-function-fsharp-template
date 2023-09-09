namespace MyFunctionApp.Infrastructure.Telemetry

open System.Reflection

open Microsoft.ApplicationInsights.Extensibility
open Microsoft.ApplicationInsights.Channel

type ComponentVersionInitializer() =

    interface ITelemetryInitializer with

        member this.Initialize(telemetry: ITelemetry) =
            telemetry.Context.Component.Version <-
                Assembly
                    .GetAssembly(typeof<ComponentVersionInitializer>)
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion

type CloudRoleNameInitializer() =

    interface ITelemetryInitializer with

        member this.Initialize(telemetry: ITelemetry) =
            telemetry.Context.Cloud.RoleName <- "azure-function-template"

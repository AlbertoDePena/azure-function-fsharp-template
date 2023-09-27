namespace MyFunctionApp.Infrastructure.Options

[<CLIMutable>]
type Application = { Message: string }

[<CLIMutable>]
type Database = { ConnectionString: string }

[<CLIMutable>]
type AzureAd = { TenantId: string; ClientId: string }

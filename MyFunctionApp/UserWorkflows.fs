namespace MyFunctionApp.User.Workflows

open System
open FsToolkit.ErrorHandling

open Microsoft.Extensions.Logging

open MyFunctionApp.Invariants
open MyFunctionApp.Domain
open MyFunctionApp.User.Domain

open MyFunctionApp.Shared.DTOs

type IUserStorage =
    abstract member AsyncDelete: UniqueId -> Async<unit>
    abstract member AsyncSave: User -> Async<unit>
    abstract member AsyncSearch: Query -> Async<PagedData<User>>        
    abstract member AsyncTryFindByEmailAddress: EmailAddress -> Async<UserDetails option>
    abstract member AsyncTryFindById: UniqueId -> Async<UserDetails option>
    
    
[<RequireQualifiedAccess>]
module UserWorkflows =

    let search (logger: ILogger) (storage: IUserStorage) (query: Query) = 
        async {
            
            let! pagedData = storage.AsyncSearch query               

            ()
        }   

module AppearCollapser.Printer

open AppearCollapser.Database
open Microsoft.Extensions.Logging
open AppearCollapser.Infrastructure

let print (logger:ILogger) (result: Result<string list * Appear option, Error>) =
    match result with
    | Ok (completedAppear, nextAppear) -> 
        logger.LogInformation("Job done: {CompletedAppear}", completedAppear)
    
        match nextAppear with
        | Some x ->  logger.LogInformation("Next appear {Appear}", x.ident)
        | None -> ()
    | Error reason  ->
        match reason with
        | LibraryNotFound path -> logger.LogCritical("Library not found in path {Path}", path)
        | UnhandledException e -> logger.LogCritical(e, "Something wrong")

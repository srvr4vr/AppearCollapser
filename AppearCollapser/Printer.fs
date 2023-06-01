module AppearCollapser.Printer

open AppearCollapser.Database
open Microsoft.Extensions.Logging

let print (logger:ILogger) (completedAppear: string list, nextAppear: Appear option) =
    logger.LogInformation("Job done: {CompletedAppear}", completedAppear)
    
    match nextAppear with
    | Some x ->  logger.LogInformation("Next appear {Appear}", x.ident)
    | None -> ()
    

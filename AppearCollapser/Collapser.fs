module AppearCollapser.Collapser

open System.Diagnostics
open AppearCollapser.Database
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open AppearCollapser.Parameters
open SafeBuilder
open AppearCollapser.Infrastructure
    
let private getAppears startAppearIdent date =
    Seq.filter (fun x -> x.startDate < date)
    >> Seq.filter (fun x -> x.ident <> Appear.defaultName)
    >> Seq.sortBy (fun x -> x.startDate)
    >> match startAppearIdent with
        | Some ident -> Seq.skipWhile (fun x -> x.ident <> ident)
        | None -> id
    >> Seq.toList 
    
let private proceed (logger:ILogger) db parameters =        
    let rec loop (completedAppears: string list) appears =
        let loopHandler appear left =
            
            logger.LogInformation("Start {Appear}", appear.ident)
            
            let sw = Stopwatch.StartNew()
            AppearHandler.proceed db appear
            sw.Stop()
            
            logger.LogInformation("Done {Appear}. {AppearLeft} - left. Elapsed time is {Time}s",
                                  appear.ident, List.length left, sw.Elapsed.TotalSeconds)
            
            match parameters.git with
            | Some task -> Git.addAndCommit parameters.directory task
            | None -> ()
            
            loop (appear.ident::completedAppears) left
            
        match (appears, List.length completedAppears, parameters.limit) with
        | [], _, _ 
          -> (completedAppears, None)
        | appear::_, count, Some limit when count = limit
          -> (completedAppears, Some appear)
        | appear::leftOverAppears, _,  _
          -> loopHandler appear leftOverAppears
            
    (parameters.startAppear, parameters.date, db.appears) 
    |||> getAppears  
    |> loop []

let private createDb (logger:ILogger) parameters =
    try 
        Database (logger, parameters.directory) |> Ok
    with | e -> Error (LibraryNotFound parameters.directory) 

let run (logger:ILogger) parameters =
    safe {
        try
            let! db = createDb logger parameters
            return proceed logger db parameters
        with ex ->
            return! Error(UnhandledException ex) 
    }
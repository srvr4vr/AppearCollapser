module AppearCollapser.Collapser

open System.Diagnostics
open System.Text.RegularExpressions
open AppearCollapser.Database
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open AppearCollapser.Parameters
   
let private appearIdentRegex = Regex (@"(""appearIdent"": "")(\w+)("")", RegexOptions.Compiled)

let private proceedAppear (db:Database) appear =
    let mapper =
        Seq.groupBy (fun x -> x.id)
        >> Seq.map snd
        >> Seq.filter (Seq.exists (fun x -> x.appear = appear))
        >> Seq.toList

    let modifier (db:Database) table appear rows =
        let replaceIdent appear row =
            { row with data = appearIdentRegex.Replace(row.data, $@"$1{appear.ident}$3" ); appear = appear }
            
        let sortedPast =
            rows
            |> Seq.filter (fun x -> x.appear.startDate <= appear.startDate)
            |> Seq.sortByDescending (fun x -> x.appear.startDate)
            |> List.ofSeq
            
        if (Seq.length sortedPast > 0) then
            sortedPast |> List.iter (db.removeRow table)
            
            sortedPast.Head
            |> replaceIdent (db.getAppearByName Appear.defaultAppear)
            |> db.addRow table
       
    db.tables
    |> Seq.map (fun (name, rows) -> (name, mapper rows))
    |> Seq.iter (fun (table, rowSet) -> rowSet |> Seq.iter (modifier db table appear))
    
    db.fixBalances appear
    db.removeAppear appear.ident
    
let private getAppears startAppearIdent date =
    Seq.filter (fun x -> x.startDate < date)
    >> Seq.filter (fun x -> x.ident <> Appear.defaultAppear)
    >> Seq.sortBy (fun x -> x.startDate)
    >> match startAppearIdent with
        | Some ident -> Seq.skipWhile(fun x -> x.ident <> ident)
        | None -> id
    >> Seq.toList 
    
let private proceed parameters (db:Database) =        
    let rec loop (completedAppears: string list) appears =
        let loopHandler appear left =
            printfn $"Start {appear.ident}"
            let sw = Stopwatch.StartNew()
            proceedAppear db appear
            sw.Stop()
            
            printfn $"Done {appear.ident}. {List.length left} - left. Elapsed time is {sw.Elapsed.TotalSeconds}s"
            
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

let collapse parameters =
    Database parameters.directory
    |> proceed parameters
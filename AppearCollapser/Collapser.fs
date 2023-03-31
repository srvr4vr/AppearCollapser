module AppearCollapser.Collapser

open System.Diagnostics
open System.Text.RegularExpressions
open AppearCollapser.Database
open FSharp.Collections.ParallelSeq
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open AppearCollapser.Parameters

let private appearIdentRegex = Regex (@"""appearIdent"": ""(\w+)""", RegexOptions.Compiled)
let private modifier (db:database) table appear rows =
    
    let replaceIdent appear row =
        { row with data = appearIdentRegex.Replace(row.data, $@"""appearIdent"": ""{appear.ident}""" ); appear = appear }
        
    let sortedPast =
        rows
        |> Seq.filter (fun x -> x.appear.startDate <= appear.startDate)
        |> Seq.sortByDescending (fun x -> x.appear.startDate)
        |> List.ofSeq
        
    if (Seq.length sortedPast > 0) then
        let edited =
            sortedPast.Head
            |> replaceIdent (db.getAppearByName defaultAppear)
        sortedPast
        |> List.iter (db.removeRow table)
        db.addOrEdit table edited
   
let private proceedAppear (db:database) appear =
    let mapper (table:string): row seq seq  =
        db.getRows table
        |> Seq.groupBy (fun x -> x.id)
        |> Seq.map snd
        |> Seq.filter (fun x-> x |> Seq.exists (fun y -> y.appear = appear))
       
    db.tables
    |> PSeq.map (fun x -> (x, mapper x))
    |> PSeq.iter (fun (table, rowSet) -> rowSet |> PSeq.iter (List.ofSeq >> (modifier db table appear)))
    
    db.fixBalances appear
    db.removeAppear appear.ident
    
let private getAppears (startAppearIdent: string option) date (db:database) =
    db.appears
    |> PSeq.filter (fun x -> x.startDate < date)
    |> PSeq.filter (fun x -> x.ident <> defaultAppear)
    |> PSeq.sortBy (fun x -> x.startDate)
    |> match startAppearIdent with
        | Some ident -> PSeq.skipWhile(fun x -> x.ident <> ident)
        | None -> id
    |> PSeq.toList 
    
let private go parameters (db:database) =        
    let rec loop (completedAppears: string list) appears =
        match (appears, completedAppears.Length, parameters.limit) with
        | [], _, _ -> (completedAppears, None)
        | appear::_, count, Some limit when count >= limit -> (completedAppears, Some appear)
        | appear::leftOverAppears, _, _ ->
            printfn $"Start {appear.ident}"
            let sw = Stopwatch.StartNew()
            proceedAppear db appear
            sw.Stop()
            
            printfn $"Done {appear.ident}. {leftOverAppears.Length} - left. Elapsed time is {sw.Elapsed.TotalSeconds}s"
            
            match parameters.git with
            | Some task -> Git.addAndCommit task parameters.directory
            | None -> ()
            
            loop (appear.ident::completedAppears) leftOverAppears
    ( parameters.startAppear, parameters.date, db) 
    |||> getAppears 
    |> loop []

let collapse parameters =
    database parameters.directory
    |> go parameters
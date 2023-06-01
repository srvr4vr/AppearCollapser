[<Microsoft.FSharp.Core.RequireQualifiedAccess>]
module AppearCollapser.AppearHandler

open AppearCollapser.Database
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Functions

let private getGroupedRowsFor appear =
    Seq.groupBy (fun x -> x.id)
    >> Seq.map snd
    >> Seq.filter (Seq.exists (fun x -> x.appear = appear))
    >> Seq.toList
 
let private defaultAppear (db:Database) = db.getAppearByName Appear.defaultName

let private collapseRows (db:Database) table appear rows =
    let replaceIdent appear row =
        { row with data = JsonHelper.replaceAppear appear.ident row.data; appear = appear }
        
    let sortedPast =
        rows
        |> Seq.filter (fun x -> x.appear.startDate <= appear.startDate)
        |> Seq.sortByDescending (fun x -> x.appear.startDate)
        |> List.ofSeq
        
    if (List.length sortedPast > 0) then
        sortedPast |> List.iter (db.removeRow table)
        
        sortedPast.Head
        |> replaceIdent (defaultAppear db)
        |> db.addRow table
        
let private collapseGroups (db:Database) appear (table, rowSet) =
    rowSet |> Seq.iter (collapseRows db table appear)

let proceed (db:Database) appear =
    let groupRows = mapBoth (id, getGroupedRowsFor appear)
    let collapse = collapseGroups db appear
    
    db.tables
    |> Seq.map groupRows
    |> Seq.iter collapse
    db.fixBalances appear
    db.removeAppear appear.ident

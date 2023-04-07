namespace AppearCollapser.Database

open System.Collections.Generic
open System.IO
open FSharp.Collections.ParallelSeq
open System
open FSharp.Json

type Row = {
    appear: Appear
    id: string
    data: string
}

type appearRecord = {
    appearIdent: string
}

type column = {
    comment: string option
}

type tableScheme = {
    name: string 
    columns: Map<string, column>
}

module Table =
    let private createRow (appears: IReadOnlyDictionary<string, Appear>) path =
        let getId (str:string) =
            let span = str.AsSpan()
            if str.Contains('#') then
                span.Slice(0, span.LastIndexOf('#')).ToString()
            else
                span.Slice(0, span.LastIndexOf('.')).ToString()
       
        let data = File.ReadAllText(path)
        
        let appear = Json.deserialize data
            
        { id = Path.GetFileName(path) |> getId; appear = appears[appear.appearIdent]; data = data; }

    let private tablesWithAppears =
        let getColumns =
            JsonHelper.loadJsonEx
            >> fun x -> x.columns
            >> Map.keys
            >> List.ofSeq
        JsonHelper.getFiles
        >> PSeq.map FileInfo
        >> PSeq.filter (fun file -> file.Name <> Appear.appearDirectoryName)
        >> PSeq.map (fun x -> (Path.GetFileNameWithoutExtension(x.FullName), getColumns x.FullName))
        >> PSeq.filter (fun (_, columns) -> columns |> List.exists ((=) "appearIdent"))
        >> PSeq.map fst
        
    let private loadTable appears =
        JsonHelper.getFiles
        >> PSeq.map (createRow appears)
        >> PSeq.toList

    let loadTables appears directory = 
        tablesWithAppears directory
        |> PSeq.map (fun tableName -> (tableName, Path.Combine(directory, tableName)))
        |> PSeq.filter (snd >> Directory.Exists)
        |> PSeq.map (fun (name, path) -> (name, loadTable appears path))
        |> dict
        |> Dictionary

    let private getTableFileName table row =
        match table with
        | "exchanges" | "asset_bundle_decoration_config" | "battlePass_quests" | "hero_faction" -> row.id
        | _ -> $"{row.id}#{row.appear.ident}"
        
    let removeRow directory table row =
        (Path.Combine(directory, table), getTableFileName table row)
        ||> JsonHelper.removeFromDisc
        
    let addRow directory table row =
        (Path.Combine(directory, table), getTableFileName table row, row.data)
        |||> JsonHelper.writeToDisk
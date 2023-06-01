namespace AppearCollapser.Database

open System.Collections.Generic
open System.IO
open FSharp.Collections.ParallelSeq
open Functions
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
            let getIdInternal (char:Char) =
                let span = str.AsSpan()
                span.Slice(0, span.LastIndexOf(char)).ToString()
                
            let getDelimiter =
                str.Contains('#') ?= ('#', '.')

            getDelimiter |> getIdInternal
       
        let data = File.ReadAllText(path)
        
        let appear = Json.deserialize data
            
        { id = Path.GetFileName path |> getId; appear = appears[appear.appearIdent.ToLower()]; data = data; }

    let private tablesWithAppears =
        let getColumns =
            JsonHelper.loadJsonEx
            >> fun x -> x.columns
            >> Map.keys
            >> List.ofSeq
        JsonHelper.getFiles
        >> PSeq.map FileInfo
        >> PSeq.filter (fun file -> file.Name <> Appear.directoryName)
        >> PSeq.map (fun x -> x.FullName)
        >> PSeq.map (mapTo (Path.GetFileNameWithoutExtension, getColumns))
        >> PSeq.filter (fun (_, columns) -> columns |> List.exists ((=) "appearIdent"))
        >> PSeq.map fst
        
    let private loadTable appears =
        JsonHelper.getFiles
        >> PSeq.map (createRow appears)
        >> PSeq.toList

    let loadTables appears directory = 
        tablesWithAppears directory
        |> PSeq.map (mapTo (id, Path.combine directory))
        |> PSeq.filter (snd >> Directory.Exists)
        |> PSeq.map (fun (name, path) -> (name, loadTable appears path))
        |> dict
        |> Dictionary

    let private getTableFileName table row =
        match table with
        | "exchanges" | "asset_bundle_decoration_config" | "battlePass_quests" | "hero_faction" -> row.id
        | _ -> $"{row.id}#{row.appear.ident}"
        
    let removeRow directory table row =
        (Path.combine directory table, getTableFileName table row)
        ||> JsonHelper.removeFromDisc
        
    let addRow directory table row =
        (Path.combine directory table, getTableFileName table row, row.data)
        |||> JsonHelper.writeToDisk
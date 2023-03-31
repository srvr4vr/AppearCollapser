module AppearCollapser.Database

open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open FSharp.Collections.ParallelSeq
open FSharp.Json

let defaultAppear = "default"

[<Struct>]
type AppearIdent = Name of string

type appear =
    {
        ident: string
        startDate: DateTime
    }

type column = {
    comment: string option
}

type tableScheme = {
    name: string 
    columns: Map<string, column>
}

type appearRecord = {
    appearIdent: string
}

type row =
    {
        appear: appear
        id: string
        data: string
    }
        
type database (directory:string) =
    do printfn "%s %s" "db start load" (DateTime.Now.ToString "HH:mm:ss")
    let loadJson path =
        File.ReadAllText(path)
        |> Json.deserialize
    
    let loadJsonEx config path =
        File.ReadAllText(path)
        |> Json.deserializeEx config
        
    let createRow (path, appears: IReadOnlyDictionary<string, appear>) =
        let getId (str:string) =
            let span = str.AsSpan()
            if str.Contains('#') then
                span.Slice(0, span.LastIndexOf('#')).ToString()
            else
                span.Slice(0, span.LastIndexOf('.')).ToString()
       
        let data = File.ReadAllText(path)
        
        let appear = Json.deserialize<appearRecord> data
            
        { id = Path.GetFileName(path) |> getId; appear = appears[appear.appearIdent]; data = data; }
    let getJsonFiles directory =
        Directory.EnumerateFiles(directory, "*.json")
    let appearDirectory = Path.Combine(directory,  "appear")

    let _appears =
        appearDirectory
        |> getJsonFiles
        |> PSeq.map loadJson
        |> PSeq.map (fun x -> x.ident, x)
        |> dict
        |> Dictionary
        
    let tablesWithAppears =
        let getColumns (x:FileInfo) =
            let config = JsonConfig.create(deserializeOption = DeserializeOption.RequireNull)
            loadJsonEx config x.FullName
            |> fun x -> x.columns
            |> Map.keys
            |> List.ofSeq
        getJsonFiles
        >> PSeq.map (fun x -> FileInfo(x))
        >> PSeq.filter (fun file -> file.Name <> appearDirectory)
        >> PSeq.map (fun x -> (Path.GetFileNameWithoutExtension(x.Name), getColumns x))
        >> PSeq.filter (fun (_, columns) -> columns |> List.exists ((=) "appearIdent"))
        >> PSeq.map fst
        
    let getTableRows directory =
        directory
        |> getJsonFiles
        |> PSeq.map (fun x -> createRow (x, _appears))
        
    let _tables =
        tablesWithAppears directory
        |> PSeq.map (fun x -> (x, Path.Combine (directory, x)))
        |> PSeq.filter (fun (_, path) -> Directory.Exists path)
        |> PSeq.map (fun (name, path) -> (name, (getTableRows path) |> List.ofSeq))
        |> PSeq.toList
        |> dict
        |> Dictionary
    
    do printfn "%s %s" $"db end load" (DateTime.Now.ToString "HH:mm:ss")

    let removeFromDisc directory id =
        let path = Path.Combine(directory, $"{id}.json")
        File.Delete(path)
    
    let write directory id data =
        let path = Path.Combine(directory, $"{id}.json")
        File.WriteAllText(path, data)
        
    let getName table row =
        match table with
        | "exchanges" | "asset_bundle_decoration_config" | "battlePass_quests" | "hero_faction" -> row.id
        | _ -> $"{row.id}#{row.appear.ident}"
        
    let writeRow table row =
        let tableFolder = Path.Combine(directory, table)
        write tableFolder (getName table row) row.data
        
    let deleteRow table row =
        let tableFolder = Path.Combine(directory, table)
        removeFromDisc tableFolder (getName table row)
        
    member this.appears:IReadOnlyCollection<appear> = _appears.Values
    member this.tables:IReadOnlyCollection<string> = _tables.Keys
    member this.getRows table = _tables[table] |> Seq.toList
    
    member this.getAppearByName name = _appears[name]
    
    member this.removeAppear appearIdent =
        try 
            _appears.Remove(appearIdent) |> ignore
            removeFromDisc appearDirectory appearIdent
        with | e ->  printf $"[Appear remove error {appearIdent}]: %s{e.Message}"
    
    member this.removeRow table row =
        try
            let excludeList = _tables[table] |> Seq.filter (fun x -> x <> row) |> List.ofSeq
            _tables[table] <- excludeList
            let tableDirectory = Path.Combine(directory, table)
            deleteRow tableDirectory row
        with | e -> printf $"[Row in {table} table remove error]: %s{e.Message}"
    
    member this.addOrEdit table row =
        try
            _tables[table] <- row::_tables[table];
                
            writeRow table row
        with | e -> printf $"[Row in {table} table add error]: %s{e.Message}"
    
    member this.fixBalances appear =
        let regex = Regex $"(\"appear\":\\s+\")({appear.ident})(\")"
        let replace (str:string) =
            regex.Replace(str, $"$1{defaultAppear}$3")
            
        Path.Combine(directory, "event_balances")
        |> getJsonFiles
        |> Seq.map (fun x -> (x, File.ReadAllText(x) |> replace))
        |> Seq.iter File.WriteAllText
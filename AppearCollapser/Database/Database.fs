namespace AppearCollapser.Database

open System
open System.Collections.Generic

type Database (directory:string) =
    do printfn "%s %s" "db start load" (DateTime.Now.ToString "HH:mm:ss")
    let _appears, _tables = Loader.load directory
    
    do printfn "%s %s" "db end load" (DateTime.Now.ToString "HH:mm:ss")
    
    member this.appears:IReadOnlyCollection<Appear> = _appears.Values
    
    member this.tables = _tables |> Seq.map (fun (KeyValue(k,v)) -> (k,v))
    member this.getAppearByName = _appears.GetValueOrDefault
    
    member this.removeAppear appearIdent =
        try 
            _appears.Remove(appearIdent) |> ignore
            Appear.remove directory appearIdent
        with | e ->  printf $"[Appear remove error {appearIdent}]: %s{e.Message}"
    
    member this.removeRow table row =
        try
            _tables[table] <- _tables[table] |> List.filter ((<>) row)
            Table.removeRow directory table row
        with | e -> printf $"[Row in {table} table remove error]: %s{e.Message}"
    
    member this.addRow table row =
        try
            _tables[table] <- row::_tables[table]
            Table.addRow directory table row
        with | e -> printf $"[Row in {table} table add error]: %s{e.Message}"
    
    member this.fixBalances appear =
        Balance.fix directory appear
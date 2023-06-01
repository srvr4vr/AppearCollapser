namespace AppearCollapser.Database

open System.Collections.Generic
open Microsoft.Extensions.Logging
open Functions

type Database (logger:ILogger, directory:string) =
    
    do logger.LogInformation("db start load")
    let _appears, _tables = Loader.load directory
    
    do logger.LogInformation("db end load")
    
    member this.appears:IReadOnlyCollection<Appear> = _appears.Values
    
    member this.tables = _tables |> Seq.map toTuple
    member this.getAppearByName = _appears.GetValueOrDefault
    
    member this.removeAppear appearIdent =
        try 
            _appears.Remove(appearIdent) |> ignore
            Appear.remove directory appearIdent
        with | e -> logger.LogError(e, "[Appear remove error {Appear}]", appearIdent)
    
    member this.removeRow table row =
        try
            _tables[table] <- _tables[table] |> List.filter ((<>) row)
            Table.removeRow directory table row
        with | e -> logger.LogError(e, "[Row in {Table} table remove error]", table)
    
    member this.addRow table row =
        try
            _tables[table] <- row::_tables[table]
            Table.addRow directory table row
        with | e -> logger.LogError(e, "[Row in {Table} table add error]", table)
    
    member this.fixBalances appear =
        Balance.fix appear directory
namespace AppearCollapser.Database

open System.IO
open Functions
open System.Text.RegularExpressions

[<RequireQualifiedAccess>]
module Balance =
    [<Literal>]
    let private eventBalancerDirectory = "event_balances"
    
    let private createRegex appear =
        $"(\"appear\":\\s+\")({appear.ident})(\")" |> Regex
    
    let private replacePattern =
        JsonHelper.getReplacePattern Appear.defaultName
        
    let private getEventBalancerPath root =
        Path.combine root eventBalancerDirectory

    let private replace (regex:Regex) =
        JsonHelper.replace regex replacePattern
        
    let private pathAndData =
        mapTo (id, File.ReadAllText)
    
    let private fixAppear =
        createRegex
        >> replace
        >> mapSnd

    let fix appear =
        getEventBalancerPath
        >> JsonHelper.getFiles
        >> Seq.map pathAndData
        >> Seq.map (fixAppear appear)
        >> Seq.iter File.WriteAllText
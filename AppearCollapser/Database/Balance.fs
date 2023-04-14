namespace AppearCollapser.Database

open System.IO
open System.Text.RegularExpressions

module Balance =
    [<Literal>]
    let private eventBalancerDirectory = "event_balances"
    
    let private createPattern appear =
        $"(\"appear\":\\s+\")({appear.ident})(\")"
        
    let private replacePattern =
        $"$1{Appear.defaultAppear}$3"

    let fix directory appear =
        let replacer =
            (createPattern appear |> Regex, replacePattern)
            ||> JsonHelper.replace
            
        Path.Combine(directory, eventBalancerDirectory)
        |> JsonHelper.getFiles
        |> Seq.map (fun x -> (x, File.ReadAllText(x) |> replacer))
        |> Seq.iter File.WriteAllText
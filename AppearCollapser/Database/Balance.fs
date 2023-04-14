namespace AppearCollapser.Database

open System.IO
open System.Text.RegularExpressions

module Balance =
    [<Literal>]
    let private eventBalancerDirectory = "event_balances"

    let fix directory appear =
        let replacer = 
            let regex = Regex $"(\"appear\":\\s+\")({appear.ident})(\")"
            JsonHelper.replace regex $"$1{Appear.defaultAppear}$3"
            
        Path.Combine(directory, eventBalancerDirectory)
        |> JsonHelper.getFiles
        |> Seq.map (fun x -> (x, File.ReadAllText(x) |> replacer))
        |> Seq.iter File.WriteAllText
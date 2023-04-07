namespace AppearCollapser.Database

open System.IO
open System.Text.RegularExpressions

module Balance =
    [<Literal>]
    let private eventBalancerDirectory = "event_balances"

    let private replace (regex:Regex) str =
        regex.Replace(str, $"$1{Appear.defaultAppear}$3")
        
    let fix directory appear =
        let regex = Regex $"(\"appear\":\\s+\")({appear.ident})(\")"
            
        Path.Combine(directory, eventBalancerDirectory)
        |> JsonHelper.getFiles
        |> Seq.map (fun x -> (x, File.ReadAllText(x) |> replace regex))
        |> Seq.iter File.WriteAllText
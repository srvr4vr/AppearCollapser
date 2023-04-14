namespace AppearCollapser.Database

open System.IO
open System.Text.RegularExpressions
open FSharp.Json

module JsonHelper =
    let private appearIdentRegex = Regex (@"(""appearIdent"": "")(\w+)("")", RegexOptions.Compiled)
    
    let private jsonConfig = JsonConfig.create(deserializeOption = DeserializeOption.RequireNull)
    
    let replace (regex:Regex) (replacePattern:string) data =
        regex.Replace(data, replacePattern)
    
    let replaceAppear (appearIdent:string) =
        replace appearIdentRegex $@"$1{appearIdent}$3"

    let loadJson<'a> =
        File.ReadAllText >> Json.deserialize<'a>
    
    let loadJsonEx<'a> =
        File.ReadAllText >> Json.deserializeEx<'a> jsonConfig
        
    let getFiles directory =
        Directory.EnumerateFiles(directory, "*.json")
        
    let removeFromDisc directory id =
        Path.Combine(directory, $"{id}.json")
        |> File.Delete

    let writeToDisk directory id data =
        (Path.Combine(directory, $"{id}.json"), data)
        |> File.WriteAllText
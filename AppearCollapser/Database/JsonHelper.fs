namespace AppearCollapser.Database

open System.IO
open System.Text.RegularExpressions
open FSharp.Json

module JsonHelper =
    let private addExt x = $"{x}.json"
    
    let private allMask = addExt "*"
    
    let private appearIdentRegex = Regex (@"(""appearIdent"": "")(\w+)("")", RegexOptions.Compiled)
    
    let private jsonConfig = JsonConfig.create(deserializeOption = DeserializeOption.RequireNull)

    let private getFilepath directory =
        Path.combine directory >> addExt
    
    let replace (regex:Regex) (replacePattern:string) data =
        regex.Replace(data, replacePattern)
    
    let getReplacePattern (x:string) =
        $@"$1{x}$3"
        
    let replaceAppear =
       getReplacePattern
       >> replace appearIdentRegex

    let loadJson<'a> =
        File.ReadAllText >> Json.deserialize<'a>
    
    let loadJsonEx<'a> =
        File.ReadAllText >> Json.deserializeEx<'a> jsonConfig
        
    let getFiles directory =
        Directory.EnumerateFiles(directory, allMask)
        
    let removeFromDisc directory =
        getFilepath directory  
        >> File.Delete
        
    let writeToDisk directory id data =
        (getFilepath directory id, data)
        |> File.WriteAllText
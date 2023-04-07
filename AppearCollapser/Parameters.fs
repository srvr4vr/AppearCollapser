module AppearCollapser.Parameters

open System
open Microsoft.FSharp.Core
open AppearCollapser.Git
open System.Globalization
open Argu
open AppearCollapser.Infrastructure

type Parameters =
    {
        directory: string
        git: Task option
        date: DateTime
        startAppear: string option
        limit: int option 
    }

let private toParams (arguments:ParseResults<Arguments>) =
    let parseDate ds =
        DateTime.ParseExact (ds, "dd.MM.yyyy", CultureInfo.CurrentCulture)
    
    let date =
        arguments.TryGetResult End_Date
        |> Option.fold (fun _ -> parseDate) DateTime.Now
    
    let git =
        arguments.TryGetResult Git
        |> Option.fold (fun _ -> Task >> Some) None
        
    let directory =
        arguments.TryGetResult Lib_Directory
        |> Option.fold (fun _ -> id) Environment.CurrentDirectory
        
    let appear =
        arguments.TryGetResult Start_Appear
        |> Option.fold (fun _ -> Some) None
    
    let limit =
        arguments.TryGetResult Limit
        |> Option.fold (fun _ -> Some) None
                      
    { directory = directory; git = git; date = date; startAppear = appear; limit = limit }
    
let createFromArguments (arg:string[]) =
    ArgumentParser.Create<Arguments>().Parse(arg)
    |> toParams
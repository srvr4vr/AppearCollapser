namespace AppearCollapser.Database

open System
open System.Collections.Generic
open System.IO
open FSharp.Collections.ParallelSeq

type Appear = {
    ident: string
    startDate: DateTime
}

module Appear = 
    [<Literal>]
    let appearDirectoryName = "appear"

    [<Literal>]
    let defaultAppear = "default"

    let private getAppearDirectory d =
        Path.Combine(d, appearDirectoryName)

    let load =
        getAppearDirectory
        >> JsonHelper.getFiles
        >> PSeq.map JsonHelper.loadJson
        >> PSeq.map (fun x -> (x.ident, x))
        >> dict
        >> Dictionary
        
    let remove directory appear =
        (getAppearDirectory directory, appear)
        ||> JsonHelper.removeFromDisc
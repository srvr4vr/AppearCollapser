namespace AppearCollapser.Database

open System
open System.Collections.Generic
open FSharp.Collections.ParallelSeq

type Appear = {
    ident: string
    startDate: DateTime
}

module Appear = 
    [<Literal>]
    let directoryName = "appear"

    [<Literal>]
    let defaultName = "default"

    let private getAppearDirectory root =
        Path.combine root directoryName

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
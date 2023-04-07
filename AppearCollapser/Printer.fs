module AppearCollapser.Printer

open System
open AppearCollapser.Database

let print (completedAppear: string list, nextAppear: Appear option) =
    printf $"Done: {String.Join(' ', completedAppear)}."
    
    match nextAppear with
    | Some x ->  printf $"Next appear {x.ident}"
    | None -> ()
    

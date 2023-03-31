open AppearCollapser

[<EntryPoint>]
let main args =
    args
    |> Parameter.createFromArguments
    |> Collapser.collapse
    |> Printer.print
    0
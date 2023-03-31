open AppearCollapser

[<EntryPoint>]
let main args =
    args
    |> Parameters.createFromArguments
    |> Collapser.collapse
    |> Printer.print
    0
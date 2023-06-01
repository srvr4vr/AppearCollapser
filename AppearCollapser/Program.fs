open AppearCollapser
open Microsoft.Extensions.Logging

[<EntryPoint>]
let main args =
    let logger = LoggerFactory
                     .Create(fun builder -> builder.AddSimpleConsole(fun x -> x.TimestampFormat <- "[dd.MM.yyyy HH:mm:ss]").SetMinimumLevel(LogLevel.Information) |> ignore)
                     .CreateLogger()
                     
    args
    |> Parameters.createFromArguments
    |> Collapser.run logger
    |> Printer.print logger
    0
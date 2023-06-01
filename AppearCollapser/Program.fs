open AppearCollapser
open Microsoft.Extensions.Logging

[<EntryPoint>]
let main args =
    let logger = LoggerFactory.Create(fun builder -> builder.AddConsole().SetMinimumLevel(LogLevel.Debug) |> ignore).CreateLogger()
    args
    |> Parameters.createFromArguments
    |> Collapser.run logger
    |> Printer.print logger
    0
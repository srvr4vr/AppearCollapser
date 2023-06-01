namespace AppearCollapser.Infrastructure

type Error =
    | LibraryNotFound of string
    | UnhandledException of System.Exception
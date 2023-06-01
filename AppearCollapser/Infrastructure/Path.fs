[<RequireQualifiedAccess>]
module Path

open Functions
open System.IO

let combine =
    curry Path.Combine

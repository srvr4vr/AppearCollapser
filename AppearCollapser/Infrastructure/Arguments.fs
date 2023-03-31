module AppearCollapser.Infrastructure

open Argu

type Arguments =
    | Lib_Directory of path:string
    | Git of task:string
    | End_Date of dateString:string
    | Start_Appear of appear:string
    | Limit of int
    
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Lib_Directory _ -> "specify a directory with library JSONs."
            | Git _ -> "save to git with task message"
            | End_Date _ -> "date until which appears will be cleared (dd.MM.yyyy)"
            | Start_Appear _ -> "appear from which processing starts"
            | Limit _ -> "number of processed appears"
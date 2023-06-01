module AppearCollapser.Git

open AppearCollapser.Command

type Task = Task of id:string
 
let private execute command =
    command
    |> Async.RunSynchronously
    |> ignore
    
let add directory =
    executeShellCommand directory "git add --all"
    |> execute

let commit directory taskId=
    ["git commit -a"; $@"""-m https://nexters.atlassian.net/browse/{taskId} del"""]
    |> String.concat " "
    |> executeShellCommand directory
    |> execute

let addAndCommit directory (Task id) =
    add directory
    id |> commit directory
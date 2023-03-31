module AppearCollapser.Git

open AppearCollapser.Command

type Task = Task of string
 
let private execute command =
    command
    |> Async.RunSynchronously
    |> ignore
    
let add directory =
    executeShellCommand directory "git add --all"
    |> execute

let commit (task: Task) directory =
    let (Task taskName) = task
    ["git commit -a"; $@"""-m https://nexters.atlassian.net/browse/{taskName} del"""]
    |> String.concat " "
    |> executeShellCommand directory
    |> execute

let addAndCommit (task: Task) directory =
    add directory
    commit task directory
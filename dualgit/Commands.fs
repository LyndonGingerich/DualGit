module DualGit.Commands

open Fli

let rec iter commands =
    match commands with
    | [] -> true
    | command: ExecContext :: rest ->
        if (Command.execute command).ExitCode <> 0 then
            false
        else
            iter rest

let createGitCommand (args: string list) =
    cli {
        Exec "git"
        Arguments args
    }

let iterGit = List.map createGitCommand >> iter

let executeGit args =
    let output = createGitCommand args |> Command.execute
    output.ExitCode = 0

let queryGit args =
    let output = createGitCommand args |> Command.execute
    if output.ExitCode = 0 then output.Text else None

let getCurrentCommit () = queryGit [ "rev-parse"; "HEAD" ]
let getCurrentBranch () = queryGit [ "name-rev"; "--name-only"; "HEAD" ]
let checkObjectExistence object = executeGit [ "rev-parse"; "--verify"; object ]

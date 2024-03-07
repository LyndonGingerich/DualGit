module DualGit.Commands

open Fli

let rec iter commands =
    match commands with
    | [] -> None
    | command: ExecContext :: rest ->
        let output = Command.execute command
        if output.ExitCode <> 0 then
            output.Error
        else
            iter rest

let private createGitCommand (args: string list) =
    cli {
        Exec "git"
        Arguments args
    }

let private iterGit = List.map createGitCommand >> iter

let private executeGit args =
    let output = createGitCommand args |> Command.execute
    if output.ExitCode = 0 then None else output.Error

let private queryGit args =
    let output = createGitCommand args |> Command.execute
    if output.ExitCode = 0 then true, output.Text else false, output.Error

let getCurrentCommit () = queryGit [ "rev-parse"; "HEAD" ]
let getCurrentBranch () = queryGit [ "name-rev"; "--name-only"; "HEAD" ]
let checkObjectExistence object = executeGit [ "rev-parse"; "--verify"; object ] |> Option.isNone
let checkIsAncestor child parent = executeGit [ "merge-base"; "--is-ancestor"; parent; child ] |> Option.isNone
let createBranch branch = executeGit [ "branch"; branch ]

let getOrCreateChild parent child =
    if checkObjectExistence child then
        if checkIsAncestor child parent |> not then
            Some $"{child} is not a descendant of base object {parent}."
        else
            None
    else
        createBranch child

let smartCheckout branch =
    [ [ "stash"; "push" ]
      [ "checkout"; branch ]
      [ "stash"; "pop" ] ]
    |> iterGit

let commit args = executeGit ("commit" :: args)

module DualGit.Commands

open Fli

let private createGitCommand (args: string list) =
    cli {
        Exec "git"
        Arguments args
    }

let private executeGit args =
    let output = createGitCommand args |> Command.execute
    if output.ExitCode = 0 then None else output.Error

let private queryGit args =
    let output = createGitCommand args |> Command.execute

    if output.ExitCode = 0 then
        Result.Ok output.Text
    else
        Result.Error output.Error

let rec private iterGit commands =
    match commands with
    | [] -> None
    | command :: rest ->
        match executeGit command with
        | Some error -> Some error
        | None -> iterGit rest

let getCurrentCommit () = queryGit [ "rev-parse"; "HEAD" ]

let getCurrentBranch () =
    queryGit [ "name-rev"; "--name-only"; "HEAD" ]

let checkObjectExistence object =
    executeGit [ "rev-parse"; "--verify"; object ] |> Option.isNone

let checkIsAncestor child parent =
    executeGit [ "merge-base"; "--is-ancestor"; parent; child ] |> Option.isNone

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
    [ [ "stash"; "push" ]; [ "checkout"; branch ]; [ "stash"; "pop" ] ] |> iterGit

let commit args =
    [ [ "add"; "*" ]; "commit" :: args ] |> iterGit

let merge args into from =
    let needsCheckout =
        match getCurrentBranch () with
        | Result.Ok(Some branch) -> branch <> into
        | _ -> true

    match if not needsCheckout then None else smartCheckout into with
    | Some error -> Some error
    | None -> executeGit ([ "merge"; from ] @ args)

module DualGit.Program

open Fli

let print = printfn "%s"

[<EntryPoint>]
let main args =
    let config = Config.get ()

    match Array.toList args with
    | "status" :: rest ->
        match rest with
        | [] ->
            match config with
            | None ->
                print "No dualgit workflow is in progress."
                0
            | Some config ->
                print $"Base commit: {config.``base``}"
                print $"Feature branch: {config.feature}"
                print $"Refactor branch: {config.refactor}"
                if config.split_commits.Count = 0 then
                    print "No \"split\" operation is in progress."
                else
                    print "A \"split\" operation is in progress."
                0
        | _ ->
            print "\"dualgit status\" takes no arguments."
            1
    | "init" :: rest ->
        if config.IsSome then
            print "Another dualgit workflow is already running."
            1
        else
            match rest with
            | [] ->
                let baseCommitOutput =
                    cli {
                        Exec "git"
                        Arguments [ "rev-parse"; "HEAD" ]
                    }
                    |> Command.execute
                match baseCommitOutput with
                | { ExitCode = 0; Text = Some baseCommit } ->
                    Config.initialize baseCommit
                    0
                | _ ->
                    print "Git failed to get the current commit."
                    0
            | _ ->
                print "\"dualgit init\" takes no arguments."
                1
    | "set" :: rest ->
        match config with
        | None ->
            print "No dualgit workflow is in progress. Please call \"dualgit init\"."
            1
        | Some config ->
            let usage = "Usage: \"dualgit set <key> <value>\""
            let tooManyArgs () =
                print $"\"dualgit set\" takes only two arguments. {usage}"
                1
            match rest with
            | "feature" :: featureBranch :: rest' ->
                if rest'.IsEmpty then
                    config.feature <- featureBranch
                    Config.save config
                    0
                else
                    tooManyArgs ()
            | "refactor" :: refactorBranch :: rest' ->
                if rest'.IsEmpty then
                    config.refactor <- refactorBranch
                    Config.save config
                    0
                else
                    tooManyArgs ()
            | _ ->
                print "Unrecognized command."
                print usage
                1
    | _ ->
        print "Command not recognized."
        1

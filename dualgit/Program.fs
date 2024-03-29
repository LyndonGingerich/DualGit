﻿module DualGit.Program

open System.IO

let print = printfn "%s"

let checkBranchesSet (config: Config.Config) =
    if config.feature = "" then
        Some "No feature branch is set.\nPlease call \"dualgit set feature <feature branch>\"."
    elif config.refactor = "" then
        Some "No refactor branch is set.\nPlease call \"dualgit set refactor <refactor branch>\"."
    else
        None

[<EntryPoint>]
let main args =
    let config = Config.get ()

    let noCurrentWorkflow =
        "No dualgit workflow is in progress. Please call \"dualgit init\"."

    match Array.toList args with
    | "status" :: rest ->
        match rest with
        | [] ->
            match config with
            | None ->
                print "No dualgit workflow is in progress."
                0
            | Some config ->
                print (Config.getStatus config)
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
                let failureMessage = "Git failed to get the current commit."

                match Commands.getCurrentCommit () with
                | Result.Ok(Some baseCommit) ->
                    Config.initialize baseCommit
                    0
                | Result.Ok None ->
                    print failureMessage
                    1
                | Result.Error error ->
                    print (Option.defaultValue failureMessage error)
                    1
            | [ baseCommit ] ->
                if Commands.checkObjectExistence baseCommit then
                    Config.initialize baseCommit
                    0
                else
                    print $"Git did not find object {baseCommit}."
                    1
            | _ ->
                print "\"dualgit init\" takes no arguments."
                1
    | "set" :: rest ->
        match config with
        | None ->
            print noCurrentWorkflow
            1
        | Some config ->
            let usage = "Usage: \"dualgit set <key> <value>\""

            let tooManyArgs () =
                print $"\"dualgit set\" takes only two arguments. {usage}"
                1

            let setBranch branch =
                Commands.getOrCreateChild config.``base`` branch

            match rest with
            | "feature" :: featureBranch :: rest' ->
                if rest'.IsEmpty then
                    match setBranch featureBranch with
                    | Some error ->
                        print error
                        1
                    | None ->
                        config.feature <- featureBranch
                        Config.save config
                        0
                else
                    tooManyArgs ()
            | "refactor" :: refactorBranch :: rest' ->
                if rest'.IsEmpty then
                    match setBranch refactorBranch with
                    | Some error ->
                        print error
                        1
                    | None ->
                        config.refactor <- refactorBranch
                        Config.save config
                        0
                else
                    tooManyArgs ()
            | _ ->
                print "Unrecognized command."
                print usage
                1
    | "commit" :: rest ->
        match config with
        | None ->
            print noCurrentWorkflow
            1
        | Some config ->
            match checkBranchesSet config with
            | Some error ->
                print error
                1
            | None ->
                let usage = "Usage: dualgit commit [feature|refactor] <commit args>"

                match rest with
                | [] ->
                    print usage
                    1
                | branchId :: commitArgs ->
                    match
                        match branchId with
                        | "feature" -> Some config.feature
                        | "refactor" -> Some config.refactor
                        | _ -> None
                    with
                    | None ->
                        print "\"dualgit commit\" takes either \"feature\" or \"refactor\" as its second argument."
                        print usage
                        1
                    | Some branch ->
                        match Commands.getCurrentBranch () with
                        | Result.Error error ->
                            print (Option.defaultValue "Getting the current branch failed." error)
                            1
                        | Result.Ok(Some currentBranch) ->
                            match
                                if currentBranch = branch then
                                    None
                                else
                                    Commands.smartCheckout branch
                            with
                            | Some error ->
                                print error
                                1
                            | None ->
                                match Commands.commit commitArgs with
                                | Some error ->
                                    print error
                                    1
                                | None -> 0
                        | Result.Ok None ->
                            print "Git failed to find the current branch."
                            1
    | "update" :: rest ->
        match config with
        | None ->
            print noCurrentWorkflow
            1
        | Some config ->
            match checkBranchesSet config with
            | Some error ->
                print error
                1
            | None ->
                match Commands.merge rest config.feature config.refactor with
                | Some error ->
                    print error
                    1
                | None -> 0
    | "reset" :: rest ->
        if rest.IsEmpty then
            File.Delete Config.configFile
            0
        else
            print "\"dualgit reset\" does not take arguments."
            1
    | "switch" :: rest ->
        match config with
        | None ->
            print noCurrentWorkflow
            1
        | Some config ->
            match checkBranchesSet config with
            | Some error ->
                print error
                1
            | None ->
                if not rest.IsEmpty then
                    print "\"dualgit switch\" takes no arguments."
                    1
                else
                    match Commands.getCurrentBranch () with
                    | Result.Error error ->
                        print (Option.defaultValue "Could not get current branch" error)
                        1
                    | Result.Ok currentBranch ->
                        match
                            if currentBranch = Some config.feature then
                                Some config.refactor
                            elif currentBranch = Some config.refactor then
                                Some config.feature
                            else
                                None
                        with
                        | None ->
                            print $"{currentBranch} is neither \"feature\" nor \"refactor\"."
                            1
                        | Some branch ->
                            match Commands.smartCheckout branch with
                            | Some error ->
                                print error
                                1
                            | None -> 0
    | _ ->
        print "Command not recognized."
        1

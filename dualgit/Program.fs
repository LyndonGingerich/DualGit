module DualGit.Program

let print = printfn "%s"

[<EntryPoint>]
let main args =
    let config = Config.get ()

    let noCurrentWorkflow = "No dualgit workflow is in progress. Please call \"dualgit init\"."
    
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
                match Commands.getCurrentCommit () with
                | Some baseCommit ->
                    Config.initialize baseCommit
                    0
                | None ->
                    print "Git failed to get the current commit."
                    0
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
                match Commands.getOrCreateChild config.``base`` branch with
                | Some error ->
                    print error
                    1
                | None ->
                    0

            match rest with
            | "feature" :: featureBranch :: rest' ->
                if rest'.IsEmpty then
                    setBranch featureBranch
                else
                    tooManyArgs ()
            | "refactor" :: refactorBranch :: rest' ->
                if rest'.IsEmpty then
                    setBranch refactorBranch
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
            if config.feature = "" then
                print "No feature branch is set."
                print "Please call \"dualgit set feature <feature branch>\"."
                1
            elif config.refactor = "" then
                print "No refactor branch is set."
                print "Please call \"dualgit set refactor <refactor branch>\"."
                1
            else
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
                        | None ->
                            print "Getting the current branch failed."
                            1
                        | Some currentBranch ->
                            if
                                currentBranch <> branch
                                && [ [ "stash"; "push" ]
                                     [ "checkout"; branch ]
                                     [ "stash"; "pop" ] ]
                                   |> Commands.iterGit
                                   |> not
                            then
                                print "Checkout failed."
                                1
                            elif
                                Commands.executeGit ("commit" :: commitArgs)
                                |> not
                            then
                                print "Commit failed."
                                1
                            else
                                0
    | _ ->
        print "Command not recognized."
        1

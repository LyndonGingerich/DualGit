module DualGit.Config

open FSharp.Configuration
open System.IO

let configFile = ".dualgit"

type Config = YamlConfig<"sample.yaml">

let save (config: Config) = config.Save configFile

let initialize baseCommit =
    let config = Config()
    config.``base`` <- baseCommit
    config.feature <- ""
    config.refactor <- ""
    config.split_commits <- [||]
    save config

let get () =
    if File.Exists configFile then
        let config = Config()
        config.Load configFile
        config |> Some
    else
        None

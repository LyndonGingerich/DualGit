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

let getStatus (config: Config) =
    [ $"Base commit: {config.``base``}"
      $"Feature branch: {config.feature}"
      $"Refactor branch: {config.refactor}"

      if config.split_commits.Count = 0 then
          "No \"split\" operation is in progress."
      else
          "A \"split\" operation is in progress." ]
    |> String.concat "\n"

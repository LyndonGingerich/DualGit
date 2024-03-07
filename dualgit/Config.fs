module DualGit.Config

open FSharp.Configuration
open System.IO

let configFile = ".dualgit"

type Config = YamlConfig<"sample.yaml">

let get () =
    if File.Exists configFile then
        let config = Config()
        config.Load configFile
        config |> Some
    else
        None 

let save (config: Config) = config.Save configFile

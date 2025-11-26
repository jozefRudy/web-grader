module Config.Google

open System.ComponentModel.DataAnnotations

type GoogleConfig() =
    static member val SectionName = "Google" with get

    [<Required>]
    member val ApiKey: string = "" with get, set
    
    [<Required>]
    member val SearchEngineId: string = "" with get, set

module Config.LiteLLM

open System.ComponentModel.DataAnnotations

type LiteLLMConfig() =
    static member val SectionName = "LiteLLM" with get

    [<Required>]
    member val Uri: string = "" with get, set

    [<Required>]
    member val Model: string = "" with get, set

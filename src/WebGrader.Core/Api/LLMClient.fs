module Api.LLMClient

open System
open System.Net.Http
open System.Net.Http.Json
open System.Threading
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open Config.LiteLLM

type ChatMessage = { Role: string; Content: string }

type ChatRequest = {
    Model: string
    Messages: ChatMessage list
    Temperature: float
}

type ChatResponse = {
    Id: string
    Choices: {| Message: ChatMessage |} list
}

type ModelsResponse = { Data: {| Id: string |} list }

type LiteLLMErrorResponse = {
    Error: {| Message: string; Code: string |}
}

type LLMClient(client: HttpClient, config: IOptions<LiteLLMConfig>) =
    let config = config.Value

    member this.GetModels(ct: CancellationToken) =
        taskResult {
            try
                let! response = client.GetFromJsonAsync<ModelsResponse>("/v1/models", ct)
                return response.Data |> List.map (fun m -> m.Id)
            with ex ->
                return! Error ex
        }

    member this.Chat(userMessage: string, ct: CancellationToken, ?additionalParams: Map<string, obj>) =
        taskResult {
            try
                let request =
                    additionalParams
                    |> Option.defaultValue Map.empty
                    |> Map.add "model" (box config.Model)
                    |> Map.add "messages" (box [ { Role = "user"; Content = userMessage } ])

                let! response = client.PostAsJsonAsync("/chat/completions", request, ct)

                if not response.IsSuccessStatusCode then
                    let! errorResult = response.Content.ReadFromJsonAsync<LiteLLMErrorResponse> ct
                    return! Error(exn $"LiteLLM API error {response.StatusCode}: {errorResult.Error.Message}")
                else
                    let! result = response.Content.ReadFromJsonAsync<ChatResponse> ct

                    return!
                        result.Choices
                        |> List.tryHead
                        |> Option.map (fun x -> x.Message.Content)
                        |> Result.requireSome (exn "empty response returned")
            with ex ->
                return! Error ex
        }

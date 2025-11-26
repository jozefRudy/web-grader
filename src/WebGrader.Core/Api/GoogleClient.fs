module Api.GoogleClient

open System
open System.Net.Http
open System.Net.Http.Json
open System.Threading
open FsToolkit.ErrorHandling
open Config.Google

type SearchResult = {
    title: string
    link: string
    snippet: string
}

type SearchInformation = {
    totalResults: string
    searchTime: float option
}

type SearchResponse = {
    items: SearchResult list option
    searchInformation: SearchInformation option
}

open Microsoft.Extensions.Options

type GoogleClient(client: HttpClient, config: IOptions<GoogleConfig>) =
    let config = config.Value

    member this.SearchFull (query: string) (ct: CancellationToken) =
        taskResult {
            try
                let encodedQuery = Uri.EscapeDataString query
                let url = $"?key={config.ApiKey}&cx={config.SearchEngineId}&q={encodedQuery}"

                let! response = client.GetAsync(url, ct)

                if not response.IsSuccessStatusCode then
                    return! Error(exn $"Google API error {response.StatusCode}")
                else
                    let! result = response.Content.ReadFromJsonAsync<SearchResponse> ct
                    return result
            with ex ->
                return! Error ex
        }

    member this.Search (query: string) (ct: CancellationToken) =
        taskResult {
            let! response = this.SearchFull query ct
            return response.items |> Option.defaultValue []
        }

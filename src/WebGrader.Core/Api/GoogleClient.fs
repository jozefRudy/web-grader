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

type SearchResponse = {
    items: SearchResult list option
}

type GoogleClient(client: HttpClient, config: GoogleConfig) =
    
    member this.Search (query: string) (ct: CancellationToken) =
        taskResult {
            try
                let encodedQuery = Uri.EscapeDataString(query)
                let url = $"?key={config.ApiKey}&cx={config.SearchEngineId}&q={encodedQuery}"
                
                let! response = client.GetAsync(url, ct)
                
                if not response.IsSuccessStatusCode then
                    return! Error(exn $"Google API error {response.StatusCode}")
                else
                    let! result = response.Content.ReadFromJsonAsync<SearchResponse>(ct)
                    return result.items |> Option.defaultValue []
            with ex ->
                return! Error ex
        }

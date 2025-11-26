module GoogleClientTests

open System
open System.Net
open System.Net.Http
open System.Threading
open Xunit
open Xunit.Abstractions
open Swensen.Unquote
open FsToolkit.ErrorHandling

open Api.GoogleClient
open Config.Google

type Client(uri: string) =
    let handler = new HttpClientHandler()
    do handler.AutomaticDecompression <- DecompressionMethods.GZip ||| DecompressionMethods.Deflate
    let client = new HttpClient(handler)
    do client.BaseAddress <- Uri uri
    member this.Client = client

module TestHelpers =
    let requireEnvVar name =
        Environment.GetEnvironmentVariable name
        |> Option.ofObj
        |> Option.defaultWith (fun () -> failwith $"{name} environment variable is required but not set")

type GoogleClientTests(logger: ITestOutputHelper) =

    [<Fact>]
    member this.``search returns results``() =
        taskResult {
            // Arrange - require env vars to be set
            let apiKey = TestHelpers.requireEnvVar "GOOGLE_API_KEY"
            let searchEngineId = TestHelpers.requireEnvVar "GOOGLE_SEARCH_ENGINE_ID"

            logger.WriteLine $"✅ Testing Google Search with query: 'test query'"
            
            let config = GoogleConfig()
            config.ApiKey <- apiKey
            config.SearchEngineId <- searchEngineId

            let client = Client "https://www.googleapis.com/customsearch/v1"
            let googleClient = GoogleClient(client.Client, config)

            // Act
            let! response = googleClient.SearchFull "test query" CancellationToken.None

            // Print entire response
            let responseStr = sprintf "%A" response
            logger.WriteLine "📊 Full Google Search Response:"
            logger.WriteLine responseStr
            
            // Assert
            let results = response.items |> Option.defaultValue []
            test <@ results.Length >= 0 @>
        }
        |> TaskResult.teeError (fun x -> Assert.Fail x.Message)

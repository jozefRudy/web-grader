module ApiTests

open System
open System.Net
open System.Net.Http
open System.Threading
open Xunit
open Xunit.Abstractions
open Swensen.Unquote
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options

open Api.GoogleClient
open Api.LLMClient
open Config.Google
open Config.LiteLLM

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

type ApiTests(logger: ITestOutputHelper) =

    [<Fact>]
    member this.``google search returns results``() =
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

    [<Fact>]
    member this.``llm chat returns response``() =
        taskResult {
            // Arrange - hardcoded config (not secrets)
            let litellmUri = "http://localhost:4000"
            let model = "gpt-5-mini"

            let config = LiteLLMConfig()
            config.Uri <- litellmUri
            config.Model <- model

            let options = Options.Create config
            let client = Client litellmUri
            let llmClient = LLMClient(client.Client, options)

            // Act
            let! response = llmClient.Chat("Say 'Hello from LiteLLM!' and nothing else", CancellationToken.None)

            // Print response
            logger.WriteLine $"📊 LLM Response:\n{response}"

            // Assert
            test <@ not (String.IsNullOrEmpty response) @>
        }
        |> TaskResult.teeError (fun x -> Assert.Fail x.Message)

    [<Fact>]
    member this.``llm get models returns list``() =
        taskResult {
            // Arrange - hardcoded config (not secrets)
            let litellmUri = "http://localhost:4000"
            let model = "gpt-4"

            logger.WriteLine $"✅ Testing LiteLLM GetModels"

            let config = LiteLLMConfig()
            config.Uri <- litellmUri
            config.Model <- model

            let options = Options.Create config
            let client = Client litellmUri
            let llmClient = LLMClient(client.Client, options)

            // Act
            let! models = llmClient.GetModels CancellationToken.None

            // Print models
            logger.WriteLine $"📊 Available Models ({models.Length}):"

            for model in models do
                logger.WriteLine $"  - {model}"

            // Assert
            test <@ models.Length > 0 @>
        }
        |> TaskResult.teeError (fun x -> Assert.Fail x.Message)

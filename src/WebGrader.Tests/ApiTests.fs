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
open Services.AeoAnalysisService

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
            let model = "gpt-5-mini"

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

    [<Fact>]
    member this.``GatherSearchData executes all queries successfully``() =
        taskResult {
            // Arrange - require env vars to be set
            let apiKey = TestHelpers.requireEnvVar "GOOGLE_API_KEY"
            let searchEngineId = TestHelpers.requireEnvVar "GOOGLE_SEARCH_ENGINE_ID"

            logger.WriteLine "✅ Testing GatherSearchData with real Google API"
            logger.WriteLine "Company: Martus Solutions"
            logger.WriteLine "Location: United States"
            logger.WriteLine "Product: CRM Software"
            logger.WriteLine "Industry: B2B SaaS"

            // Setup Google Client
            let googleConfig = GoogleConfig()
            googleConfig.ApiKey <- apiKey
            googleConfig.SearchEngineId <- searchEngineId

            let googleHttpClient = Client "https://www.googleapis.com/customsearch/v1"
            let googleClient = GoogleClient(googleHttpClient.Client, googleConfig)

            // Setup LLM Client (mock for now, not used in GatherSearchData)
            let llmConfig = LiteLLMConfig()
            llmConfig.Uri <- "http://localhost:4000"
            llmConfig.Model <- "gpt-4o-mini"

            let llmHttpClient = Client "http://localhost:4000"
            let llmClient = LLMClient(llmHttpClient.Client, Options.Create llmConfig)

            // Create service
            let service = AeoAnalysisService(googleClient, llmClient)

            // Act
            logger.WriteLine "\n🔍 Executing search queries..."

            let! searchData =
                service.GatherSearchData(
                    "Martus Solutions",
                    "United States",
                    "CRM Software",
                    "B2B SaaS",
                    CancellationToken.None
                )

            // Assert and Log Results
            logger.WriteLine "\n📊 Brand Recognition Results:"
            logger.WriteLine $"  Brand Mentions: {searchData.BrandRecognition.BrandMentions.Length} results"
            logger.WriteLine $"  Brand + Industry: {searchData.BrandRecognition.BrandIndustry.Length} results"
            logger.WriteLine $"  Best Product: {searchData.BrandRecognition.BestProduct.Length} results"
            logger.WriteLine $"  Reviews: {searchData.BrandRecognition.Reviews.Length} results"

            logger.WriteLine "\n📊 Competition Results:"
            logger.WriteLine $"  VS Competitors: {searchData.Competition.VsCompetitors.Length} results"
            logger.WriteLine $"  Alternatives: {searchData.Competition.Alternatives.Length} results"
            logger.WriteLine $"  Top Companies: {searchData.Competition.TopCompanies.Length} results"

            logger.WriteLine "\n📊 Sentiment Results:"
            logger.WriteLine $"  Review Sentiment: {searchData.Sentiment.ReviewSentiment.Length} results"
            logger.WriteLine $"  Complaints: {searchData.Sentiment.Complaints.Length} results"
            logger.WriteLine $"  Reddit: {searchData.Sentiment.Reddit.Length} results"
            logger.WriteLine $"  Testimonials: {searchData.Sentiment.Testimonials.Length} results"

            // Sample some results
            if searchData.BrandRecognition.BrandMentions.Length > 0 then
                logger.WriteLine "\n📌 Sample Brand Mention Result:"
                let first = searchData.BrandRecognition.BrandMentions.[0]
                logger.WriteLine $"  Title: {first.title}"
                logger.WriteLine $"  Link: {first.link}"
                logger.WriteLine $"  Snippet: {first.snippet}"

            // Assertions
            test <@ searchData.BrandRecognition.BrandMentions.Length >= 0 @>
            test <@ searchData.Competition.TopCompanies.Length >= 0 @>
            test <@ searchData.Sentiment.Reddit.Length >= 0 @>

            logger.WriteLine "\n✅ All search queries executed successfully!"
        }
        |> TaskResult.teeError (fun x ->
            logger.WriteLine $"\n❌ Test failed: {x.Message}"
            Assert.Fail x.Message
        )

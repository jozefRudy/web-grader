module Services.AeoAnalysisService

open Api.GoogleClient
open Api.LLMClient
open Models.AeoModels
open System
open System.Threading
open FsToolkit.ErrorHandling

type AeoAnalysisService(googleClient: GoogleClient, llmClient: LLMClient) =
    
    member private this.BuildAnalysisPrompt(companyName: string, location: string, product: string, industry: string, searchData: {| BrandRecognition: {| BrandMentions: SearchResult list; BrandIndustry: SearchResult list; BestProduct: SearchResult list; Reviews: SearchResult list |}; Competition: {| VsCompetitors: SearchResult list; Alternatives: SearchResult list; TopCompanies: SearchResult list |}; Sentiment: {| ReviewSentiment: SearchResult list; Complaints: SearchResult list; Reddit: SearchResult list; Testimonials: SearchResult list |} |}) =
        
        let brandMentionsSample = 
            searchData.BrandRecognition.BrandMentions 
            |> List.truncate 5 
            |> List.map (fun r -> $"- {r.title}: {r.snippet}")
            |> String.concat "\n"
        
        let topCompaniesSample = 
            searchData.Competition.TopCompanies 
            |> List.truncate 5 
            |> List.map (fun r -> $"- {r.title}: {r.snippet}")
            |> String.concat "\n"
        
        let reviewsSample = 
            searchData.Sentiment.ReviewSentiment 
            |> List.truncate 5 
            |> List.map (fun r -> $"- {r.title}: {r.snippet}")
            |> String.concat "\n"
        
        $"""You are an AEO (Answer Engine Optimization) analyst. Analyze the following search results for {companyName} and provide a comprehensive report.

Company: {companyName}
Location: {location}
Product/Service: {product}
Industry: {industry}

## Brand Recognition Search Results:
Brand Mentions: {searchData.BrandRecognition.BrandMentions.Length} results
{brandMentionsSample}

## Competition Search Results:
Top Companies: {searchData.Competition.TopCompanies.Length} results
{topCompaniesSample}

## Sentiment Search Results:
Reviews: {searchData.Sentiment.ReviewSentiment.Length} results
{reviewsSample}

Please provide a JSON response with the following structure:
{{
  "overallScore": 75,
  "brandRecognitionScore": 15,
  "marketScore": 8,
  "sentimentScore": 35,
  "competitors": [
    {{"name": "Competitor A", "shareOfVoice": 25.5}}
  ],
  "strengths": ["strength1", "strength2"],
  "weaknesses": ["weakness1", "weakness2"],
  "summary": "Executive summary here",
  "brandRecognitionDetails": {{
    "recognitionScore": 82,
    "marketPosition": "Challenger",
    "confidenceLevel": 85,
    "sourceDiversity": 7
  }},
  "marketCompetitionDetails": {{
    "totalMentions": 1247,
    "competitorMentions": {{"Competitor A": 234}},
    "commonComparisonTopics": ["pricing", "features"],
    "marketTrends": ["trend1", "trend2"]
  }},
  "sentimentDetails": {{
    "overallSentiment": 68,
    "positiveFactors": ["factor1"],
    "negativeFactors": ["factor1"],
    "neutralMentions": 23
  }},
  "sourceAnalysis": {{
    "totalSources": 47,
    "topSources": [["reddit.com", 23], ["twitter.com", 18]],
    "sourceDiversity": 8
  }},
  "keyInsights": {{
    "primaryStrengths": ["strength1"],
    "criticalWeaknesses": ["weakness1"],
    "marketOpportunities": ["opportunity1"],
    "competitiveAdvantages": ["advantage1"]
  }}
}}

Return ONLY valid JSON, no additional text."""
    
    member this.GatherSearchData(companyName: string, location: string, product: string, industry: string, ct: CancellationToken) =
        taskResult {
            // First 5 queries in parallel
            let! brandMentions = googleClient.Search $"\"{companyName}\"" ct
            and! brandIndustry = googleClient.Search $"\"{companyName}\" {industry}" ct
            and! bestProduct = googleClient.Search $"best {product} {location}" ct
            and! reviews = googleClient.Search $"\"{companyName}\" reviews" ct
            and! vsCompetitors = googleClient.Search $"\"{companyName}\" vs" ct
            
            // Remaining queries in parallel
            let! alternatives = googleClient.Search $"alternatives to {companyName}" ct
            and! topCompanies = googleClient.Search $"top {industry} companies {location}" ct
            and! reviewSentiment = googleClient.Search $"\"{companyName}\" review" ct
            and! complaints = googleClient.Search $"\"{companyName}\" complaints" ct
            and! reddit = googleClient.Search $"\"{companyName}\" reddit" ct
            and! testimonials = googleClient.Search $"\"{companyName}\" testimonial OR success story" ct
            
            return {|
                BrandRecognition = {| 
                    BrandMentions = brandMentions
                    BrandIndustry = brandIndustry
                    BestProduct = bestProduct
                    Reviews = reviews
                |}
                Competition = {| 
                    VsCompetitors = vsCompetitors
                    Alternatives = alternatives
                    TopCompanies = topCompanies
                |}
                Sentiment = {| 
                    ReviewSentiment = reviewSentiment
                    Complaints = complaints
                    Reddit = reddit
                    Testimonials = testimonials
                |}
            |}
        }
    
    member this.GenerateReport(companyName: string, location: string, product: string, industry: string, ct: CancellationToken) =
        taskResult {
            let! searchData = this.GatherSearchData(companyName, location, product, industry, ct)
            
            // Build prompt with search data
            let prompt = this.BuildAnalysisPrompt(companyName, location, product, industry, searchData)
            
            // Call LLM for analysis
            let! llmResponse = llmClient.Chat(prompt, ct)
            
            // Parse JSON response
            let! report = 
                try
                    let json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(llmResponse)
                    
                    let report: AeoReport = {
                        CompanyName = companyName
                        Location = location
                        Product = product
                        Industry = industry
                        Score = {
                            Overall = json.GetProperty("overallScore").GetInt32()
                            BrandRecognition = json.GetProperty("brandRecognitionScore").GetInt32()
                            MarketScore = json.GetProperty("marketScore").GetInt32()
                            Sentiment = json.GetProperty("sentimentScore").GetInt32()
                        }
                        Competitors = 
                            json.GetProperty("competitors").EnumerateArray()
                            |> Seq.map (fun c -> {
                                Name = c.GetProperty("name").GetString()
                                ShareOfVoice = c.GetProperty("shareOfVoice").GetDouble()
                            })
                            |> Seq.toList
                        Strengths = 
                            json.GetProperty("strengths").EnumerateArray()
                            |> Seq.map (fun s -> s.GetString())
                            |> Seq.toList
                        Weaknesses = 
                            json.GetProperty("weaknesses").EnumerateArray()
                            |> Seq.map (fun w -> w.GetString())
                            |> Seq.toList
                        Summary = json.GetProperty("summary").GetString()
                        AnalysisDate = DateTime.UtcNow
                        BrandRecognitionDetails = 
                            let brd = json.GetProperty("brandRecognitionDetails")
                            {
                                RecognitionScore = brd.GetProperty("recognitionScore").GetInt32()
                                MarketPosition = brd.GetProperty("marketPosition").GetString()
                                ConfidenceLevel = brd.GetProperty("confidenceLevel").GetInt32()
                                SourceDiversity = brd.GetProperty("sourceDiversity").GetInt32()
                            }
                        MarketCompetitionDetails = 
                            let mcd = json.GetProperty("marketCompetitionDetails")
                            {
                                TotalMentions = mcd.GetProperty("totalMentions").GetInt32()
                                CompetitorMentions = 
                                    mcd.GetProperty("competitorMentions").EnumerateObject()
                                    |> Seq.map (fun kv -> (kv.Name, kv.Value.GetInt32()))
                                    |> Map.ofSeq
                                CommonComparisonTopics = 
                                    mcd.GetProperty("commonComparisonTopics").EnumerateArray()
                                    |> Seq.map (fun t -> t.GetString())
                                    |> Seq.toList
                                MarketTrends = 
                                    mcd.GetProperty("marketTrends").EnumerateArray()
                                    |> Seq.map (fun t -> t.GetString())
                                    |> Seq.toList
                            }
                        SentimentDetails = 
                            let sd = json.GetProperty("sentimentDetails")
                            {
                                OverallSentiment = sd.GetProperty("overallSentiment").GetInt32()
                                PositiveFactors = 
                                    sd.GetProperty("positiveFactors").EnumerateArray()
                                    |> Seq.map (fun f -> f.GetString())
                                    |> Seq.toList
                                NegativeFactors = 
                                    sd.GetProperty("negativeFactors").EnumerateArray()
                                    |> Seq.map (fun f -> f.GetString())
                                    |> Seq.toList
                                NeutralMentions = sd.GetProperty("neutralMentions").GetInt32()
                            }
                        SourceAnalysis = 
                            let sa = json.GetProperty("sourceAnalysis")
                            {
                                TotalSources = sa.GetProperty("totalSources").GetInt32()
                                TopSources = 
                                    sa.GetProperty("topSources").EnumerateArray()
                                    |> Seq.map (fun arr -> 
                                        let items = arr.EnumerateArray() |> Seq.toArray
                                        (items.[0].GetString(), items.[1].GetInt32())
                                    )
                                    |> Seq.toList
                                SourceDiversity = sa.GetProperty("sourceDiversity").GetInt32()
                            }
                        KeyInsights = 
                            let ki = json.GetProperty("keyInsights")
                            {
                                PrimaryStrengths = 
                                    ki.GetProperty("primaryStrengths").EnumerateArray()
                                    |> Seq.map (fun s -> s.GetString())
                                    |> Seq.toList
                                CriticalWeaknesses = 
                                    ki.GetProperty("criticalWeaknesses").EnumerateArray()
                                    |> Seq.map (fun w -> w.GetString())
                                    |> Seq.toList
                                MarketOpportunities = 
                                    ki.GetProperty("marketOpportunities").EnumerateArray()
                                    |> Seq.map (fun o -> o.GetString())
                                    |> Seq.toList
                                CompetitiveAdvantages = 
                                    ki.GetProperty("competitiveAdvantages").EnumerateArray()
                                    |> Seq.map (fun a -> a.GetString())
                                    |> Seq.toList
                            }
                    }
                    Ok report
                with ex ->
                    Error(exn $"Failed to parse LLM response: {ex.Message}\nResponse: {llmResponse}")
            
            return report
        }

module Services.AeoAnalysisService

open Api.GoogleClient
open Api.LLMClient
open Models.AeoModels
open System.Threading
open FsToolkit.ErrorHandling

type AeoAnalysisService(googleClient: GoogleClient, llmClient: LLMClient) =
    
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
            
            // TODO: Feed searchData to LLM for analysis
            // For now, return error to test the flow
            return! Error(exn "LLM analysis not yet implemented")
        }

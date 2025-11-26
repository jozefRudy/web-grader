namespace WebGrader.Api.Controllers

open Microsoft.AspNetCore.Mvc
open Giraffe.ViewEngine
open Models.AeoModels
open Views.AeoViews
open System

[<Route("/")>]
type AeoController() =
    inherit Controller()

    [<HttpGet>]
    member this.Index() =
        let html = formView () |> RenderView.AsString.htmlDocument
        this.Content(html, "text/html")

    [<HttpPost("api/analyze")>]
    member this.Analyze
        (
            [<FromForm>] companyName: string,
            [<FromForm>] location: string,
            [<FromForm>] product: string,
            [<FromForm>] industry: string
        ) =
        task {
            try
                // TODO: Call AeoService for real analysis
                // For now, return mock report with fake data
                let report = {
                    CompanyName = companyName
                    Location = location
                    Product = product
                    Industry = industry
                    Score = {
                        Overall = 75
                        BrandRecognition = 15
                        MarketScore = 8
                        Sentiment = 35
                    }
                    Competitors = [
                        {
                            Name = "Competitor A"
                            ShareOfVoice = 25.5
                        }
                        {
                            Name = "Competitor B"
                            ShareOfVoice = 18.3
                        }
                        {
                            Name = "Competitor C"
                            ShareOfVoice = 12.7
                        }
                    ]
                    Strengths = [
                        "Strong brand presence in target market"
                        "High customer satisfaction ratings"
                        "Innovative product features"
                        "Active social media engagement"
                    ]
                    Weaknesses = [
                        "Limited market share compared to leaders"
                        "Fewer reviews than competitors"
                        "Need more industry recognition"
                    ]
                    Summary =
                        $"Based on our analysis, {companyName} shows strong potential in the {industry} sector. The brand demonstrates solid fundamentals with an overall AEO score of 75/100, indicating good visibility across search engines and AI platforms."
                    AnalysisDate = DateTime.UtcNow
                }

                let html = reportFragment report |> RenderView.AsString.htmlNode
                return this.Content(html, "text/html")
            with ex ->
                let html = errorFragment ex.Message |> RenderView.AsString.htmlNode
                return this.Content(html, "text/html")
        }

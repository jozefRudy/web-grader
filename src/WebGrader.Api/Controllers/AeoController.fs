namespace WebGrader.Api.Controllers

open Microsoft.AspNetCore.Mvc
open Giraffe.ViewEngine
open Models.AeoModels
open Views.AeoViews
open System
open System.Threading
open System.Threading.Tasks
open System.Threading

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
            [<FromForm>] industry: string,
            ct: CancellationToken
        ) =
        task {
            try
                do! Task.Delay(5000, ct)
                // TODO: Call AeoService for real analysis
                // For now, return comprehensive mock report with fake data
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
                        $"Based on our comprehensive analysis, {companyName} demonstrates solid market positioning in the {industry} sector. With an overall AEO score of 75/100, the brand shows good visibility across search engines and AI platforms. The analysis reveals strong brand recognition but indicates opportunities for market share growth and enhanced industry presence."
                    AnalysisDate = DateTime.UtcNow
                    // New expanded categories with realistic dummy data
                    BrandRecognitionDetails = {
                        RecognitionScore = 82
                        MarketPosition = "Challenger"
                        ConfidenceLevel = 85
                        SourceDiversity = 7
                    }
                    MarketCompetitionDetails = {
                        TotalMentions = 1247
                        CompetitorMentions =
                            Map [
                                "Competitor A", 234
                                "Competitor B", 156
                                "Competitor C", 98
                                "Competitor D", 67
                            ]
                        CommonComparisonTopics = [
                            "pricing"
                            "features"
                            "customer support"
                            "ease of use"
                            "integration capabilities"
                        ]
                        MarketTrends = [
                            "Growing demand for cloud-based solutions"
                            "Increasing focus on AI integration"
                            "Rising competition in enterprise segment"
                            "Shift towards subscription models"
                        ]
                    }
                    SentimentDetails = {
                        OverallSentiment = 68
                        PositiveFactors = [
                            "Excellent customer support"
                            "Intuitive user interface"
                            "Reliable performance"
                            "Good value for money"
                            "Strong feature set"
                        ]
                        NegativeFactors = [
                            "Higher pricing compared to competitors"
                            "Limited mobile app functionality"
                            "Steep learning curve for advanced features"
                            "Occasional service outages"
                        ]
                        NeutralMentions = 23
                    }
                    SourceAnalysis = {
                        TotalSources = 47
                        TopSources = [
                            ("reddit.com", 23)
                            ("twitter.com", 18)
                            ("producthunt.com", 15)
                            ("g2.com", 12)
                            ("capterra.com", 9)
                        ]
                        SourceDiversity = 8
                    }
                    KeyInsights = {
                        PrimaryStrengths = [
                            "Strong brand recognition in target market"
                            "Excellent customer satisfaction scores"
                            "Innovative feature development"
                            "Active community engagement"
                        ]
                        CriticalWeaknesses = [
                            "Market share growth opportunities"
                            "Competitive pricing strategy needed"
                            "Mobile experience enhancement"
                            "Enterprise segment penetration"
                        ]
                        MarketOpportunities = [
                            "Expand into enterprise SaaS market"
                            "Strengthen mobile application ecosystem"
                            "Develop strategic partnerships"
                            "Invest in content marketing"
                            "Enhance AI/ML capabilities"
                        ]
                        CompetitiveAdvantages = [
                            "Superior customer support quality"
                            "Strong brand loyalty"
                            "Innovative product roadmap"
                            "Technical expertise"
                            "Market timing advantage"
                        ]
                    }
                }

                let html = reportFragment report |> RenderView.AsString.htmlNode
                return this.Content(html, "text/html")
            with ex ->
                let html = errorFragment ex.Message |> RenderView.AsString.htmlNode
                return this.Content(html, "text/html")
        }

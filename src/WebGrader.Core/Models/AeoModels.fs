module Models.AeoModels

open System

type AnalyzeRequest = {
    CompanyName: string
    Location: string
    Product: string
    Industry: string
}

type AeoScore = {
    Overall: int
    BrandRecognition: int
    MarketScore: int
    Sentiment: int
}

type CompetitorInfo = {
    Name: string
    ShareOfVoice: float
}

type AeoReport = {
    CompanyName: string
    Location: string
    Product: string
    Industry: string
    Score: AeoScore
    Competitors: CompetitorInfo list
    Strengths: string list
    Weaknesses: string list
    Summary: string
    AnalysisDate: DateTime
}

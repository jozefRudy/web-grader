module Models.AeoModels

open System

type AnalysisMode = 
    | GoogleSearch  // Real-time Google Search + LLM synthesis
    | DirectLLM     // Pure LLM knowledge test (no external search)

type AnalyzeRequest = {
    CompanyName: string
    Location: string
    Product: string
    Industry: string
    Mode: AnalysisMode
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

type BrandRecognitionDetails = {
    RecognitionScore: int      // 0-100 (detailed score)
    MarketPosition: string     // "Leader", "Challenger", "Niche"
    ConfidenceLevel: int       // 0-100 (how confident in analysis)
    SourceDiversity: int       // 0-10 (variety of sources)
}

type MarketCompetitionDetails = {
    TotalMentions: int                    // Total brand mentions found
    CompetitorMentions: Map<string, int> // Mentions per competitor
    CommonComparisonTopics: string list  // ["pricing", "features", "support"]
    MarketTrends: string list            // ["growing", "competitive", "niche"]
}

type SentimentDetails = {
    OverallSentiment: int                // 0-100
    PositiveFactors: string list         // ["great support", "easy to use"]
    NegativeFactors: string list         // ["expensive", "limited features"]
    NeutralMentions: int                 // Count of neutral mentions
}

type SourceAnalysis = {
    TotalSources: int                    // Number of different websites
    TopSources: (string * int) list      // [("reddit.com", 15), ("twitter.com", 8)]
    SourceDiversity: int                 // 0-10 (how diverse sources are)
}

type KeyInsights = {
    PrimaryStrengths: string list        // Top 3 strengths
    CriticalWeaknesses: string list      // Top 3 areas for improvement
    MarketOpportunities: string list     // Growth opportunities
    CompetitiveAdvantages: string list   // What sets them apart
}

type AeoReport = {
    CompanyName: string
    Location: string
    Product: string
    Industry: string
    Mode: AnalysisMode
    Score: AeoScore
    Competitors: CompetitorInfo list
    Strengths: string list
    Weaknesses: string list
    Summary: string
    AnalysisDate: DateTime
    // New expanded categories
    BrandRecognitionDetails: BrandRecognitionDetails
    MarketCompetitionDetails: MarketCompetitionDetails
    SentimentDetails: SentimentDetails
    SourceAnalysis: SourceAnalysis
    KeyInsights: KeyInsights
}

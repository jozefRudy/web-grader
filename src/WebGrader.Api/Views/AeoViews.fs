module Views.AeoViews

open Giraffe.ViewEngine
open Models.AeoModels

let layout (content: XmlNode list) =
    html [ attr "data-theme" "light" ] [
        head [] [
            meta [ _charset "utf-8" ]
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [ str "AEO Brand Analyzer" ]
            link [ _href "https://cdn.jsdelivr.net/npm/daisyui@4/dist/full.css"; _rel "stylesheet" ]
            script [ _src "https://cdn.tailwindcss.com" ] []
            script [ _src "https://unpkg.com/htmx.org@1.9.10" ] []
            style [] [
                rawText """
                .htmx-indicator {
                    display: none;
                }
                .htmx-request .htmx-indicator {
                    display: block;
                }
                """
            ]
        ]
        body [ _class "min-h-screen bg-base-200 text-base-content" ] content
    ]

let formView() =
    layout [
        div [ _class "container mx-auto max-w-4xl p-8" ] [
            div [ _class "card bg-base-100 shadow-xl" ] [
                div [ _class "card-body" ] [
                    h1 [ _class "card-title text-3xl mb-6" ] [ str "AEO Brand Analysis" ]
                    
                    form [ 
                        attr "hx-post" "/api/analyze"
                        attr "hx-target" "#report"
                        attr "hx-indicator" "#loading"
                        _class "space-y-4"
                    ] [
                        // Company Name
                        div [ _class "form-control" ] [
                            label [ _class "label" ] [
                                span [ _class "label-text font-semibold" ] [ str "Company Name *" ]
                            ]
                            input [ 
                                _type "text"
                                _name "companyName"
                                _required
                                _placeholder "e.g., Acme Corp"
                                _class "input input-bordered w-full"
                            ]
                        ]
                        
                        // Location
                        div [ _class "form-control" ] [
                            label [ _class "label" ] [
                                span [ _class "label-text font-semibold" ] [ str "Location *" ]
                            ]
                            input [ 
                                _type "text"
                                _name "location"
                                _required
                                _placeholder "e.g., United States"
                                _class "input input-bordered w-full"
                            ]
                        ]
                        
                        // Product/Service
                        div [ _class "form-control" ] [
                            label [ _class "label" ] [
                                span [ _class "label-text font-semibold" ] [ str "Product/Service *" ]
                            ]
                            input [ 
                                _type "text"
                                _name "product"
                                _required
                                _placeholder "e.g., CRM Software"
                                _class "input input-bordered w-full"
                            ]
                        ]
                        
                        // Industry
                        div [ _class "form-control" ] [
                            label [ _class "label" ] [
                                span [ _class "label-text font-semibold" ] [ str "Industry *" ]
                            ]
                            input [ 
                                _type "text"
                                _name "industry"
                                _required
                                _placeholder "e.g., B2B SaaS"
                                _class "input input-bordered w-full"
                            ]
                        ]
                        
                        button [ _type "submit"; _class "btn btn-primary w-full mt-6" ] [
                            str "Analyze Brand"
                        ]
                        
                        div [ _id "loading"; _class "htmx-indicator mt-4" ] [
                            div [ _class "alert alert-info" ] [
                                span [ _class "loading loading-spinner" ] []
                                str " Analyzing... This may take 30-60 seconds"
                            ]
                        ]
                    ]
                ]
            ]
            
            div [ _id "report"; _class "mt-8" ] []
        ]
    ]

let reportFragment (report: AeoReport) =
    div [ _class "card bg-base-100 shadow-xl" ] [
        div [ _class "card-body" ] [
            h2 [ _class "card-title text-2xl mb-4" ] [
                str $"AEO Analysis: {report.CompanyName}"
            ]

            div [ _class "text-sm text-gray-600 mb-6" ] [
                str $"Location: {report.Location} | Product: {report.Product} | Industry: {report.Industry}"
            ]

            // Score cards
            div [ _class "grid grid-cols-2 md:grid-cols-4 gap-4 mb-8" ] [
                div [ _class "stat bg-blue-700 text-white rounded-lg p-4 shadow-md" ] [
                    div [ _class "stat-title text-sm text-blue-100" ] [ str "Overall Score" ]
                    div [ _class "stat-value text-3xl font-bold" ] [ str (string report.Score.Overall) ]
                    div [ _class "stat-desc text-blue-200" ] [ str "out of 100" ]
                ]
                div [ _class "stat bg-green-50 text-green-900 rounded-lg p-4 shadow-md" ] [
                    div [ _class "stat-title text-sm text-green-700" ] [ str "Brand Recognition" ]
                    div [ _class "stat-value text-3xl font-bold" ] [ str (string report.Score.BrandRecognition) ]
                    div [ _class "stat-desc text-green-600" ] [ str "out of 20" ]
                ]
                div [ _class "stat bg-blue-50 text-blue-900 rounded-lg p-4 shadow-md" ] [
                    div [ _class "stat-title text-sm text-blue-700" ] [ str "Market Score" ]
                    div [ _class "stat-value text-3xl font-bold" ] [ str (string report.Score.MarketScore) ]
                    div [ _class "stat-desc text-blue-600" ] [ str "out of 10" ]
                ]
                div [ _class "stat bg-orange-50 text-orange-900 rounded-lg p-4 shadow-md" ] [
                    div [ _class "stat-title text-sm text-orange-700" ] [ str "Sentiment" ]
                    div [ _class "stat-value text-3xl font-bold" ] [ str (string report.Score.Sentiment) ]
                    div [ _class "stat-desc text-orange-600" ] [ str "out of 40" ]
                ]
            ]

            // Source Analysis
            div [ _class "mb-8" ] [
                h3 [ _class "text-xl font-bold mb-4 text-purple-700" ] [ str "Source Analysis" ]
                div [ _class "bg-purple-50 p-4 rounded-lg" ] [
                    div [ _class "grid grid-cols-2 md:grid-cols-3 gap-4 mb-4" ] [
                        div [ _class "text-center" ] [
                            div [ _class "text-2xl font-bold text-purple-600" ] [ str (string report.SourceAnalysis.TotalSources) ]
                            div [ _class "text-sm text-gray-600" ] [ str "Total Sources" ]
                        ]
                        div [ _class "text-center" ] [
                            div [ _class "text-2xl font-bold text-purple-600" ] [ str $"{report.SourceAnalysis.SourceDiversity}/10" ]
                            div [ _class "text-sm text-gray-600" ] [ str "Source Diversity" ]
                        ]
                        div [ _class "text-center md:col-span-1" ] [
                            div [ _class "text-sm text-gray-600" ] [ str "Top Sources" ]
                        ]
                    ]

                    if not report.SourceAnalysis.TopSources.IsEmpty then
                        div [ _class "space-y-2" ] [
                            for (source, count) in report.SourceAnalysis.TopSources |> List.take 5 do
                                div [ _class "flex justify-between bg-white p-2 rounded" ] [
                                    span [] [ str source ]
                                    span [ _class "font-semibold" ] [ str $"{count} mentions" ]
                                ]
                        ]
                ]
            ]

            // Brand Recognition Details
            div [ _class "mb-8" ] [
                h3 [ _class "text-xl font-bold mb-4 text-green-700" ] [ str "Brand Recognition Details" ]
                div [ _class "bg-green-50 p-4 rounded-lg" ] [
                    div [ _class "grid grid-cols-2 md:grid-cols-4 gap-4" ] [
                        div [ _class "text-center" ] [
                            div [ _class "text-2xl font-bold text-green-600" ] [ str (string report.BrandRecognitionDetails.RecognitionScore) ]
                            div [ _class "text-sm text-gray-600" ] [ str "Recognition Score" ]
                        ]
                        div [ _class "text-center" ] [
                            div [ _class "text-lg font-semibold text-green-600" ] [ str report.BrandRecognitionDetails.MarketPosition ]
                            div [ _class "text-sm text-gray-600" ] [ str "Market Position" ]
                        ]
                        div [ _class "text-center" ] [
                            div [ _class "text-2xl font-bold text-green-600" ] [ str $"{report.BrandRecognitionDetails.ConfidenceLevel}%%" ]
                            div [ _class "text-sm text-gray-600" ] [ str "Confidence Level" ]
                        ]
                        div [ _class "text-center" ] [
                            div [ _class "text-2xl font-bold text-green-600" ] [ str $"{report.BrandRecognitionDetails.SourceDiversity}/10" ]
                            div [ _class "text-sm text-gray-600" ] [ str "Source Diversity" ]
                        ]
                    ]
                ]
            ]

            // Market Competition
            div [ _class "mb-8" ] [
                h3 [ _class "text-xl font-bold mb-4 text-blue-700" ] [ str "Market Score Analysis" ]
                div [ _class "bg-blue-50 p-4 rounded-lg" ] [
                    div [ _class "mb-4" ] [
                        div [ _class "text-lg" ] [
                            str $"Total Mentions Found: "
                            strong [] [ str (string report.MarketCompetitionDetails.TotalMentions) ]
                        ]
                    ]

                    if not report.MarketCompetitionDetails.CompetitorMentions.IsEmpty then
                        div [ _class "mb-4" ] [
                            h4 [ _class "font-semibold mb-2" ] [ str "Competitor Mentions:" ]
                            div [ _class "grid grid-cols-2 md:grid-cols-3 gap-2" ] [
                                for (competitor, mentions) in Map.toSeq report.MarketCompetitionDetails.CompetitorMentions do
                                    div [ _class "bg-white p-2 rounded shadow-sm" ] [
                                        str $"{competitor}: {mentions} mentions"
                                    ]
                            ]
                        ]

                    if not report.MarketCompetitionDetails.CommonComparisonTopics.IsEmpty then
                        div [ _class "mb-4" ] [
                            h4 [ _class "font-semibold mb-2" ] [ str "Common Comparison Topics:" ]
                            div [ _class "flex flex-wrap gap-2" ] [
                                for topic in report.MarketCompetitionDetails.CommonComparisonTopics do
                                    span [ _class "badge bg-blue-200 text-blue-900 border-none" ] [ str topic ]
                            ]
                        ]

                    if not report.MarketCompetitionDetails.MarketTrends.IsEmpty then
                        div [] [
                            h4 [ _class "font-semibold mb-2" ] [ str "Market Trends:" ]
                            ul [ _class "list-disc list-inside space-y-1" ] [
                                for trend in report.MarketCompetitionDetails.MarketTrends do
                                    li [] [ str trend ]
                            ]
                        ]
                ]
            ]

            // Sentiment Analysis
            div [ _class "mb-8" ] [
                h3 [ _class "text-xl font-bold mb-4 text-orange-700" ] [ str "Sentiment Analysis" ]
                div [ _class "bg-orange-50 p-4 rounded-lg" ] [
                    div [ _class "mb-4" ] [
                        div [ _class "text-lg" ] [
                            str $"Overall Sentiment: "
                            strong [] [ str $"{report.SentimentDetails.OverallSentiment}/100" ]
                        ]
                        div [ _class "text-sm text-gray-600" ] [
                            str $"({report.SentimentDetails.NeutralMentions} neutral mentions)"
                        ]
                    ]

                    div [ _class "grid md:grid-cols-2 gap-6" ] [
                        div [] [
                            h4 [ _class "font-semibold mb-2 text-green-600" ] [ str "Positive Factors" ]
                            ul [ _class "list-disc list-inside space-y-1" ] [
                                for factor in report.SentimentDetails.PositiveFactors do
                                    li [] [ str factor ]
                            ]
                        ]
                        div [] [
                            h4 [ _class "font-semibold mb-2 text-red-600" ] [ str "Areas for Improvement" ]
                            ul [ _class "list-disc list-inside space-y-1" ] [
                                for factor in report.SentimentDetails.NegativeFactors do
                                    li [] [ str factor ]
                            ]
                        ]
                    ]
                ]
            ]

            // Key Insights
            div [ _class "mb-8" ] [
                h3 [ _class "text-xl font-bold mb-4 text-indigo-700" ] [ str "Key Insights & Recommendations" ]
                div [ _class "bg-indigo-50 p-4 rounded-lg" ] [
                    div [ _class "grid md:grid-cols-2 gap-6" ] [
                        div [] [
                            h4 [ _class "font-semibold mb-2 text-green-600" ] [ str "💪 Primary Strengths" ]
                            ul [ _class "list-disc list-inside space-y-1" ] [
                                for strength in report.KeyInsights.PrimaryStrengths do
                                    li [] [ str strength ]
                            ]
                        ]
                        div [] [
                            h4 [ _class "font-semibold mb-2 text-blue-600" ] [ str "🎯 Market Opportunities" ]
                            ul [ _class "list-disc list-inside space-y-1" ] [
                                for opportunity in report.KeyInsights.MarketOpportunities do
                                    li [] [ str opportunity ]
                            ]
                        ]
                    ]

                    div [ _class "grid md:grid-cols-2 gap-6 mt-6" ] [
                        div [] [
                            h4 [ _class "font-semibold mb-2 text-orange-600" ] [ str "⚠️ Critical Weaknesses" ]
                            ul [ _class "list-disc list-inside space-y-1" ] [
                                for weakness in report.KeyInsights.CriticalWeaknesses do
                                    li [] [ str weakness ]
                            ]
                        ]
                        div [] [
                            h4 [ _class "font-semibold mb-2 text-purple-600" ] [ str "🏆 Competitive Advantages" ]
                            ul [ _class "list-disc list-inside space-y-1" ] [
                                for advantage in report.KeyInsights.CompetitiveAdvantages do
                                    li [] [ str advantage ]
                            ]
                        ]
                    ]
                ]
            ]

            // Summary
            div [ _class "mb-6" ] [
                h3 [ _class "text-xl font-bold mb-2" ] [ str "Executive Summary" ]
                p [ _class "text-gray-700 leading-relaxed" ] [ str report.Summary ]
            ]

            // Print button
            div [ _class "flex justify-end gap-4" ] [
                button [
                    _class "btn btn-secondary"
                    _onclick "window.print()"
                ] [ str "Print Report" ]
            ]
        ]
    ]

let errorFragment (message: string) =
    div [ _class "alert alert-error shadow-lg" ] [
        div [] [
            str "Error: "
            str message
        ]
    ]

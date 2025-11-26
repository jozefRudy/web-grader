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
                str $"{report.Location} | {report.Product} | {report.Industry}"
            ]
            
            // Score cards
            div [ _class "grid grid-cols-2 md:grid-cols-4 gap-4 mb-8" ] [
                div [ _class "stat bg-blue-600 text-white rounded-lg p-4 shadow-md" ] [
                    div [ _class "stat-title text-sm text-blue-100" ] [ str "Overall Score" ]
                    div [ _class "stat-value text-3xl font-bold" ] [ str (string report.Score.Overall) ]
                    div [ _class "stat-desc text-blue-200" ] [ str "out of 100" ]
                ]
                div [ _class "stat bg-green-600 text-white rounded-lg p-4 shadow-md" ] [
                    div [ _class "stat-title text-sm text-green-100" ] [ str "Brand Recognition" ]
                    div [ _class "stat-value text-3xl font-bold" ] [ str (string report.Score.BrandRecognition) ]
                    div [ _class "stat-desc text-green-200" ] [ str "out of 20" ]
                ]
                div [ _class "stat bg-purple-600 text-white rounded-lg p-4 shadow-md" ] [
                    div [ _class "stat-title text-sm text-purple-100" ] [ str "Market Score" ]
                    div [ _class "stat-value text-3xl font-bold" ] [ str (string report.Score.MarketScore) ]
                    div [ _class "stat-desc text-purple-200" ] [ str "out of 10" ]
                ]
                div [ _class "stat bg-orange-600 text-white rounded-lg p-4 shadow-md" ] [
                    div [ _class "stat-title text-sm text-orange-100" ] [ str "Sentiment" ]
                    div [ _class "stat-value text-3xl font-bold" ] [ str (string report.Score.Sentiment) ]
                    div [ _class "stat-desc text-orange-200" ] [ str "out of 40" ]
                ]
            ]
            
            // Summary
            div [ _class "mb-6" ] [
                h3 [ _class "text-xl font-bold mb-2" ] [ str "Summary" ]
                p [ _class "text-gray-700" ] [ str report.Summary ]
            ]
            
            // Strengths and Weaknesses
            div [ _class "grid md:grid-cols-2 gap-6 mb-6" ] [
                div [] [
                    h3 [ _class "text-xl font-bold mb-2 text-success" ] [ str "Strengths" ]
                    ul [ _class "list-disc list-inside space-y-1" ] [
                        for strength in report.Strengths do
                            li [] [ str strength ]
                    ]
                ]
                div [] [
                    h3 [ _class "text-xl font-bold mb-2 text-warning" ] [ str "Growth Areas" ]
                    ul [ _class "list-disc list-inside space-y-1" ] [
                        for weakness in report.Weaknesses do
                            li [] [ str weakness ]
                    ]
                ]
            ]
            
            // Competitors
            if not report.Competitors.IsEmpty then
                div [ _class "mb-6" ] [
                    h3 [ _class "text-xl font-bold mb-2" ] [ str "Top Competitors" ]
                    div [ _class "overflow-x-auto" ] [
                        table [ _class "table table-zebra w-full" ] [
                            thead [] [
                                tr [] [
                                    th [] [ str "Competitor" ]
                                    th [] [ str "Share of Voice %" ]
                                ]
                            ]
                            tbody [] [
                                for comp in report.Competitors do
                                    tr [] [
                                        td [] [ str comp.Name ]
                                        td [] [ str $"%.1f{comp.ShareOfVoice}%%" ]
                                    ]
                            ]
                        ]
                    ]
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

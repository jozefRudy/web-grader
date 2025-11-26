namespace WebGrader.Api.Controllers

open Microsoft.AspNetCore.Mvc
open Giraffe.ViewEngine
open Models.AeoModels
open Views.AeoViews
open Services.AeoAnalysisService
open System
open System.Threading

[<Route("/")>]
type AeoController(aeoService: AeoAnalysisService) =
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
            [<FromForm>] mode: string,
            ct: CancellationToken
        ) =
        task {
            try
                // Parse mode string to discriminated union
                let analysisMode =
                    match mode with
                    | "direct" -> AnalysisMode.DirectLLM
                    | _ -> AnalysisMode.GoogleSearch // default to Google

                // Call the AeoAnalysisService to generate the report
                let! result = aeoService.GenerateReport(companyName, location, product, industry, analysisMode, ct)

                match result with
                | Ok report ->
                    let html = reportFragment report |> RenderView.AsString.htmlNode
                    return this.Content(html, "text/html")
                | Error ex ->
                    let html = errorFragment ex.Message |> RenderView.AsString.htmlNode
                    return this.Content(html, "text/html")
            with ex ->
                let html = errorFragment ex.Message |> RenderView.AsString.htmlNode
                return this.Content(html, "text/html")
        }

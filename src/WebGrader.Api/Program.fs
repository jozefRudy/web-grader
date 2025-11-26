namespace WebGrader.Api

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Config.Google
open Config.LiteLLM
open Extensions.ApiExtension
open Api.GoogleClient
open Api.LLMClient
open Services.AeoAnalysisService
open Utils.Settings



module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder args
        let timeout = TimeSpan.FromSeconds 60.0

        // Register HTTP clients
        do
            builder.Services |> requireOptions<GoogleConfig> GoogleConfig.SectionName

            builder.Services.AddConfigurableResilientClient<Api.GoogleClient.GoogleClient>(
                (fun sp c ->
                    c.BaseAddress <- Uri "https://www.googleapis.com/customsearch/v1"
                    c.Timeout <- timeout
                ),
                configureResilience = (fun options -> configureStandardTimeouts timeout options)
            )

            builder.Services |> requireOptions<LiteLLMConfig> LiteLLMConfig.SectionName

            builder.Services.AddConfigurableResilientClient<Api.LLMClient.LLMClient>(
                (fun sp c ->
                    let config = sp.GetRequiredService<IOptions<LiteLLMConfig>>().Value
                    c.BaseAddress <- Uri config.Uri
                    c.Timeout <- timeout

                ),
                configureResilience = (fun options -> configureStandardTimeouts timeout options)
            )

        builder.Services.AddScoped<AeoAnalysisService>() |> ignore

        builder.Services.AddControllers()

        let app = builder.Build()
        app.MapControllers()
        app.Run()

        exitCode

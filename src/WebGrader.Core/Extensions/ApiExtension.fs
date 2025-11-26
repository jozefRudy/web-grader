module Extensions.ApiExtension

open System
open System.Net
open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Http.Resilience
open Microsoft.Extensions.Logging
open Api.LoggingHandler

let configureStandardTimeouts (baseTimeout: TimeSpan) (options: HttpStandardResilienceOptions) =
    options.AttemptTimeout.Timeout <- baseTimeout
    options.TotalRequestTimeout.Timeout <- baseTimeout * 3.0
    options.CircuitBreaker.SamplingDuration <- baseTimeout * 2.0

type IServiceCollection with

    member this.AddConfigurableResilientClient<'T when 'T: not struct>
        (
            configureClient: IServiceProvider -> HttpClient -> unit,
            ?configureResilience: HttpStandardResilienceOptions -> unit,
            ?configureHandlers: IHttpClientBuilder -> IHttpClientBuilder
        ) =
        let client =
            this
                .AddHttpClient<'T>()
                .ConfigureHttpClient(fun (sp: IServiceProvider) (c: HttpClient) -> configureClient sp c)
                .AddHttpMessageHandler(fun (sp: IServiceProvider) ->
                    let logger = sp.GetRequiredService<ILogger<'T>>()
                    new LoggingHandler<'T>(logger) :> DelegatingHandler
                )

        let clientWithHandlers =
            match configureHandlers with
            | Some configure -> configure client
            | None -> client

        let finalClient =
            clientWithHandlers.ConfigurePrimaryHttpMessageHandler(fun () ->
                let handler = new HttpClientHandler()
                handler.AutomaticDecompression <- DecompressionMethods.GZip ||| DecompressionMethods.Deflate
                handler :> HttpMessageHandler
            )

        match configureResilience with
        | Some configure -> finalClient.AddStandardResilienceHandler(fun options -> configure options)
        | None -> finalClient.AddStandardResilienceHandler()

    member this.AddResilientClient<'T when 'T: not struct>(uri: string option) =
        this.AddConfigurableResilientClient<'T>(fun (_: IServiceProvider) (c: HttpClient) ->
            match uri with
            | Some uri -> c.BaseAddress <- Uri uri
            | None -> ()
        )

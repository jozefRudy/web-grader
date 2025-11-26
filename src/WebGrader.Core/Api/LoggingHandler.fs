module Api.LoggingHandler

open System.Net.Http
open System.Threading
open Microsoft.Extensions.Logging

type internal CallableHandler(messageHandler) =
    inherit DelegatingHandler(messageHandler)

    member internal x.CallSendAsync(request, cancellationToken) =
        base.SendAsync(request, cancellationToken)

type LoggingHandler<'T>(logger: ILogger<'T>) =
    inherit DelegatingHandler()

    override this.SendAsync(request, ct) =
        let wrapped = new CallableHandler(base.InnerHandler)

        task {
            try
                return! wrapped.CallSendAsync(request, ct)
            with ex ->
                logger.LogError(ex, "HTTP request failed: {Method} {Uri}", request.Method, request.RequestUri)
                return raise ex
        }

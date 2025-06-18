using Microsoft.Extensions.Logging;

namespace ThreeTP.Payment.Infrastructure.Loggin;

public class NmiLoggingHandler : DelegatingHandler
{
    private readonly ILogger<NmiLoggingHandler> _logger;

    public NmiLoggingHandler(ILogger<NmiLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestContent = request.Content != null ? await request.Content.ReadAsStringAsync() : null;

        _logger.LogInformation("NMI Request: {Method} {Url} {Content}",
            request.Method, request.RequestUri, requestContent);

        var response = await base.SendAsync(request, cancellationToken);

        var responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : null;

        _logger.LogInformation("NMI Response: {StatusCode} {Content}",
            response.StatusCode, responseContent);

        return response;
    }
}
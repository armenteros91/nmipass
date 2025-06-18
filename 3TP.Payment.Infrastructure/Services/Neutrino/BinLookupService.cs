using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ThreeTP.Payment.Application.DTOs.Responses.BIN_Checker;

namespace ThreeTP.Payment.Infrastructure.Services.Neutrino;

public class BinLookupService
{
    private readonly HttpClient _httpClient;
    private readonly NeutrinoApiOptions _neutrinoApiOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<BinLookupService> _logger;

    public BinLookupService(
        HttpClient httpClient,
        IOptions<NeutrinoApiOptions> options,
        IHttpContextAccessor httpContextAccessor,
        ILogger<BinLookupService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _neutrinoApiOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate endpoint configuration
        if (string.IsNullOrWhiteSpace(_neutrinoApiOptions.EndPoint))
        {
            throw new ArgumentException("Neutrino API endpoint cannot be null or empty.", nameof(options));
        }
    }

    public async Task<BinlookupResponse> GetBinLookupAsync(string binNumber)
    {
        BinlookupResponse binlookupResponse =new BinlookupResponse();
        
        if (string.IsNullOrWhiteSpace(binNumber))
        {
            _logger.LogWarning("BIN number is null or empty");
            throw new ArgumentException("BIN number cannot be null or empty.", nameof(binNumber));
        }

        _logger.LogInformation("Starting bin lookup for BIN: {BinNumber}", binNumber);
        try
        {
            var context = _httpContextAccessor.HttpContext;
            var clientIp = EndpointHttpContextExtensions.GetEndpoint(context);
            if (clientIp == null)
            {
                _logger.LogWarning("Client IP could not be retrieved from HttpContext.");
            }

            var formData = new[]
            {
                new KeyValuePair<string, string>("bin-number", binNumber),
                new KeyValuePair<string, string>("customer-ip", clientIp.ToString() ?? String.Empty)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _neutrinoApiOptions.EndPoint);
            request.Content = new FormUrlEncodedContent(formData);
            request.Headers.Add("User-ID", _neutrinoApiOptions.UserId);
            request.Headers.Add("API-Key", _neutrinoApiOptions.ApiKey);
            request.Headers.Add("Accept", "*/*");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Throws if not successful

            var responseContent = await response.Content.ReadAsStringAsync();

            try
            {
                binlookupResponse = JsonConvert.DeserializeObject<BinlookupResponse>(responseContent)
                                    ?? throw new JsonException("Deserialized response is null.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize API response for BIN: {BinNumber}", binNumber);
                throw new InvalidOperationException("Failed to parse API response.", ex);
            }


            _logger.LogInformation("Successfully retrieved bin data for BIN: {BinNumber}", binNumber);

            return binlookupResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception occurred while retrieving bin data for BIN: {BinNumber}",
                binNumber);
            throw new BinLookupException("Failed to retrieve BIN data due to HTTP error.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving bin data for BIN: {BinNumber}",
                binNumber);
            throw new BinLookupException("An unexpected error occurred during BIN lookup.", ex);
        }
    }
}

// Custom exception for better error handling
public class BinLookupException : Exception
{
    public BinLookupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
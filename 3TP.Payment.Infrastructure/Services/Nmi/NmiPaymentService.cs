using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Helpers;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.Payment;
using ThreeTP.Payment.Domain.Entities.Nmi;

namespace ThreeTP.Payment.Infrastructure.Services.Nmi;

/// <summary>
/// Implements the NMI payment gateway service for processing transactions and queries.
/// </summary>
public class NmiPaymentService : INmiPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NmiPaymentService> _logger;
    private readonly IOptions<NmiSettings> _nmiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="NmiPaymentService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API requests.</param>
    /// <param name="mapper">The AutoMapper instance for mapping DTOs to entities.</param>
    /// <param name="logger">The logger for capturing service events and errors.</param>
    /// <param name="nmiSettings"></param>
    /// <param name="unitOfWork"></param>
    public NmiPaymentService(
        HttpClient httpClient,
        IMapper mapper,
        ILogger<NmiPaymentService> logger,
        IOptions<NmiSettings> nmiSettings,
        IUnitOfWork unitOfWork)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _nmiSettings = nmiSettings ?? throw new ArgumentNullException(nameof(nmiSettings));
        _unitOfWork = unitOfWork;
        ValidateNmiSettings(_nmiSettings);
    }

    private void ValidateNmiSettings(IOptions<NmiSettings> settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.Endpoint.Transaction))
            throw new InvalidOperationException("NMI BaseURL is not configured.");

        if (string.IsNullOrWhiteSpace(settings.Value.Endpoint?.Transaction))
            throw new InvalidOperationException("NMI Transaction endpoint is not configured.");

        if (string.IsNullOrWhiteSpace(settings.Value.Query?.QueryApi))
            throw new InvalidOperationException("NMI Query endpoint is not configured.");
    }

    /// <summary>
    /// Sends a transaction request to the NMI payment gateway and processes the Response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the transaction request DTO, inheriting from <see cref="BaseTransactionRequestDto"/>.</typeparam>
    /// <param name="dto">The transaction request data.</param>
    /// <returns>A <see cref="NmiResponseDto"/> containing the transaction Response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    public async Task<NmiResponseDto> SendAsync<TRequest>(TRequest dto) where TRequest : SaleTransactionRequestDto
    {
        ArgumentNullException.ThrowIfNull(dto);
        var form = new FormUrlEncodedContent(Utils.DtoExtensions.ToKeyValuePairsProperty(dto));
        
        //var formContent = await form.ReadAsStringAsync();
        var formContent = Utils.SensitiveDataLogger.ToMaskedFormUrlEncoded(dto);
        _logger.LogInformation("Contenido del form enviado: {FormContent}", formContent);
         
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_nmiSettings.Value.BaseUrl}{_nmiSettings.Value.Endpoint.Transaction}")
        {
            Content = form
        };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

        // Enviar la solicitud
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("NMI request failed with status code {StatusCode} and content: {Content}",
                response.StatusCode, content);
            throw new HttpRequestException($"NMI request failed: {response.StatusCode}");
        }

        var parsedResponse = Utils.DtoExtensions.ParseResponse<NmiResponseDto>(content);
        
        // Map and persist request
        // var requestLog = _mapper.Map<NmiTransactionRequestLog>(dto);
        // //requestLog.RawContent = string.Join("&", Utils.DtoExtensions.ToKeyValuePairsProperty(dto).Select(kv => $"{kv.Key}={kv.Value}"));
        // requestLog.RawContent = formContent;
        
        return parsedResponse;
    }

    /// <summary>
    /// Queries a transaction from the NMI payment gateway using the provided query parameters and returns the Response as an XML-deserialized object.
    /// </summary>
    /// <param name="dto">The query transaction request data.</param>
    /// <returns>A <see cref="QueryResponseDto"/> containing the query Response data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the XML Response cannot be deserialized.</exception>
    public async Task<QueryResponseDto> QueryAsync(QueryTransactionRequestDto dto)
    {
        if (dto == null)
        {
            _logger.LogError("Query transaction request DTO is null.");
            throw new ArgumentNullException(nameof(dto), "Query transaction request DTO cannot be null.");
        }

        _logger.LogInformation("Sending query request to NMI API for TransactionId: {TransactionId}",
            dto.TransactionId);

        var form = dto.ToFormContent();

        _logger.LogInformation(" Pasarela Content Query : {0}", form.ToString());
        try
        {
            var response =
                await _httpClient.PostAsync($"{_nmiSettings.Value.BaseUrl}{_nmiSettings.Value.Query.QueryApi}",
                    form);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received query Response: {Content}", content);

            var responseDeserialized = Utils.QueryResponseSerializer.DeserializeXml(content);

            //todo : Consultar si se debe persisitir o guardar la consultas en la base de datos , informacion de las transacciones , por PCI DSS Complaint no adminte guardar informacion privilegiada y no es recomendable 

            // Map and persist request
            // var requestLog = _mapper.Map<NmiTransactionRequestLog>(dto);
            // requestLog.RawContent = string.Join("&",
            //     Utils.DtoExtensions.ToKeyValuePairsForNotLogginSensitiveData(dto)
            //         .Where(kv => kv.Value != null)
            //         .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

            //  await _requestLogRepo.AddAsync(requestLog);

            // Map and persist Response
            // var responseLog = _mapper.Map<NmiTransactionResponseLog>(parsedResponse); // todo: modelo que consulta de respueta fcani 
            // responseLog.RawResponse = content;
            // await _responseLogRepo.AddAsync(responseLog);

            _logger.LogInformation(" gateway Query processed successfully ");
            return responseDeserialized;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to query transaction from NMI API");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Gateway query Response");
            throw new InvalidOperationException("Failed to process query Response", ex);
        }
    }


    /// <summary>
    /// Parses a raw NMI API Response into a <see cref="NmiResponseDto"/>.
    /// </summary>
    /// <param name="raw">The raw Response string from the NMI API.</param>
    /// <returns>A <see cref="NmiResponseDto"/> containing the parsed Response data.</returns>
    private NmiResponseDto ParseResponse(string raw)
    {
        var pairs = raw.Split('&')
            .Select(part => part.Split('='))
            .Where(split => split.Length == 2)
            .ToDictionary(split => split[0], split => Uri.UnescapeDataString(split[1]));

        var dto = new NmiResponseDto();
        foreach (var prop in typeof(NmiResponseDto).GetProperties())
        {
            if (pairs.TryGetValue(prop.Name.ToLower(), out var val))
            {
                prop.SetValue(dto, Convert.ChangeType(val, prop.PropertyType));
            }
        }
        return dto;
    }
}
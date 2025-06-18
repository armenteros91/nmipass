using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Helpers;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Nmi;

namespace ThreeTP.Payment.Infrastructure.Services.Nmi;

/// <summary>
/// Implements the NMI payment gateway service for processing transactions and queries.
/// </summary>
public class NmiPaymentService : INmiPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
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
    }

    /// <summary>
    /// Sends a transaction request to the NMI payment gateway and processes the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the transaction request DTO, inheriting from <see cref="BaseTransactionRequestDto"/>.</typeparam>
    /// <param name="dto">The transaction request data.</param>
    /// <returns>A <see cref="NmiResponseDto"/> containing the transaction response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    public async Task<NmiResponseDto> SendAsync<TRequest>(TRequest dto) where TRequest : BaseTransactionRequestDto
    {
        var form = new FormUrlEncodedContent(Utils.DtoExtensions.ToKeyValuePairs(dto));

        var response =
            await _httpClient.PostAsync($"{_nmiSettings.Value.BaseURL}{_nmiSettings.Value.Endpoint.Transaction}",
                form);
        var content = await response.Content.ReadAsStringAsync();

        var parsedResponse = ParseResponse(content);

        // Map and persist request
        var requestLog = _mapper.Map<NmiTransactionRequestLog>(dto);

        requestLog.RawContent =
            string.Join("&",
                Utils.DtoExtensions.ToKeyValuePairs(dto)
                    .Select(kv => $"{kv.Key}={kv.Value}")); // TODO: validar si un campo no esta presente no agregar 

        //await _unitOfWork.requestLogRepo.AddAsync(requestLog);

        //todo:pendiente agregar log de tabla de transacciones roilan

        // Map and persist response
        // var responseLog = _mapper.Map<NmiTransactionResponseLog>(parsedResponse);
        // responseLog.RawResponse = content;
        // await _responseLogRepo.AddAsync(responseLog);

        return parsedResponse;
    }

    /// <summary>
    /// Queries a transaction from the NMI payment gateway using the provided query parameters and returns the response as an XML-deserialized object.
    /// </summary>
    /// <param name="dto">The query transaction request data.</param>
    /// <returns>A <see cref="QueryResponseDto"/> containing the query response data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the XML response cannot be deserialized.</exception>
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
                await _httpClient.PostAsync($"{_nmiSettings.Value.BaseURL}{_nmiSettings.Value.Query.QueryApi}",
                    form);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received query response: {Content}", content);

            var responseDeserialized = Utils.QueryResponseSerializer.DeserializeXml(content);

            //todo : Consultar si se debe persisitir o guardar la consultas en la base de datos , informacion de las transacciones , por PCI DSS Complaint no adminte guardar informacion privilegiada y no es recomendable 

            // Map and persist request
            // var requestLog = _mapper.Map<NmiTransactionRequestLog>(dto);
            // requestLog.RawContent = string.Join("&",
            //     Utils.DtoExtensions.ToKeyValuePairs(dto)
            //         .Where(kv => kv.Value != null)
            //         .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

            //  await _requestLogRepo.AddAsync(requestLog);

            // Map and persist response
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
            _logger.LogError(ex, "Error processing Gateway query response");
            throw new InvalidOperationException("Failed to process query response", ex);
        }
    }


    /// <summary>
    /// Parses a raw NMI API response into a <see cref="NmiResponseDto"/>.
    /// </summary>
    /// <param name="raw">The raw response string from the NMI API.</param>
    /// <returns>A <see cref="NmiResponseDto"/> containing the parsed response data.</returns>
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
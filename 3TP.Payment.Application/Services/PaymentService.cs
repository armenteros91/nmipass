using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Nmi;

namespace ThreeTP.Payment.Application.Services;

public class PaymentService: IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INmiPaymentGateway _nmiPaymentGateway;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        INmiPaymentGateway nmiPaymentGateway,
        ILogger<PaymentService> logger,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _nmiPaymentGateway = nmiPaymentGateway;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<NmiResponseDto> ProcessPaymentAsync(string apiKey, BaseTransactionRequestDto paymentRequest)
    {
        // 1. Buscar Tenant por ApiKey
        var tenant = await _unitOfWork.TenantRepository.GetByApiKeyAsync(apiKey);

        if (tenant == null || !tenant.IsActive)
        {
            _logger.LogWarning("Tenant not found or inactive for ApiKey: {ApiKey}", apiKey);
            throw new UnauthorizedAccessException("Invalid API Key or inactive tenant.");
        }

        // 2. Setear dinámicamente el ApiKey del terminal en el Request
        var apiKeyForTerminal = tenant.ApiKeys.FirstOrDefault(k => k.ApiKeyValue == apiKey)?.ApiKeyValue;
        if (string.IsNullOrWhiteSpace(apiKeyForTerminal))
        {
            _logger.LogWarning("API Key not found for Tenant ID: {TenantId}", tenant.TenantId);
            throw new UnauthorizedAccessException("API Key not associated with this tenant.");
        }

        //todo: obtener el secreto del terminal almacenado en el keyvault de aws  roilan
        //todo: buscar en el secreto el config de apikey terminal  de pasarela asociado a ese tenant 
        
        
        // 3. Registrar log de la solicitud (NmiTransactionRequestLog)
        var requestLog = _mapper.Map<NmiTransactionRequestLog>(paymentRequest);
        //requestLog.TenantId = tenant.Id;
        await _unitOfWork.Repository<NmiTransactionRequestLog>().AddAsync(requestLog);

       
        // NMI espera que `security_key` esté incluido and created on  merchant Owner before .
        paymentRequest.SecurityKey = apiKeyForTerminal;

        // 5. Enviar el Request al Gateway NMI
        var response = await _nmiPaymentGateway.SendAsync(paymentRequest); //envia a la pasarela procesar el pago 

        _logger.LogInformation("Payment processed for Tenant {TenantId} with Response: {ResponseCode}", tenant.TenantId,
            response.Response);
        
        
        //todo: registrar en la tabla de transacciones 
        // 6. Registrar entidad de transacción (Transactions)
        // var transactionEntity = _mapper.Map<Transactions>(new CreateTransactionRequestDto
        // {
        //     TenantId = tenant.Id,
        //     TraceId = paymentRequest.TraceId,
        //     Amount = paymentRequest.Amount,
        //     TypeTransactionsId = (int)paymentRequest.TypeTransaction,
        //     CreatedBy = "system"
        // });

        // await _unitOfWork.Repository<Transactions>().AddAsync(transactionEntity);

        

        // 7. Guardar respuesta detallada (TransactionResponse)
        // var responseEntity = _mapper.Map<TransactionResponse>(response);
        // responseEntity.TransactionId = transactionEntity.TransactionsId.ToString();
        // await _unitOfWork.Repository<TransactionResponse>().AddAsync(responseEntity);

        // 8. Guardar respuesta sin procesar como log adicional (opcional)
        // var responseLog = _mapper.Map<NmiTransactionResponseLog>((response.RawResponse ?? "", requestLog.Id, response.TransactionId, response.Response, response.ResponseText));
        // await _unitOfWork.Repository<NmiTransactionResponseLog>().AddAsync(responseLog);

        // 8. Guardar todo en una sola transacción
        await _unitOfWork.SaveChangesAsync();

        return response;
    }

    public async Task<QueryResponseDto> QueryProcessPaymentAsync(string apiKey,
        QueryTransactionRequestDto queryTransactionRequest)
    {
        QueryResponseDto responseDto = new();

        // 1. Buscar Tenant
        var tenant = await _unitOfWork.TenantRepository.GetByApiKeyAsync(apiKey);
        //validar tenant y terminal id 
        if (tenant == null || !tenant.IsActive)
        {
            _logger.LogWarning("Tenant not found or inactive for ApiKey: {ApiKey}", apiKey);
            throw new UnauthorizedAccessException("Invalid API Key or inactive tenant.");
        }

        // 2. Obtener ApiKey
        var apiKeyForTerminal = tenant.ApiKeys.FirstOrDefault(k => k.ApiKeyValue == apiKey)?.ApiKeyValue;

        if (string.IsNullOrWhiteSpace(apiKeyForTerminal))
        {
            _logger.LogWarning("API Key not found for Tenant ID: {TenantId}", tenant.TenantId);
            throw new UnauthorizedAccessException("API Key not associated with this tenant.");
        }

        //set terminalId para NMI
        queryTransactionRequest.SecurityKey = apiKeyForTerminal;

        //enviar consulta a NMI 
        var response = await _nmiPaymentGateway.QueryAsync(queryTransactionRequest);

        _logger.LogInformation(
            $"Query process for Tenant {tenant.TenantId} for transactionID:  {queryTransactionRequest.TransactionId.ToString()}");
        _logger.LogInformation(" Json Paylaod: {Serialize}", JsonSerializer.Serialize(response));

        //guardar en la base de datos ,  enmascarar datos sensibles PCI DSS Complaint todo: consultar si es requerido

        //devolver la respuesta del servicio

        // 4. Serializar como string completo para persistencia segura (enmascarar si es necesario)
        //var responseXmlRaw = Utils.QueryResponseSerializer.DeserializeXml(response);

        // 5. Crear log en entidad persistente
        // var responseLog = new NmiTransactionResponseLog
        // {
        //     RequestId = Guid.NewGuid(),
        //     RawResponse = responseXmlRaw,
        //     TransactionId = response?.NmResponse?.Transaction?.TransactionId,
        //     Status = response?.NmResponse?.Transaction?.Action?.Success,
        //     Message = response?.NmResponse?.Transaction?.Action?.ResponseText,
        //     ReceivedAt = DateTime.UtcNow
        // };

        // await _unitOfWork.Repository<NmiTransactionResponseLog>().AddAsync(responseLog);

        await _unitOfWork.SaveChangesAsync();

        return response;
    }
}
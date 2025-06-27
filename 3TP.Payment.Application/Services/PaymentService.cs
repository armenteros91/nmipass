using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Helpers;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.aws;
using ThreeTP.Payment.Application.Interfaces.Payment;
using ThreeTP.Payment.Application.Queries.AwsSecrets;
using ThreeTP.Payment.Domain.Entities.Nmi;
using ThreeTP.Payment.Domain.Entities.Payments;

namespace ThreeTP.Payment.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INmiPaymentGateway _nmiPaymentGateway;
    private readonly ILogger<PaymentService> _logger;
    private readonly IAwsSecretManagerService _awsSecretManagerService;

    public PaymentService(
        INmiPaymentGateway nmiPaymentGateway,
        ILogger<PaymentService> logger,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAwsSecretManagerService awsSecretManagerService)
    {
        _nmiPaymentGateway = nmiPaymentGateway;
        _awsSecretManagerService = awsSecretManagerService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<NmiResponseDto> ProcessPaymentAsync(string apiKey, SaleTransactionRequestDto paymentRequest)
    {
        // 1. Buscar Tenant por ApiKey
        var tenant = await _unitOfWork.TenantRepository.GetByApiKeyAsync(apiKey);
        if (tenant == null || !tenant.IsActive) 
        {
            _logger.LogWarning("Tenant not found or inactive for ApiKey: {ApiKey}", apiKey);
            throw new UnauthorizedAccessException("Invalid API Key or inactive tenant.");
        }

        // 2. Validate the tenant's API key (already fetched tenant by this apiKey)
        if (tenant.ApiKey == null || tenant.ApiKey.ApiKeyValue != apiKey || !tenant.ApiKey.Status)
        {
            _logger.LogWarning("Tenant's API key does not match the provided key, is missing, or is inactive for Tenant ID: {TenantId}", tenant.TenantId);
            throw new UnauthorizedAccessException("Invalid API Key or API Key not active for this tenant.");
        }

        // 3.Obtener el terminal asociado al tenant
        if (tenant.Terminal == null)
        {
            _logger.LogWarning("Terminal not configured for Tenant ID: {TenantId}", tenant.TenantId);
            throw new ApplicationException("Terminal not configured for this tenant.");
        }
        var secretName = tenant.Terminal.SecretKeyEncrypted; 
        if (string.IsNullOrWhiteSpace(secretName))
        {
            _logger.LogWarning("Secret name not configured for Terminal ID: {TerminalId}", tenant.Terminal.TerminalId);
            throw new ApplicationException("Terminal secret name not configured.");
        }

        string nmiSecurityKey;
        try
        {
            var secretIdForAws = $"tenant/{tenant.TenantId}/terminal/{tenant.Terminal.TerminalId}/secretkey";
            _logger.LogInformation("Fetching NMI Security Key from AWS Secrets Manager for Secret ID: {SecretId}", secretIdForAws);
            var secretValueResponse = await _awsSecretManagerService.GetSecretValueAsync(new GetSecretValueQuery(secretIdForAws));
            if (secretValueResponse == null || string.IsNullOrWhiteSpace(secretValueResponse.SecretString))
            {
                _logger.LogWarning("NMI Security Key not found in AWS Secrets Manager for Secret ID: {SecretId}", secretIdForAws);
                throw new ApplicationException("NMI Security Key not found in AWS Secrets Manager.");
            }

            nmiSecurityKey = secretValueResponse.SecretString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NMI Security Key for Terminal ID: {TerminalId}", tenant.Terminal.TerminalId);
            throw new ApplicationException("Error retrieving NMI Security Key.", ex);
        }

        // 3. Registrar log de la solicitud (NmiTransactionRequestLog)
        string maskedRawRequestString =  Utils.SensitiveDataLogger.ToMaskedFormUrlEncoded(paymentRequest);
        var requestLog = _mapper.Map<NmiTransactionRequestLog>(paymentRequest);
        requestLog.NmiTransactionRequestLogId = Guid.NewGuid();
        requestLog.RawContent = maskedRawRequestString;
        await _unitOfWork.Repository<NmiTransactionRequestLog>().AddAsync(requestLog);
        await _unitOfWork.SaveChangesAsync(); 
        
        
        var typetransaccion = Guid.Parse(paymentRequest.TypeTransaction);
        //validar el tipo de transaccion 
        var transactionType = await _unitOfWork.Repository<TransactionType>()
            .GetOneAsync(x => x.TypeTransactionsId == typetransaccion);
        
        // 5. Enviar el Request al Gateway NMI
        paymentRequest.SecurityKey = nmiSecurityKey;
        paymentRequest.TypeTransaction = transactionType?.Description;
        var gatewayResponse = await _nmiPaymentGateway.SendAsync(paymentRequest); 
        
        _logger.LogInformation("Payment processed for Tenant {TenantId} with Response: {ResponseCode}", tenant.TenantId, gatewayResponse.Response);
        
        var traceId = Guid.Parse(paymentRequest.OrderId.ToString());
        
        // Crear la entidad
        var transactionEntity = _mapper.Map<Transactions>(new CreateTransactionRequestDto
        {
            TenantId = tenant.TenantId,
            TraceId = traceId,
            Amount = paymentRequest.Amount.Value,
            TypeTransactionsId = typetransaccion,
            PaymentTransactionsId = gatewayResponse.TransactionId,
            responseCode = gatewayResponse.ResponseCode,
            CreatedBy = "system"
        });
        
        await _unitOfWork.Repository<Transactions>().AddAsync(transactionEntity);

        // 7. Guardar respuesta detallada (TransactionResponse)
        var transactionResponseEntity = _mapper.Map<TransactionResponse>(gatewayResponse);
        transactionResponseEntity.FkTransactionsId = transactionEntity.TransactionsId; // FK a Transactions
        await _unitOfWork.Repository<TransactionResponse>().AddAsync(transactionResponseEntity);

        
        //  8. Guardar respuesta sin procesar como log adicional (NmiTransactionResponseLog)
        string maskedResponselogString = Utils.SensitiveDataLogger.ToMaskedFormUrlEncoded(gatewayResponse);
        var nmiResponseLog = _mapper.Map<NmiTransactionResponseLog>(gatewayResponse);
        nmiResponseLog.RequestId = requestLog.NmiTransactionRequestLogId; // FK a NmiTransactionRequestLog
        nmiResponseLog.RawResponse =   maskedResponselogString;
        await _unitOfWork.Repository<NmiTransactionResponseLog>().AddAsync(nmiResponseLog);
        await _unitOfWork.SaveChangesAsync();
        return gatewayResponse;
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

        // 2. Validate the tenant's API key and ensure it's active.
        if (tenant.ApiKey == null || tenant.ApiKey.ApiKeyValue != apiKey || !tenant.ApiKey.Status)
        {
            _logger.LogWarning(
                "Tenant's API key does not match the provided key, is missing, or is inactive for Tenant ID: {TenantId}",
                tenant.TenantId);
            throw new UnauthorizedAccessException("Invalid API Key or API Key not active for this tenant.");
        }

        if (tenant.Terminal == null)
        {
            _logger.LogWarning("Terminal not configured for Tenant ID: {TenantId}", tenant.TenantId);
            throw new ApplicationException("Terminal not configured for this tenant.");
        }

        string nmiSecurityKey;
        try
        {
            var secretIdForAws = $"terminals/{tenant.Terminal.TerminalId}/nmi_security_key"; // Adjust if necessary
            _logger.LogInformation(
                "Fetching NMI Security Key from AWS Secrets Manager for Secret ID: {SecretId} in QueryProcessPaymentAsync",
                secretIdForAws);

            var secretValueResponse =
                await _awsSecretManagerService.GetSecretValueAsync(new GetSecretValueQuery(secretIdForAws));

            if (secretValueResponse == null || string.IsNullOrWhiteSpace(secretValueResponse.SecretString))
            {
                _logger.LogWarning("NMI Security Key not found in AWS Secrets Manager for Secret ID: {SecretId}",
                    secretIdForAws);
                throw new ApplicationException("NMI Security Key not found in AWS Secrets Manager.");
            }

            nmiSecurityKey = secretValueResponse.SecretString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving NMI Security Key for Terminal ID: {TerminalId} in QueryProcessPaymentAsync",
                tenant.Terminal.TerminalId);
            throw new ApplicationException("Error retrieving NMI Security Key.", ex);
        }

        // Set NMI Security Key for the NMI request
        queryTransactionRequest.SecurityKey = nmiSecurityKey;

        //enviar consulta a NMI 
        var response = await _nmiPaymentGateway.QueryAsync(queryTransactionRequest);

        _logger.LogInformation("Query process for Tenant {TenantTenantId} for transactionID:  {TransactionId}",
            tenant.TenantId, queryTransactionRequest.TransactionId);

        _logger.LogInformation(" Json Paylaod: {Serialize}",
            JsonConvert.SerializeObject(response, formatting: Formatting.Indented));

        //guardar en la base de datos ,  enmascarar datos sensibles PCI DSS Complaint todo: consultar si es requerido

        //devolver la respuesta del servicio

        // 4. Serializar como string completo para persistencia segura (enmascarar si es necesario)
        //var responseXmlRaw = Utils.QueryResponseSerializer.DeserializeXml(Response);

        // 5. Crear log en entidad persistente
        // var responseLog = new NmiTransactionResponseLog
        // {
        //     RequestId = Guid.NewGuid(),
        //     RawResponse = responseXmlRaw,
        //     TransactionId = Response?.NmResponse?.Transaction?.TransactionId,
        //     Status = Response?.NmResponse?.Transaction?.Action?.Success,
        //     Message = Response?.NmResponse?.Transaction?.Action?.ResponseText,
        //     ReceivedAt = DateTime.UtcNow
        // };

        //await _unitOfWork.Repository<NmiTransactionResponseLog>().AddAsync(responseLog);

        await _unitOfWork.SaveChangesAsync();

        return response;
    }
}
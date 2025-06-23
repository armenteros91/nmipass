using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Queries.AwsSecrets;
using ThreeTP.Payment.Domain.Entities.Nmi;
using ThreeTP.Payment.Domain.Entities.Payments;

namespace ThreeTP.Payment.Application.Services;

public class PaymentService: IPaymentService
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
        
        // Obtener el terminal asociado al tenant
        if (tenant.Terminal == null)
        {
            _logger.LogWarning("Terminal not configured for Tenant ID: {TenantId}", tenant.TenantId);
            throw new ApplicationException("Terminal not configured for this tenant.");
        }

        // Obtener el secreto del terminal (NMI Security Key)
        // Asumimos que el SecretId para AWS Secrets Manager se almacena o se puede derivar del terminal.
        // Por ahora, usaremos un placeholder o una convención si no está directamente en la entidad Terminal.
        // Si Terminal.SecretKeyEncrypted es el nombre del secreto en AWS Secrets Manager:
        var secretName = tenant.Terminal.SecretKeyEncrypted; // Esto podría necesitar ajuste si SecretKeyEncrypted no es el nombre del secreto.
        if (string.IsNullOrWhiteSpace(secretName))
        {
            _logger.LogWarning("Secret name not configured for Terminal ID: {TerminalId}", tenant.Terminal.TerminalId);
            throw new ApplicationException("Terminal secret name not configured.");
        }

        string nmiSecurityKey;
        try
        {
            // Aquí asumimos que GetDecryptedSecretKeyAsync devuelve el valor del secreto que es el NMI Security Key.
            // Si GetDecryptedSecretKeyAsync desencripta un valor que *es* el NMI key, está bien.
            // Si el `SecretKeyEncrypted` es el *nombre* del secreto en AWS, entonces necesitamos llamar a AWSSecretManagerService.
            // Basado en el nombre `GetDecryptedSecretKeyAsync` y `SecretKeyEncrypted`, parece que el secreto ya está en la BD (encriptado).
            // El TODO original decía "obtener el secreto del terminal almacenado en el keyvault de aws".
            // Aclaración: Si el secreto está en AWS SM, necesitamos el ID/ARN del secreto.
            // Si `Terminal.SecretKeyEncrypted` es el ARN o nombre del secreto en AWS SM:

            // Vamos a asumir que `tenant.Terminal.Name` podría ser usado como parte del SecretId o que hay un campo específico.
            // Por ahora, vamos a simular que tenemos el secretId. Necesitaríamos más información de cómo se almacena.
            // Supongamos que el nombre del secreto en AWS sigue una convención como "nmi_security_key_{terminalId}"
            // O que el campo `Terminal.SecretKeyEncrypted` en realidad almacena el *nombre* del secreto en AWS SM.
            // Para el propósito de este ejemplo, si `GetDecryptedSecretKeyAsync` no es para AWS SM, lo adaptaremos.
            // El `AwsSecretManagerService` espera un `SecretId`.

            // Opción 1: El secreto está encriptado en la DB y `GetDecryptedSecretKeyAsync` lo desencripta.
            // nmiSecurityKey = await _unitOfWork.TerminalRepository.GetDecryptedSecretKeyAsync(tenant.Terminal.TerminalId);
            // if (string.IsNullOrWhiteSpace(nmiSecurityKey))
            // {
            //     _logger.LogWarning("Failed to decrypt NMI Security Key for Terminal ID: {TerminalId}", tenant.Terminal.TerminalId);
            //     throw new ApplicationException("Could not retrieve NMI Security Key.");
            // }

            // Opción 2: El secreto está en AWS Secrets Manager y `tenant.Terminal.SecretKeyEncrypted` es el ID del secreto.
            // O si hay otro campo como `tenant.Terminal.AwsSecretName`.
            // Asumamos que `tenant.Terminal.Name` es el identificador para el secreto.
            var secretIdForAws = $"terminals/{tenant.Terminal.TerminalId}/nmi_security_key"; // Esto es una suposición de cómo se podría nombrar el secreto.
                                                                                               // O si `tenant.Terminal.SecretKeyEncrypted` es el nombre del secreto en AWS SM:
                                                                                               // var secretIdForAws = tenant.Terminal.SecretKeyEncrypted;

            _logger.LogInformation("Fetching NMI Security Key from AWS Secrets Manager for Secret ID: {SecretId}", secretIdForAws);
         

            var secretValueResponse = await _awsSecretManagerService.GetSecretValueAsync(
                    new GetSecretValueQuery(secretIdForAws)
                );

            if (secretValueResponse == null || string.IsNullOrWhiteSpace(secretValueResponse.SecretString))
            {
                _logger.LogWarning("NMI Security Key not found in AWS Secrets Manager for Secret ID: {SecretId}", secretIdForAws);
                throw new ApplicationException("NMI Security Key not found in AWS Secrets Manager.");
            }
            // Asumimos que el SecretString es el NMI security key directamente o un JSON que lo contiene.
            // Si es JSON: var secretData = JsonSerializer.Deserialize<Dictionary<string, string>>(secretValueResponse.SecretString);
            // nmiSecurityKey = secretData["nmi_security_key"];
            nmiSecurityKey = secretValueResponse.SecretString; // Asumimos que es el valor directo.

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NMI Security Key for Terminal ID: {TerminalId}", tenant.Terminal.TerminalId);
            throw new ApplicationException("Error retrieving NMI Security Key.", ex);
        }
        
        // 3. Registrar log de la solicitud (NmiTransactionRequestLog)
        var requestLog = _mapper.Map<NmiTransactionRequestLog>(paymentRequest);
        requestLog.Id = tenant.TenantId; // Asociar con el ID interno del tenant
        await _unitOfWork.Repository<NmiTransactionRequestLog>().AddAsync(requestLog);
        // Importante: Para que requestLog.Id esté disponible, SaveChangesAsync podría necesitar ser llamado aquí si Id es generado por DB.
        // O si es Guid generado en código, está bien. NmiTransactionRequestLog.Id es Guid.
       
        // NMI espera que `security_key` esté incluido and created on  merchant Owner before .
        paymentRequest.SecurityKey = nmiSecurityKey; // Usar la clave obtenida de AWS SM
        
        // 5. Enviar el Request al Gateway NMI
        var nmiResponse = await _nmiPaymentGateway.SendAsync(paymentRequest); //envia a la pasarela procesar el pago

        _logger.LogInformation("Payment processed for Tenant {TenantId} with Response: {ResponseCode}", tenant.TenantId,
            nmiResponse.Response);

        // Validaciones previas //todo: el model ya tiene un fluent validations - usar el fluentvalidactionmodelo ==> 
        if (paymentRequest == null)
            throw new ArgumentNullException(nameof(paymentRequest), "El request de pago no puede ser nulo.");

        if (string.IsNullOrWhiteSpace(paymentRequest.OrderId))
            throw new ArgumentException("OrderId es obligatorio.", nameof(paymentRequest.OrderId));

        if (!Guid.TryParse(paymentRequest.OrderId, out var traceId))
            throw new ArgumentException("OrderId no es un GUID válido.", nameof(paymentRequest.OrderId));

        if (!paymentRequest.Amount.HasValue || paymentRequest.Amount <= 0)
            throw new ArgumentException("El monto debe ser mayor que cero.", nameof(paymentRequest.Amount));

  

        // Crear la entidad
        var transactionEntity = _mapper.Map<Transactions>(new CreateTransactionRequestDto
        {
            TenantId = tenant.TenantId,               // ID del tenant
            TraceId = traceId,                        // OrderId como GUID
            Amount = paymentRequest.Amount.Value,     // Monto asegurado como decimal
            TypeTransactionsId =Guid.Parse(paymentRequest.TypeTransaction), // Tipo como GUID desde BD
            CreatedBy = "system"                      // Quién crea el registro
        });
        await _unitOfWork.Repository<Transactions>().AddAsync(transactionEntity);
        // Importante: Para que transactionEntity.TransactionsId esté disponible para TransactionResponse,
        // SaveChangesAsync podría necesitar ser llamado aquí si TransactionsId es generado por DB.
        // Transactions.TransactionsId es Guid, generado en el mapper, así que está bien.

        // 7. Guardar respuesta detallada (TransactionResponse)
        var transactionResponseEntity = _mapper.Map<TransactionResponse>(nmiResponse);
        transactionResponseEntity.TransactionId = transactionEntity.TransactionsId.ToString(); // FK a Transactions
        await _unitOfWork.Repository<TransactionResponse>().AddAsync(transactionResponseEntity);

        // 8. Guardar respuesta sin procesar como log adicional (NmiTransactionResponseLog)
        var nmiResponseLog = _mapper.Map<NmiTransactionResponseLog>(nmiResponse);
        nmiResponseLog.RequestId = requestLog.Id; // FK a NmiTransactionRequestLog
        // nmiResponseLog.RawResponse ya está mapeado desde nmiResponse.RawResponse
        // nmiResponseLog.TransactionId ya está mapeado desde nmiResponse.TransactionId (el de NMI)
        // nmiResponseLog.Status ya está mapeado
        // nmiResponseLog.Message ya está mapeado
        await _unitOfWork.Repository<NmiTransactionResponseLog>().AddAsync(nmiResponseLog);

        // 9. Guardar todo en una sola transacción
        await _unitOfWork.SaveChangesAsync();

        return nmiResponse;
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
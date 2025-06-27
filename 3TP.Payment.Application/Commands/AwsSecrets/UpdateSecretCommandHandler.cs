using System.Net;
using Amazon.SecretsManager.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.aws;

namespace ThreeTP.Payment.Application.Commands.AwsSecrets;

public class UpdateSecretCommandHandler : IRequestHandler<UpdateSecretCommand, UpdateSecretResponse>
{
    private readonly IAwsSecretsProvider _awsSecretsProvider;
    private readonly IAwsSecretCacheService _cacheService;
    private readonly IAwsSecretSyncService _syncService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSecretCommandHandler> _logger;

    public UpdateSecretCommandHandler(
        IAwsSecretsProvider awsSecretsProvider,
        IAwsSecretCacheService cacheService,
        IAwsSecretSyncService syncService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSecretCommandHandler> logger)
    {
        _awsSecretsProvider = awsSecretsProvider;
        _cacheService = cacheService;
        _syncService = syncService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateSecretResponse> Handle(UpdateSecretCommand command, CancellationToken cancellationToken)
    {
        var strategy = _unitOfWork.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("Updating secret {SecretId}", command.SecretId);

                // 1. Actualizar en AWS
                var response = await _awsSecretsProvider.UpdateSecretAsync(
                    command.SecretId,
                    command.NewSecretString,
                    command.Description,
                    cancellationToken
                );

                //guardar la respuesta si el secreto se guardo en el aws 
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    // 2. Sincronizar con base de datos si aplica
                    if (command.TerminalId.HasValue)
                    {
                        await _syncService.SyncSecretToTerminalAsync(command.TerminalId.Value, command.SecretId,
                            cancellationToken);
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    // 3. Invalidar cache
                    _cacheService.Invalidate(command.SecretId);
                    _logger.LogInformation("Secret {SecretId} updated successfully", command.SecretId);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating secret {SecretId}", command.SecretId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        });
    }
}
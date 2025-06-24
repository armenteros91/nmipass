using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.aws;

namespace ThreeTP.Payment.Application.Services;

public class AwsSecretSyncService : IAwsSecretSyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AwsSecretSyncService> _logger;

    public AwsSecretSyncService(IUnitOfWork unitOfWork, ILogger<AwsSecretSyncService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SyncSecretToTerminalAsync(Guid terminalId, string secretString, CancellationToken cancellationToken)
    {
        var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalId);
        if (terminal != null)
        {
            await _unitOfWork.TerminalRepository.UpdateSecretKeyAsync(terminalId, secretString);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Secret synced to terminal {TerminalId}", terminalId);
        }
        else
        {
            _logger.LogWarning("Terminal {TerminalId} not found for secret sync", terminalId);
        }
    }
}

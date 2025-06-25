using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;

namespace ThreeTP.Payment.Application.Commands.Terminals;

public class UpdateTerminalCommandHandler : IRequestHandler<UpdateTerminalCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UpdateTerminalCommandHandler> _logger;

    public UpdateTerminalCommandHandler(
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        ILogger<UpdateTerminalCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateTerminalCommand request, CancellationToken cancellationToken)
    {
        var strategy = _unitOfWork.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            _logger.LogInformation("Starting UpdateTerminalCommand for TerminalId: {TerminalId}", request.TerminalId);

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                _logger.LogInformation("Transaction started for TerminalId: {TerminalId}", request.TerminalId);

                var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(request.TerminalId);

                if (terminal == null)
                {
                    _logger.LogWarning("Terminal with ID {TerminalId} not found", request.TerminalId);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return false;
                }

                var dto = request.UpdateRequest;

                bool updated = terminal.Update(
                    name: dto.SecretUpdate?.SecretDescription,
                    isActive: dto.TerminalUpdate.IsActive,
                    apiKey: dto.TerminalUpdate.ApiKey,
                    encrypt: _encryptionService.Encrypt,
                    hash: _encryptionService.Hash
                );

                if (updated)
                {
                    _logger.LogInformation("Terminal with ID {TerminalId} updated successfully", request.TerminalId);
                    _unitOfWork.TerminalRepository.Update(terminal);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    _logger.LogInformation("Transaction committed for TerminalId: {TerminalId}", request.TerminalId);
                }
                else
                {
                    _logger.LogInformation("No changes detected for TerminalId: {TerminalId}", request.TerminalId);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                }

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating TerminalId: {TerminalId}", request.TerminalId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
       });
    }
}
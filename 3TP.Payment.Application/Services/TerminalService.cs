using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions; // For TenantNotFoundException, TerminalNotFoundException (if created)
using ThreeTP.Payment.Application.Common.Exceptions; // For CustomValidationException
// using ThreeTP.Payment.Application.Common.Responses; // For ValidationErrorResponse - Not used in current code
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreeTP.Payment.Application.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TerminalService> _logger;
        private readonly IEncryptionService _encryptionService; // Retained for GetDecryptedSecretKeyAsync as per existing logic

        public TerminalService(
            IUnitOfWork unitOfWork,
            ILogger<TerminalService> logger,
            IEncryptionService encryptionService) // IEncryptionService kept for GetDecryptedSecretKeyAsync
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        public async Task<Terminal> CreateTerminalAsync(Terminal terminal)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));

            // Basic validation (can be expanded or use FluentValidation if triggered differently)
            if (string.IsNullOrWhiteSpace(terminal.Name))
                throw new CustomValidationException("Terminal name is required.");
            if (terminal.TenantId == Guid.Empty)
                throw new CustomValidationException("TenantId is required.");
            // In the Terminal entity, SecretKeyEncrypted is where the plain key is temporarily stored before AddAsync encrypts it.
            if (string.IsNullOrWhiteSpace(terminal.SecretKeyEncrypted))
                throw new CustomValidationException("SecretKey is required.");


            _logger.LogInformation("Creating terminal {TerminalName} for tenant {TenantId}", terminal.Name, terminal.TenantId);
            try
            {
                var tenant = await _unitOfWork.TenantRepository.GetByIdAsync(terminal.TenantId);
                if (tenant == null)
                {
                    throw new TenantNotFoundException($"Tenant with ID {terminal.TenantId} not found. Cannot create terminal.");
                }

                // TerminalRepository.AddAsync is responsible for encrypting the plain key stored in terminal.SecretKeyEncrypted
                await _unitOfWork.TerminalRepository.AddAsync(terminal);
                await _unitOfWork.CommitAsync();
                return terminal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating terminal {TerminalName}", terminal.Name);
                // Consider if _unitOfWork.RollbackTransactionAsync() is needed/appropriate here,
                // depending on IUnitOfWork.CommitAsync()'s transaction management behavior.
                throw;
            }
        }

        public async Task<Terminal?> GetTerminalByIdAsync(Guid terminalId)
        {
            _logger.LogInformation("Fetching terminal by ID {TerminalId}", terminalId);
            var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalId);
            if (terminal == null)
            {
                _logger.LogWarning("Terminal with ID {TerminalId} not found", terminalId);
            }
            return terminal;
        }

        public async Task<IEnumerable<Terminal>> GetTerminalsByTenantIdAsync(Guid tenantId)
        {
            _logger.LogInformation("Fetching terminals for Tenant ID {TenantId}", tenantId);
            return await _unitOfWork.TerminalRepository.GetByTenantIdAsync(tenantId);
        }

        public async Task<bool> UpdateTerminalAsync(Terminal terminalUpdateData)
        {
            if (terminalUpdateData == null) throw new ArgumentNullException(nameof(terminalUpdateData));
            _logger.LogInformation("Updating terminal {TerminalId}", terminalUpdateData.TerminalId);

            try
            {
                var existingTerminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalUpdateData.TerminalId);
                if (existingTerminal == null)
                {
                    _logger.LogWarning("Terminal with ID {TerminalId} not found for update.", terminalUpdateData.TerminalId);
                    return false;
                }

                existingTerminal.Name = terminalUpdateData.Name ?? existingTerminal.Name;
                existingTerminal.IsActive = terminalUpdateData.IsActive;
                // Note: Secret key updates are not handled here. Use ITerminalRepository.UpdateSecretKeyAsync for that.

                _unitOfWork.TerminalRepository.Update(existingTerminal);
                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating terminal {TerminalId}", terminalUpdateData.TerminalId);
                throw;
            }
        }

        public async Task<bool> SetActiveStatusAsync(Guid terminalId, bool isActive)
        {
            _logger.LogInformation("Setting active status to {IsActive} for terminal {TerminalId}", isActive, terminalId);
            try
            {
                var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalId);
                if (terminal == null)
                {
                    _logger.LogWarning("Terminal with ID {TerminalId} not found for SetActiveStatus.", terminalId);
                    return false;
                }

                terminal.IsActive = isActive;
                _unitOfWork.TerminalRepository.Update(terminal);
                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting active status for terminal {TerminalId}", terminalId);
                throw;
            }
        }

        public async Task<bool> DeleteTerminalAsync(Guid terminalId)
        {
            _logger.LogInformation("Deleting terminal {TerminalId}", terminalId);
            try
            {
                var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalId);
                if (terminal == null)
                {
                    _logger.LogWarning("Terminal with ID {TerminalId} not found for deletion.", terminalId);
                    return false;
                }

                _unitOfWork.TerminalRepository.Delete(terminal);
                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting terminal {TerminalId}", terminalId);
                throw;
            }
        }

        public async Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId)
        {
            _logger.LogInformation("Getting decrypted secret key for terminal ID {TerminalId}", terminalId);
            var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalId);
            if (terminal == null || string.IsNullOrEmpty(terminal.SecretKeyEncrypted))
            {
                _logger.LogWarning("Terminal {TerminalId} not found or secret key is empty for GetDecryptedSecretKeyAsync.", terminalId);
                return null;
            }
            return _encryptionService.Decrypt(terminal.SecretKeyEncrypted);
        }

        public async Task<Terminal?> FindBySecretNameAsync(string plainSecretName)
        {
            _logger.LogInformation("Finding terminal by secret name");
            return await _unitOfWork.TerminalRepository.FindBySecretNameAsync(plainSecretName);
        }
    }
}
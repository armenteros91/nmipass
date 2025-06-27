using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions; 

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public class AddApiKeyToTenantCommandHandler : IRequestHandler<AddApiKeyToTenantCommand, TenantApiKey>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddApiKeyToTenantCommandHandler> _logger;

        public AddApiKeyToTenantCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<AddApiKeyToTenantCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TenantApiKey> Handle(AddApiKeyToTenantCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to add API key for TenantId: {TenantId}", request.TenantId);

            var tenant = await _unitOfWork.TenantRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant with NmiTransactionRequestLogId {TenantId} not found", request.TenantId);
                throw new TenantNotFoundException(request.TenantId);
            }

            // The TenantApiKey constructor only takes key and tenantId.
            // Other properties like Description and Status need to be set explicitly.
            var newApiKey = new TenantApiKey(request.ApiKeyValue, request.TenantId)
            {
                Description = request.Description,
                Status = request.IsActive
            };

            try
            {
                // The AddApiKey method on the Tenant entity handles adding the key
                // and also raises the TenantApiKeyAddedEvent.
                tenant.AddApiKey(newApiKey);

                // Mark the tenant as updated. EF Core will detect the change to the ApiKey property.
                _unitOfWork.TenantRepository.Update(tenant);

                // Commit changes to the database. This will save the Tenant and the new/updated TenantApiKey,
                // and also dispatch any domain events (like TenantApiKeyAddedEvent).
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Successfully added/updated API key for TenantId: {TenantId}", request.TenantId);
                return newApiKey;
            }
            catch (InvalidTenantException ex) // Catch specific exception from tenant.AddApiKey
            {
                _logger.LogWarning(ex, "Error adding/updating API key for tenant {TenantId}: {ErrorMessage}", request.TenantId, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding API key to tenant {TenantId}", request.TenantId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken); 
                throw; 
            }
        }
    }
}

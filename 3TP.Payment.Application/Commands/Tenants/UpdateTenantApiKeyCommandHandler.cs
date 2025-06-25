using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions; // For TenantNotFoundException

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public class UpdateTenantApiKeyCommandHandler : IRequestHandler<UpdateTenantApiKeyCommand, Tenant>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTenantApiKeyCommandHandler> _logger;

        public UpdateTenantApiKeyCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateTenantApiKeyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Tenant> Handle(UpdateTenantApiKeyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to update API key for TenantId: {TenantId}", request.TenantId);

            var tenant = await _unitOfWork.TenantRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant with Id {TenantId} not found for API key update", request.TenantId);
                throw new TenantNotFoundException(request.TenantId);
            }

            if (string.IsNullOrWhiteSpace(request.NewApiKey))
            {
                _logger.LogWarning("New API key for TenantId: {TenantId} cannot be null or whitespace.", request.TenantId);
                throw new ArgumentException("New API key cannot be null or whitespace.", nameof(request.NewApiKey));
            }

            tenant.ApiKey = request.NewApiKey;
            // No domain event for ApiKey change for now, can be added if needed.

            try
            {
                _unitOfWork.TenantRepository.Update(tenant); // Ensure the repository context tracks the change.
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Successfully updated API key for TenantId: {TenantId}", request.TenantId);
                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating API key for TenantId: {TenantId}", request.TenantId);
                // Consider if rollback is needed, though CommitAsync handles transaction.
                throw;
            }
        }
    }
}

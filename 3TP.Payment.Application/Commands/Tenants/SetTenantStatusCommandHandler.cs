using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Commands.Tenants;
using ThreeTP.Payment.Domain.Abstractions.UnitOfWork;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreeTP.Payment.Application.Handlers.Tenants
{
    public class SetTenantStatusCommandHandler : IRequestHandler<SetTenantStatusCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SetTenantStatusCommandHandler> _logger;

        public SetTenantStatusCommandHandler(IUnitOfWork unitOfWork, ILogger<SetTenantStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(SetTenantStatusCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _unitOfWork.TenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant with Id {TenantId} not found for status change.", request.TenantId);
                throw new TenantNotFoundException($"Tenant with Id {request.TenantId} not found.");
            }

            if (request.IsActive)
            {
                tenant.Activate();
                _logger.LogInformation("Tenant {TenantId} activated.", request.TenantId);
            }
            else
            {
                tenant.Deactivate();
                _logger.LogInformation("Tenant {TenantId} deactivated.", request.TenantId);
            }

            try
            {
                _unitOfWork.TenantRepository.Update(tenant); // Mark entity as modified
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Tenant {TenantId} status successfully updated to IsActive: {IsActive}", request.TenantId, request.IsActive);
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for tenant {TenantId}", request.TenantId);
                // Potential rollback logic or re-throw
                throw;
            }
        }
    }
}

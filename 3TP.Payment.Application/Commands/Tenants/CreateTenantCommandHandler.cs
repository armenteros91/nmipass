using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Events;

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Tenant>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateTenantCommandHandler> _logger;

        public CreateTenantCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateTenantCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Tenant> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            // Validation is now handled by FluentValidation an ValidationBehavior pipeline
            if (await _unitOfWork.TenantRepository.CompanyCodeExistsAsync(request.CompanyCode))
            {
                _logger.LogWarning("Company code {CompanyCode} already exists", request.CompanyCode);
                throw new Exception($"Company code {request.CompanyCode} already exists");
            }

            var tenant = new Tenant(request.CompanyName, request.CompanyCode);

            try
            {
                await _unitOfWork.TenantRepository.AddAsync(tenant);
                
                tenant.AddDomainEvent(TenantActivatedEvent.Create(tenant));
                
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Tenant {CompanyName} created successfully with Id {TenantId}", tenant.CompanyName, tenant.TenantId);
                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant {CompanyName}", request.CompanyName);
                // Rollback logic might be needed here if CommitAsync fails partially
                // or if subsequent operations within a larger transaction fail.
                // For now, assume CommitAsync handles atomicity or TenantRepository.AddAsync is idempotent/safely retryable.
                throw; // Re-throw the exception to be handled by higher-level error handling
            }
        }
        // The ValidateTenant method has been removed as FluentValidation will handle this.
    }
}

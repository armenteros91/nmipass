using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Application.Helpers;
// using ThreeTP.Payment.Application.Interfaces.Tenants; // ITenantService no longer needed for AddApiKeyAsync
using ThreeTP.Payment.Domain.Events.TenantEvent;

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Tenant>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateTenantCommandHandler> _logger;
        // private readonly ITenantService _tenantService; // No longer needed for AddApiKeyAsync

        public CreateTenantCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateTenantCommandHandler> logger) // ITenantService removed
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            // _tenantService = tenantService; // No longer needed
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

            // Generate API Key and assign it directly to the tenant
            var apiKey = Utils.GenerateApiKey();
            tenant.ApiKey = apiKey; // Assign generated ApiKey

            try
            {
                await _unitOfWork.TenantRepository.AddAsync(tenant);

                // Domain event for tenant activation
                tenant.AddDomainEvent(TenantActivatedEvent.Create(tenant));

                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Tenant {CompanyName} created successfully with Id {TenantId} and APIKey {ApiKey}",
                    tenant.CompanyName, tenant.TenantId, tenant.ApiKey); // Log tenant.ApiKey
                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant {CompanyName}", request.CompanyName);
                throw;
            }
        }
    }
}
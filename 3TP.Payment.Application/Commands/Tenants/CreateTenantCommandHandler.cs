using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Application.Helpers;
using ThreeTP.Payment.Application.Interfaces.Tenants;
using ThreeTP.Payment.Domain.Events.TenantEvent;

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Tenant>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateTenantCommandHandler> _logger;
        private readonly ITenantService _tenantService;

        public CreateTenantCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateTenantCommandHandler> logger,
            ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _tenantService = tenantService;
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

                // Generate API Key and add it to the tenant
                var apiKeyString = Utils.GenerateApiKey();
                var tenantApiKey = new TenantApiKey(apiKeyString, tenant.TenantId)
                {
                    Description = $"Default API Key for {tenant.CompanyName}",
                    Status = true
                };
                tenant.AddApiKey(tenantApiKey); // This now sets the single ApiKey property on the tenant

                tenant.AddDomainEvent(TenantActivatedEvent.Create(tenant));
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Tenant {CompanyName} created successfully with NmiTransactionRequestLogId {TenantId} and APIKey {ApiKey}",
                    tenant.CompanyName, tenant.TenantId, apiKeyString); // Logging the key value for information
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
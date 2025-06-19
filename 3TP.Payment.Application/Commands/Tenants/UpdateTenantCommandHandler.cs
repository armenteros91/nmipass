using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Commands.Tenants;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.Application.Handlers.Tenants
{
    public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTenantCommandHandler> _logger;

        public UpdateTenantCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateTenantCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
        {
            // Validation for CompanyName, CompanyCode, and TenantId is now handled by FluentValidation and ValidationBehavior pipeline
            var existingTenant = await _unitOfWork.TenantRepository.GetByIdAsync(request.TenantId);
            if (existingTenant == null)
            {
                _logger.LogWarning("Tenant with Id {TenantId} not found for update.", request.TenantId);
                throw new TenantNotFoundException(request.TenantId);
            }

            // Check if CompanyCode has changed and if the new one already exists for another tenant
            if (existingTenant.CompanyCode != request.CompanyCode &&
                await _unitOfWork.TenantRepository.CompanyCodeExistsAsync(request.CompanyCode))
            {
                _logger.LogWarning("Attempted to update tenant {TenantId} with company code {CompanyCode} that already exists.", request.TenantId, request.CompanyCode);
                throw new Exception(request.CompanyCode);
            }

            //existingTenant.Update(request.CompanyName, request.CompanyCode);

            try
            {
                _unitOfWork.TenantRepository.Update(existingTenant); // Assuming Update is a synchronous method marking entity as modified
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Tenant {TenantId} updated successfully. New CompanyName: {CompanyName}, New CompanyCode: {CompanyCode}",
                    request.TenantId, request.CompanyName, request.CompanyCode);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant {TenantId}", request.TenantId);
                // Potential rollback logic or re-throw
                throw;
            }
        }
    }
}

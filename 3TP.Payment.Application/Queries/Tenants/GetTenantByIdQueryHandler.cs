using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Domain.Abstractions.UnitOfWork;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.Application.Queries.Tenants
{
    public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Tenant>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTenantByIdQueryHandler> _logger;

        public GetTenantByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTenantByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Tenant> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching tenant by Id {TenantId}.", request.TenantId);
            var tenant = await _unitOfWork.TenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant with Id {TenantId} not found.", request.TenantId);
                throw new TenantNotFoundException($"Tenant with Id {request.TenantId} not found.");
            }

            _logger.LogInformation("Successfully fetched tenant with Id {TenantId}.", request.TenantId);
            return tenant;
        }
    }
}

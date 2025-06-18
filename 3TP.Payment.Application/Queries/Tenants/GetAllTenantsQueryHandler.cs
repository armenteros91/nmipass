using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Domain.Abstractions.UnitOfWork;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Queries.Tenants
{
    public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, IEnumerable<Tenant>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllTenantsQueryHandler> _logger;

        public GetAllTenantsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllTenantsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Tenant>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all tenants.");
            var tenants = await _unitOfWork.TenantRepository.GetAllAsync(cancellationToken);
            _logger.LogInformation("Successfully fetched {TenantCount} tenants.", tenants?.Count() ?? 0);
            return tenants;
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Queries.Tenants
{
    public class GetTenantByApiKeyQueryHandler : IRequestHandler<GetTenantByApiKeyQuery, Tenant?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTenantByApiKeyQueryHandler> _logger;

        public GetTenantByApiKeyQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTenantByApiKeyQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Tenant?> Handle(GetTenantByApiKeyQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey))
            {
                _logger.LogWarning("API key is null or whitespace in GetTenantByApiKeyQuery. Returning null.");
                return null;
            }

            _logger.LogInformation("Fetching tenant by API Key."); // Avoid logging API key itself
            var tenant = await _unitOfWork.TenantRepository.GetByApiKeyAsync(request.ApiKey);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant not found for the provided API Key.");
            }
            else
            {
                _logger.LogInformation("Successfully fetched tenant with Id {TenantId} using API Key.", tenant.TenantId);
            }
            return tenant;
        }
    }
}

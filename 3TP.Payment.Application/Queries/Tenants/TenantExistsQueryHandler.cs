using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;

namespace ThreeTP.Payment.Application.Queries.Tenants
{
    public class TenantExistsQueryHandler : IRequestHandler<TenantExistsQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TenantExistsQueryHandler> _logger;

        public TenantExistsQueryHandler(IUnitOfWork unitOfWork, ILogger<TenantExistsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(TenantExistsQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                _logger.LogInformation("CompanyCode is null or whitespace in TenantExistsQuery. Returning false.");
                return false;
            }

            _logger.LogInformation("Checking if tenant exists with CompanyCode {CompanyCode}.", request.CompanyCode);
            var exists = await _unitOfWork.TenantRepository.CompanyCodeExistsAsync(request.CompanyCode);

            if (exists)
            {
                _logger.LogInformation("Tenant with CompanyCode {CompanyCode} exists.", request.CompanyCode);
            }
            else
            {
                _logger.LogInformation("Tenant with CompanyCode {CompanyCode} does not exist.", request.CompanyCode);
            }
            return exists;
        }
    }
}

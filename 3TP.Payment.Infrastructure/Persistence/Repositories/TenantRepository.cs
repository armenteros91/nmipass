using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.Interfaces.Tenants;
using ThreeTP.Payment.Domain.Entities.Tenant;
using Tenant = ThreeTP.Payment.Domain.Entities.Tenant.Tenant;

namespace ThreeTP.Payment.Infrastructure.Persistence.Repositories
{
    public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
    {
        private readonly NmiDbContext _context;
        private readonly ILogger<TenantRepository> _logger;

        public TenantRepository(
            NmiDbContext context,
            ILogger<TenantRepository> logger)
            : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Tenant?> GetByIdAsync(Guid id) => await GetOneAsync(t => t.TenantId == id);

        public Task<Tenant?> GetByApiKeyAsync(string apiKey) => GetOneAsync(t => t.ApiKeys.Equals(apiKey));

        public async Task<bool> CompanyCodeExistsAsync(string companyCode) =>
            await ExistsAsync(tenant => tenant.CompanyCode==companyCode);

        public void Update(Tenant tenant)
        {
            _context.Update(tenant); // track for save entity
        }

        public void Addapikey(TenantApiKey tenantApiKey)
        {
            _context.Add(tenantApiKey);
        }
    }

}
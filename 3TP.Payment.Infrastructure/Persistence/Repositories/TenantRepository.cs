using Microsoft.EntityFrameworkCore; // Required for Include
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces.Tenants;
using ThreeTP.Payment.Domain.Entities.Tenant;

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

        public async Task<Tenant?> GetByIdAsync(Guid id) =>
            await GetOneAsync(t => t.TenantId == id, query => query.Include(t => t.Terminal));

        public Task<Tenant?> GetByApiKeyAsync(string apiKey) =>
            GetOneAsync(t => t.ApiKey == apiKey, query => query.Include(t => t.Terminal));


        public async Task<bool> CompanyCodeExistsAsync(string companyCode) =>
            await ExistsAsync(tenant => tenant.CompanyCode == companyCode);

        public void Update(Tenant tenant)
        {
            _context.Update(tenant); // track for save entity
        }

        // Addapikey method removed as TenantApiKey entity and its direct repository manipulation are gone.
    }
}
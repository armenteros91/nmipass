using ThreeTP.Payment.Application.Interfaces.Repository;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Interfaces.Tenants;

public interface ITenantRepository : IGenericRepository<Tenant>
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByApiKeyAsync(string apiKey); // Signature remains the same, implementation will change
    
    Task<bool> CompanyCodeExistsAsync(string companyCode);
    
    void Update(Tenant tenant);

    // void Addapikey(TenantApiKey tenantApiKey); // Removed
}
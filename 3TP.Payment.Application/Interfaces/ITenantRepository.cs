using System;
using System.Threading.Tasks;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Interfaces;

public interface ITenantRepository : IGenericRepository<Tenant>
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByApiKeyAsync(string apiKey);
    
    Task<bool> CompanyCodeExistsAsync(string companyCode);
    
    void Update(Tenant tenant);

    void Addapikey(TenantApiKey tenantApiKey);
}
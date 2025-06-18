using ThreeTP.Payment.Domain.Commons;

namespace ThreeTP.Payment.Domain.Entities.Tenant;

public abstract class TenantApiKey : BaseEntity
{
    public Guid TenantApikeyId { get; set; }
    
    public string ApiKeyValue { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public string? Description { get; set; }
    public bool Status { get; set; }
    
    protected TenantApiKey() { }

    public TenantApiKey(string key, Guid tenantId)
    {
        ApiKeyValue = key;
        TenantId = tenantId;
    }
}
using ThreeTP.Payment.Domain.Commons;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.Domain.Events.TenantEvent;

public record TenantApiKeyAddedEvent : IDomainEvent
{
    public Tenant Tenant { get; }
    public TenantApiKey ApiKey { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    private TenantApiKeyAddedEvent(Tenant tenant, TenantApiKey apiKey)
    {
        Tenant = ValidateTenant(tenant);
        ApiKey = ValidateApiKey(apiKey);
    }

    public static TenantApiKeyAddedEvent Create(Tenant tenant, TenantApiKey apiKey)
    {
        return new TenantApiKeyAddedEvent(tenant, apiKey);
    }

    private static Tenant ValidateTenant(Tenant tenant)
    {
        if (tenant == null)
            throw new InvalidTenantException(nameof(Tenant), "Tenant cannot be null");
        return tenant;
    }

    private static TenantApiKey ValidateApiKey(TenantApiKey apiKey)
    {
        if (apiKey == null)
            throw new InvalidTenantException(nameof(ApiKey), "ApiKey cannot be null");
        return apiKey;
    }
}
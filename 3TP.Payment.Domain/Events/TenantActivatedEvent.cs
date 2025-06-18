using ThreeTP.Payment.Domain.Commons;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.Domain.Events;

public record TenantActivatedEvent : IDomainEvent
{
    public Tenant Tenant { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    private TenantActivatedEvent(Tenant tenant)
    {
        Tenant = ValidateTenant(tenant);
    }

    public static TenantActivatedEvent Create(Tenant tenant)
    {
        return new TenantActivatedEvent(tenant);
    }

    private static Tenant ValidateTenant(Tenant tenant)
    {
        if (tenant == null)
            throw new InvalidTenantException(nameof(Tenant), "Tenant cannot be null");
        return tenant;
    }
}
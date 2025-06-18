using ThreeTP.Payment.Domain.Commons;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Domain.Exceptions;

namespace ThreeTP.Payment.Domain.Events;

public record TenantDeactivatedEvent : IDomainEvent
{
    public Tenant Tenant { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    private TenantDeactivatedEvent(Tenant tenant)
    {
        Tenant = ValidateTenant(tenant);
    }

    public static TenantDeactivatedEvent Create(Tenant tenant)
    {
        return new TenantDeactivatedEvent(tenant);
    }

    private static Tenant ValidateTenant(Tenant tenant)
    {
        if (tenant == null)
            throw new InvalidTenantException(nameof(Tenant), "Tenant cannot be null");
        return tenant;
    }
}
using ThreeTP.Payment.Domain.Commons;

namespace ThreeTP.Payment.Application.Interfaces;

/// <summary>
/// contrato de eventos del dominio
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<BaseEntityWithEvents> entitiesWithEvents);
    
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
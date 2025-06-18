using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Commons;

namespace ThreeTP.Payment.Infrastructure.Events
{
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DomainEventDispatcher> _logger;

        public DomainEventDispatcher(IMediator mediator,
            ILogger<DomainEventDispatcher> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task DispatchAsync(IEnumerable<BaseEntityWithEvents> entitiesWithEvents)
        {
            var withEvents = entitiesWithEvents.ToList();
            var domainEvents = withEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent);
            }

            foreach (var entity in withEvents)
            {
                entity.ClearDomainEvents();
            }
        }

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            var eventType = domainEvent.GetType();
            _logger.LogDebug("Publishing domain event: {EventType}", eventType.Name);
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
using MediatR;
using Microsoft.Extensions.Logging;

namespace ThreeTP.Payment.Domain.Events.TenantEvent;

public class TenantActivatedEventHandler :INotificationHandler<TenantActivatedEvent>
{
    private readonly ILogger<TenantActivatedEventHandler> _logger;

    public TenantActivatedEventHandler(ILogger<TenantActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TenantActivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled TenantActivatedEvent for tenant {TenantId}", notification.Tenant.TenantId);
        return Task.CompletedTask;
    }
}
using MediatR;

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public record SetTenantStatusCommand(Guid TenantId, bool IsActive) : IRequest<Unit>;
}

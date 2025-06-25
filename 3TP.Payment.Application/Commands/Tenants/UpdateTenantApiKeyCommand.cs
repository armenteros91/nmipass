using MediatR;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public record UpdateTenantApiKeyCommand(Guid TenantId, string NewApiKey) : IRequest<Tenant>;
}

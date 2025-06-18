using MediatR;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Queries.Tenants
{
    public record GetTenantByApiKeyQuery(string ApiKey) : IRequest<Tenant?>;
}

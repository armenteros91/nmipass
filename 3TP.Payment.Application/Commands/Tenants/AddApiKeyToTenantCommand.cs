using MediatR;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public record AddApiKeyToTenantCommand(
        Guid TenantId,
        string ApiKeyValue,
        string? Description,
        bool IsActive // Added IsActive based on plan step 3, ensuring consistency
    ) : IRequest<TenantApiKey>;
}

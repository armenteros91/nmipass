using MediatR;

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public record UpdateTenantCommand(Guid TenantId, string CompanyName, string CompanyCode) : IRequest<Unit>;
}

using MediatR;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Commands.Tenants
{
    public record CreateTenantCommand(string CompanyName, string CompanyCode) : IRequest<Tenant>;
}

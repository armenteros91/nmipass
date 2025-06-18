using MediatR;

namespace ThreeTP.Payment.Application.Queries.Tenants
{
    public record TenantExistsQuery(string CompanyCode) : IRequest<bool>;
}

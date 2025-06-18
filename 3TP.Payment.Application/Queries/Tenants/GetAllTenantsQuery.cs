using MediatR;
using System.Collections.Generic;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Queries.Tenants
{
    public record GetAllTenantsQuery : IRequest<IEnumerable<Tenant>>;
}

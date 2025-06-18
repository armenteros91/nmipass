using MediatR;
using System;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Queries.Tenants
{
    public record GetTenantByIdQuery(Guid TenantId) : IRequest<Tenant>;
}

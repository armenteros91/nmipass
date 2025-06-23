using MediatR;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using System;

namespace ThreeTP.Payment.Application.Queries.Terminals
{
    public class GetTerminalByTenantIdQuery : IRequest<TerminalResponseDto?>
    {
        public Guid TenantId { get; }

        public GetTerminalByTenantIdQuery(Guid tenantId)
        {
            TenantId = tenantId;
        }
    }
}

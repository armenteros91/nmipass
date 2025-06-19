using MediatR;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using System;
using System.Collections.Generic;

namespace ThreeTP.Payment.Application.Queries.Terminals
{
    public class GetTerminalsByTenantIdQuery : IRequest<IEnumerable<TerminalResponseDto>>
    {
        public Guid TenantId { get; }

        public GetTerminalsByTenantIdQuery(Guid tenantId)
        {
            TenantId = tenantId;
        }
    }
}

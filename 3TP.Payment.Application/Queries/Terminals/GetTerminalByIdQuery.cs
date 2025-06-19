using MediatR;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using System;

namespace ThreeTP.Payment.Application.Queries.Terminals
{
    public class GetTerminalByIdQuery : IRequest<TerminalResponseDto?>
    {
        public Guid TerminalId { get; }

        public GetTerminalByIdQuery(Guid terminalId)
        {
            TerminalId = terminalId;
        }
    }
}

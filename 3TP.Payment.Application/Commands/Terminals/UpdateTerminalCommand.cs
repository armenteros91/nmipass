using MediatR;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using System;

namespace ThreeTP.Payment.Application.Commands.Terminals
{
    public class UpdateTerminalCommand : IRequest<bool> // Returns true if update was successful
    {
        public Guid TerminalId { get; }
        public UpdateTerminalRequestDto UpdateRequest { get; }

        public UpdateTerminalCommand(Guid terminalId, UpdateTerminalRequestDto updateRequest)
        {
            TerminalId = terminalId;
            UpdateRequest = updateRequest;
        }
    }
}

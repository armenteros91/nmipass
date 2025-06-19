using MediatR;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;

namespace ThreeTP.Payment.Application.Commands.Terminals
{
    public class CreateTerminalCommand : IRequest<TerminalResponseDto>
    {
        public CreateTerminalRequestDto TerminalRequest { get; }

        public CreateTerminalCommand(CreateTerminalRequestDto terminalRequest)
        {
            TerminalRequest = terminalRequest;
        }
    }
}

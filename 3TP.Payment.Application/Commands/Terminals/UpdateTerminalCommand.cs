using MediatR;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;

namespace ThreeTP.Payment.Application.Commands.Terminals;

public class UpdateTerminalCommand : IRequest<bool>
{
    public Guid TerminalId { get; set; }
    public UpdateTerminalAndSecretRequest UpdateRequest { get; }

    public UpdateTerminalCommand(Guid terminalId, UpdateTerminalAndSecretRequest updateRequest)
    {
        TerminalId = terminalId;
        UpdateRequest = updateRequest;
    }
        
}
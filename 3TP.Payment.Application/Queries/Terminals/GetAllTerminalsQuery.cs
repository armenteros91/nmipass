using MediatR;
using System.Collections.Generic;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;

namespace ThreeTP.Payment.Application.Queries.Terminals
{
    public class GetAllTerminalsQuery : IRequest<List<TerminalResponseDto>>
    {
        // No parameters needed
    }
}

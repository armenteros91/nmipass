using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Application.Interfaces.Terminals; // Assuming ITerminalRepository or similar

namespace ThreeTP.Payment.Application.Queries.Terminals
{
    public class GetAllTerminalsQueryHandler : IRequestHandler<GetAllTerminalsQuery, List<TerminalResponseDto>>
    {
        // Assuming you have a repository or service to fetch all terminals.
        // For this example, let's use ITerminalService, though direct repository access might be more CQRS pure for queries.
        private readonly ITerminalService _terminalService;
        private readonly ILogger<GetAllTerminalsQueryHandler> _logger;

        public GetAllTerminalsQueryHandler(ITerminalService terminalService, ILogger<GetAllTerminalsQueryHandler> logger)
        {
            _terminalService = terminalService;
            _logger = logger;
        }

        public async Task<List<TerminalResponseDto>> Handle(GetAllTerminalsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetAllTerminalsQuery");
            try
            {
                // The existing ITerminalService.GetAllTerminalsAsync() already returns List<TerminalResponseDto>
                var terminals = await _terminalService.GetAllTerminalsAsync();
                return terminals;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error handling GetAllTerminalsQuery");
                throw; // Rethrow or handle as per application's error handling strategy
            }
        }
    }
}

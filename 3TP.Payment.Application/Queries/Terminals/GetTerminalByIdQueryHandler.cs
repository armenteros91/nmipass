using MediatR;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace ThreeTP.Payment.Application.Queries.Terminals
{
    public class GetTerminalByIdQueryHandler : IRequestHandler<GetTerminalByIdQuery, TerminalResponseDto?>
    {
        private readonly ITerminalRepository _terminalRepository;
        private readonly IMapper _mapper;

        public GetTerminalByIdQueryHandler(ITerminalRepository terminalRepository, IMapper mapper)
        {
            _terminalRepository = terminalRepository;
            _mapper = mapper;
        }

        public async Task<TerminalResponseDto?> Handle(GetTerminalByIdQuery request, CancellationToken cancellationToken)
        {
            var terminal = await _terminalRepository.GetByIdAsync(request.TerminalId);
            return terminal == null ? null : _mapper.Map<TerminalResponseDto>(terminal);
        }
    }
}

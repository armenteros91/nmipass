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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTerminalByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TerminalResponseDto?> Handle(GetTerminalByIdQuery request, CancellationToken cancellationToken)
        {
            var terminalRepository = _unitOfWork.TerminalRepository;
            var terminal = await terminalRepository.GetByIdAsync(request.TerminalId);
            return terminal == null ? null : _mapper.Map<TerminalResponseDto>(terminal);
        }
    }
}

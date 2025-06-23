using MediatR;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace ThreeTP.Payment.Application.Queries.Terminals
{
    public class GetTerminalsByTenantIdQueryHandler : IRequestHandler<GetTerminalsByTenantIdQuery, IEnumerable<TerminalResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTerminalsByTenantIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TerminalResponseDto>> Handle(GetTerminalsByTenantIdQuery request, CancellationToken cancellationToken)
        {
            var terminalRepository = _unitOfWork.TerminalRepository;
            var terminals = await terminalRepository.GetByTenantIdAsync(request.TenantId);
            return _mapper.Map<IEnumerable<TerminalResponseDto>>(terminals);
        }
    }
}

using MediatR;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace ThreeTP.Payment.Application.Queries.Terminals
{
    public class GetTerminalByTenantIdQueryHandler : IRequestHandler<GetTerminalByTenantIdQuery, TerminalResponseDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTerminalByTenantIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TerminalResponseDto?> Handle(GetTerminalByTenantIdQuery request, CancellationToken cancellationToken)
        {
            var terminal = await _unitOfWork.TerminalRepository.GetByTenantIdAsync(request.TenantId);
            if (terminal == null)
            {
                return null;
            }
            return _mapper.Map<TerminalResponseDto>(terminal);
        }
    }
}

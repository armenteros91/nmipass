using MediatR;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Domain.Exceptions; // Assuming you have a TenantNotFoundException
using AutoMapper; // You'll need to add AutoMapper for mapping

namespace ThreeTP.Payment.Application.Commands.Terminals
{
    public class CreateTerminalCommandHandler : IRequestHandler<CreateTerminalCommand, TerminalResponseDto>
    {
        private readonly ITerminalRepository _terminalRepository;
        private readonly ITenantRepository _tenantRepository; // To verify TenantId
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTerminalCommandHandler(
            ITerminalRepository terminalRepository,
            ITenantRepository tenantRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _terminalRepository = terminalRepository;
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TerminalResponseDto> Handle(CreateTerminalCommand request, CancellationToken cancellationToken)
        {
            var tenantExists = await _tenantRepository.GetByIdAsync(request.TerminalRequest.TenantId);
            if (tenantExists == null)
            {
                // Or a more specific exception like TenantNotFoundException(request.TerminalRequest.TenantId)
                throw new TenantNotFoundException(request.TerminalRequest.TenantId);
            }

            var terminal = new Terminal(
                request.TerminalRequest.Name,
                request.TerminalRequest.TenantId,
                request.TerminalRequest.SecretKey // The repository will handle encryption
            );

            await _terminalRepository.AddAsync(terminal);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<TerminalResponseDto>(terminal);
        }
    }
}

using MediatR;
using ThreeTP.Payment.Application.Interfaces; // For ITerminalService
using ThreeTP.Payment.Domain.Entities.Tenant; // For Terminal entity
using ThreeTP.Payment.Application.DTOs.Responses.Terminals; // For TerminalResponseDto
using System.Threading;
using System.Threading.Tasks;
using AutoMapper; // For IMapper
// TenantNotFoundException might be thrown by the service now, so direct check might not be needed here.
// Using ThreeTP.Payment.Application.DTOs.Requests.Terminals; // Implicitly used by CreateTerminalCommand

namespace ThreeTP.Payment.Application.Commands.Terminals
{
    public class CreateTerminalCommandHandler : IRequestHandler<CreateTerminalCommand, TerminalResponseDto>
    {
        private readonly ITerminalService _terminalService;
        private readonly IMapper _mapper;

        public CreateTerminalCommandHandler(
            ITerminalService terminalService, // Changed dependencies
            IMapper mapper)
        {
            _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<TerminalResponseDto> Handle(CreateTerminalCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.TerminalRequest == null) throw new ArgumentNullException(nameof(request.TerminalRequest), "TerminalRequest cannot be null.");

            // The command carries CreateTerminalRequestDto.
            // The service expects a Terminal domain entity.
            // The TerminalMappingProfile maps CreateTerminalRequestDto to Terminal,
            // using a constructor that takes (name, tenantId, secretKey).
            // The 'secretKey' from DTO is passed to this constructor.
            // The Terminal entity's constructor assigns this plain key to its SecretKeyEncrypted property (as a temporary holder).
            // TerminalService then passes this Terminal entity to TerminalRepository.AddAsync,
            // which encrypts the value in SecretKeyEncrypted.
            var terminalToCreate = _mapper.Map<Terminal>(request.TerminalRequest);

            // The service will handle tenant existence check and actual creation logic.
            // It will also handle any exceptions like TenantNotFoundException.
            var createdTerminal = await _terminalService.CreateTerminalAsync(terminalToCreate);

            // The service returns a Terminal domain entity.
            // This handler needs to map it to TerminalResponseDto.
            return _mapper.Map<TerminalResponseDto>(createdTerminal);
        }
    }
}

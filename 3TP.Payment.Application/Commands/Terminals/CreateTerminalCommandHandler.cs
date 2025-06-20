using MediatR;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using System.Threading;
using System.Threading.Tasks;
using ThreeTP.Payment.Domain.Exceptions; // Assuming you have a TenantNotFoundException
using AutoMapper; // You'll need to add AutoMapper for mapping
using ThreeTP.Payment.Application.Commands.AwsSecrets; // Added for CreateSecretCommand

namespace ThreeTP.Payment.Application.Commands.Terminals
{
    public class CreateTerminalCommandHandler : IRequestHandler<CreateTerminalCommand, TerminalResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAwsSecretManagerService _awsSecretManagerService; // New field

        public CreateTerminalCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAwsSecretManagerService awsSecretManagerService) // New parameter
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _awsSecretManagerService = awsSecretManagerService; // Assign new parameter
        }

        public async Task<TerminalResponseDto> Handle(CreateTerminalCommand request, CancellationToken cancellationToken)
        {
            var tenantRepository = _unitOfWork.TenantRepository;
            var terminalRepository = _unitOfWork.TerminalRepository;

            var tenantExists = await tenantRepository.GetByIdAsync(request.TerminalRequest.TenantId);
            if (tenantExists == null)
            {
                throw new TenantNotFoundException(request.TerminalRequest.TenantId);
            }

            var terminal = new Terminal(
                request.TerminalRequest.Name,
                request.TerminalRequest.TenantId,
                request.TerminalRequest.SecretKey
            );

            await terminalRepository.AddAsync(terminal);

            var secretName = $"tenant/{terminal.TenantId}/terminal/{terminal.TerminalId}/secretkey";
            var createSecretCommand = new CreateSecretCommand(
                     secretName,
                     request.TerminalRequest.SecretKey,
                     $"Secret key for terminal {terminal.TerminalId}",
                     terminal.TerminalId
                 );

            try
            {
                await _awsSecretManagerService.CreateSecretAsync(createSecretCommand, cancellationToken);
                return _mapper.Map<TerminalResponseDto>(terminal);
            }
            catch (Exception ex)
            {
                // Consider logging the exception with context here.
                // Example: _logger.LogError(ex, "Error during terminal creation and secret storage for tenant {TenantId}, terminal name {TerminalName}", request.TerminalRequest.TenantId, request.TerminalRequest.Name);
                throw; // Rethrow to allow higher-level error handlers to manage it.
                       // The transaction should have been rolled back by CreateSecretAsync.
            }
        }
    }
}

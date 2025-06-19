using MediatR; // Added
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals; // Added
using ThreeTP.Payment.Application.DTOs.Responses.Terminals; // Added
using ThreeTP.Payment.Application.Commands.Terminals; // Added
using ThreeTP.Payment.Application.Queries.Terminals; // Added
using System; // Added
using System.Collections.Generic; // Added
using System.Threading.Tasks; // Added


namespace ThreeTP.Payment.Application.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly ITerminalRepository _terminalRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IMediator _mediator; // Added

        public TerminalService(
            ITerminalRepository terminalRepository,
            IEncryptionService encryptionService,
            IMediator mediator) // Added mediator
        {
            _terminalRepository = terminalRepository;
            _encryptionService = encryptionService;
            _mediator = mediator; // Added
        }

        public async Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId)
        {
            var terminal = await _terminalRepository.GetByIdAsync(terminalId);
            return terminal != null
                ? _encryptionService.Decrypt(terminal.SecretKeyEncrypted)
                : null;
        }

        public async Task<Terminal?> FindBySecretNameAsync(string plainSecretName)
        {
            // This method in the repository seems to directly use the plainSecretName to find by hash.
            // If this is intended to use an encrypted version or another logic, it should be adjusted.
            // For now, keeping it as is, assuming the repository handles it correctly.
            return await _terminalRepository.FindBySecretNameAsync(plainSecretName);
        }

        // New method implementations
        public async Task<TerminalResponseDto> CreateTerminalAsync(CreateTerminalRequestDto createRequest)
        {
            return await _mediator.Send(new CreateTerminalCommand(createRequest));
        }

        public async Task<TerminalResponseDto?> GetTerminalByIdAsync(Guid terminalId)
        {
            return await _mediator.Send(new GetTerminalByIdQuery(terminalId));
        }

        public async Task<IEnumerable<TerminalResponseDto>> GetTerminalsByTenantIdAsync(Guid tenantId)
        {
            return await _mediator.Send(new GetTerminalsByTenantIdQuery(tenantId));
        }

        public async Task<bool> UpdateTerminalAsync(Guid terminalId, UpdateTerminalRequestDto updateRequest)
        {
            return await _mediator.Send(new UpdateTerminalCommand(terminalId, updateRequest));
        }
    }
}
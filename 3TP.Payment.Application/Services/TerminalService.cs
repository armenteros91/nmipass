using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Commands.AwsSecrets;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Application.Commands.Terminals;
using ThreeTP.Payment.Application.Interfaces.Terminals;
using ThreeTP.Payment.Application.Queries.Terminals;

namespace ThreeTP.Payment.Application.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly IMediator _mediator;
        private readonly ILogger<TerminalService> _logger;

        public TerminalService(
            IUnitOfWork unitOfWork,
            IEncryptionService encryptionService,
            IMediator mediator,
            ILogger<TerminalService> logger)
        {
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId)
        {
            var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalId);
            return terminal != null
                ? _encryptionService.Decrypt(terminal.SecretKeyEncrypted)
                : null;
        }

        public async Task<Terminal?> FindBySecretNameAsync(string plainSecretName)
        {
            return await _unitOfWork.TerminalRepository.FindBySecretNameAsync(plainSecretName);
        }


        public async Task<TerminalResponseDto> CreateTerminalAsync(CreateTerminalRequestDto createRequest)
        {
            return await _mediator.Send(new CreateTerminalCommand(createRequest));
        }

        public async Task<TerminalResponseDto?> GetTerminalByIdAsync(Guid terminalId)
        {
            return await _mediator.Send(new GetTerminalByIdQuery(terminalId));
        }

        public async Task<TerminalResponseDto?> GetTerminalByTenantIdAsync(Guid tenantId)
        {
            return await _mediator.Send(new GetTerminalByTenantIdQuery(tenantId));
        }

        public async Task<bool> UpdateTerminalAsync(Guid terminalId, UpdateTerminalAndSecretRequest updateRequest)
        {
            return await _mediator.Send(new UpdateTerminalCommand(terminalId, updateRequest));
        }

        public async Task<bool> UpdateTerminalAndSecretAsync(UpdateTerminalCommand terminalCommand,
            string? newSecretString, string? secretId, string? description, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting coordinated update for TerminalId: {TerminalId}",
                terminalCommand.TerminalId);

            // 1. Actualizar la terminal
            var terminalUpdated = await _mediator.Send(terminalCommand, cancellationToken);

            if (!terminalUpdated)
            {
                _logger.LogWarning("Terminal update failed for TerminalId: {TerminalId}", terminalCommand.TerminalId);
                return false;
            }

            // 2. Si hay un nuevo secreto, actualizar en AWS
            if (!string.IsNullOrWhiteSpace(newSecretString) && !string.IsNullOrWhiteSpace(secretId))
            {
                var updateSecretCommand = new UpdateSecretCommand
                {
                    SecretId = secretId,
                    NewSecretString = newSecretString,
                    Description = description,
                    TerminalId = terminalCommand.TerminalId
                };

                await _mediator.Send(updateSecretCommand, cancellationToken);
                _logger.LogInformation("Secret updated for TerminalId: {TerminalId}", terminalCommand.TerminalId);
            }

            return true;
        }

        /// <summary>
        /// get all terminals info
        /// </summary>
        /// <returns></returns>
        public async Task<List<TerminalResponseDto>> GetAllTerminalsAsync()
        {
            var terminals = await _unitOfWork.TerminalRepository.GetAllAsync();

            return terminals.Select(t => new TerminalResponseDto
            {
                TerminalId = t.TerminalId,
                TenantId = t.TenantId,
                Name = t.Name
            }).ToList();
        }
    }
}
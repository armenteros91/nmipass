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

        /// <summary>
        /// actualiza un secreto asociado a un Terminal
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="updateRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> UpdateTerminalAndSecretAsync(Guid terminalId,
            UpdateTerminalAndSecretRequest updateRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting coordinated update for TerminalId: {TerminalId}", terminalId);

            var terminal = await _unitOfWork.TerminalRepository.GetByIdAsync(terminalId);
            if (terminal == null)
            {
                _logger.LogWarning("Terminal with ID {TerminalId} not found");
                return false;
            }

            var terminalCommand = new UpdateTerminalCommand(terminalId, updateRequest);

            var terminalUpdated = await _mediator.Send(terminalCommand, cancellationToken);
            if (!terminalUpdated)
            {
                _logger.LogWarning("Terminal update failed for TerminalId: {TerminalId}", terminalId);
                return false;
            }

            if (updateRequest.SecretUpdate is { NewSecretString: not null, SecretId: not null })
            {
                var updateSecretCommand = new UpdateSecretCommand
                {
                    SecretId = updateRequest.SecretUpdate.SecretId,
                    NewSecretString = updateRequest.SecretUpdate.NewSecretString,
                    Description = updateRequest.SecretUpdate.SecretDescription,
                    TerminalId = terminalId
                };

                await _mediator.Send(updateSecretCommand, cancellationToken);
                _logger.LogInformation("Secret updated for TerminalId: {TerminalId}", terminalId);
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
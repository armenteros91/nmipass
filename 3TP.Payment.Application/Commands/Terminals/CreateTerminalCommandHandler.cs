using MediatR;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Domain.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Commands.AwsSecrets;
using ThreeTP.Payment.Application.Interfaces.aws;

namespace ThreeTP.Payment.Application.Commands.Terminals;

public class CreateTerminalCommandHandler : IRequestHandler<CreateTerminalCommand, TerminalResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAwsSecretManagerService _awsSecretManagerService;
    private readonly ILogger<CreateTerminalCommandHandler> _logger;

    /// <summary>
    /// Handles the creation of a terminal for a tenant and stores its secret key in AWS Secrets Manager. 
    /// </summary>
    /// <param name="unitOfWork"></param>
    /// <param name="mapper"></param>
    /// <param name="awsSecretManagerService"></param>
    /// <param name="logger"></param>
    public CreateTerminalCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAwsSecretManagerService awsSecretManagerService,
        ILogger<CreateTerminalCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _awsSecretManagerService =
            awsSecretManagerService ?? throw new ArgumentNullException(nameof(awsSecretManagerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a terminal for the specified tenant and stores its secret key in AWS Secrets Manager.
    /// </summary>
    /// <param name="request">The command containing the terminal creation details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A DTO representing the created terminal.</returns>
    /// <exception cref="TenantNotFoundException">Thrown if the tenant does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the tenant already has a terminal or if the request is invalid.</exception>
    public async Task<TerminalResponseDto> Handle(CreateTerminalCommand request,
        CancellationToken cancellationToken)
    {
        var tenantRepository = _unitOfWork.TenantRepository;
        var terminalRepository = _unitOfWork.TerminalRepository;

        var tenant = await tenantRepository.GetByIdAsync(request.TerminalRequest.TenantId);
        if (tenant == null)
        {
            throw new TenantNotFoundException(request.TerminalRequest.TenantId);
        }

        // Check if the tenant already has a terminal
        var existingTerminal = await terminalRepository.GetByTenantIdAsync(request.TerminalRequest.TenantId);
        if (existingTerminal != null)
        {
            _logger.LogWarning("Tenant {TenantId} already has terminal {TerminalId}",
                request.TerminalRequest.TenantId, existingTerminal.TerminalId);
            // Consider using a more specific exception or error Response
            throw new InvalidOperationException(
                $"Tenant '{request.TerminalRequest.TenantId}' already has an associated terminal '{existingTerminal.TerminalId}'.");
        }

        var terminal = new Terminal(
            request.TerminalRequest.Name,
            request.TerminalRequest.TenantId,
            request.TerminalRequest.SecretKey
        );

        try
        {
            await terminalRepository.AddAsync(terminal);

            var secretName = $"tenant/{terminal.TenantId}/terminal/{terminal.TerminalId}/secretkey";
            var createSecretCommand = new CreateSecretCommand(
                secretName,
                request.TerminalRequest.SecretKey,
                $"Secret key for terminal {terminal.TerminalId}",
                terminal.TerminalId
            );

            await _awsSecretManagerService.CreateSecretAsync(createSecretCommand, cancellationToken);
            _logger.LogInformation("Terminal {TerminalId} created for tenant {TenantId}",
                terminal.TerminalId, terminal.TenantId);
            return _mapper.Map<TerminalResponseDto>(terminal);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create terminal for tenant {TenantId}, name {TerminalName}",
                request.TerminalRequest.TenantId, request.TerminalRequest.Name);
            throw;
        }
    }
}
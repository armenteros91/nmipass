using Amazon.SecretsManager.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Interfaces;

namespace ThreeTP.Payment.Application.Commands.AwsSecrets
{
    public class CreateSecretCommandHandler
        : IRequestHandler<CreateSecretCommand, CreateSecretResponse>
    {
        private readonly IAwsSecretManagerService _awsSecretManagerService;
        private readonly ILogger<CreateSecretCommandHandler> _logger;

        public CreateSecretCommandHandler(
            IAwsSecretManagerService awsSecretManagerService,
            ILogger<CreateSecretCommandHandler> logger)
        {
            _awsSecretManagerService = awsSecretManagerService ?? throw new ArgumentNullException(nameof(awsSecretManagerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CreateSecretResponse> Handle(
            CreateSecretCommand createSecretCommand,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(createSecretCommand);
            try
            {
                _logger.LogInformation("Creating AWS secret: {SecretName}", createSecretCommand.Name);
                var response = await _awsSecretManagerService.CreateSecretAsync(createSecretCommand, cancellationToken);
                _logger.LogInformation("Secret {SecretName} created successfully", createSecretCommand.Name);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AWS secret: {SecretName}", createSecretCommand.Name);
                throw;
            }
            
        }
    }
}